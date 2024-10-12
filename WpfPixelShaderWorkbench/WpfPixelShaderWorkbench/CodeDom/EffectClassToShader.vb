Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Media.Media3D
Imports System.IO
Imports System.Text
Imports System.Globalization

Public Class EffectClassToShader

    ' Regular expression that matches a comment from double-slash to end-of-line (but not a triple-slash comment):
    Private ReadOnly CommentRegex As New Regex("(?<!/)//$|(?<!/)//[^/].*?$", RegexOptions.Compiled Or RegexOptions.Multiline)

    ' Patterns that match special triple-slash comments in the header:
    Private Const ClassPattern As String = "<class>(?<class>.*)</class>"
    Private Const NamespacePattern As String = "<namespace>(?<namespace>.*)</namespace>"
    Private Const DescriptionPattern As String = "<description>(?<description>.*)</description>"
    Private Const TargetPattern As String = "<target>(?<target>.*)</target>"
    Private Const HeaderCommentPattern As String = "^///\s*(" + ClassPattern + "|" + NamespacePattern + "|" + DescriptionPattern + "|" + TargetPattern + ")\s*?$\s*"
    Private ReadOnly HeaderCommentsRegex As New Regex("(" + HeaderCommentPattern + ")+", RegexOptions.Compiled Or RegexOptions.Multiline)

    ' Patterns that match special triple-slash comments before each register declaration:
    Private Const SummaryPattern As String = "<summary>(?<summary>.*)</summary>"
    Private Const TypePattern As String = "<type>(?<type>.*)</type>"
    Private Const MinValuePattern As String = "<minValue>(?<minValue>.*)</minValue>"
    Private Const MaxValuePattern As String = "<maxValue>(?<maxValue>.*)</maxValue>"
    Private Const DefaultValuePattern As String = "<defaultValue>(?<defaultValue>.*)</defaultValue>"
    Private Const SpecialCommentPattern As String = "^///\s*(" + SummaryPattern + "|" + TypePattern + "|" + MinValuePattern + "|" + MaxValuePattern + "|" + DefaultValuePattern + ")\s*?$\s*"
    Private Const SpecialCommentsPattern As String = "(" + SpecialCommentPattern + ")*"

    ' Patterns used in a constant register declaration in HLSL:
    Private Const RegisterTypePattern As String = "(?<registerType>\w+?)"
    Private Const RegisterNamePattern As String = "(?<registerName>\w+)"
    Private Const RequiredWhitespacePattern As String = "\s+"
    Private Const OptionalWhitespacePattern As String = "\s*"
    Private Const RegisterConstantNumberPattern As String = "[CcSs](?<registerNumber>\d+)"
    Private Const InitializerValuePattern As String = "(?<initializerValue>[^;]+)"
    Private Const OptionalInitializerPattern As String = "(?<initializer>=" + OptionalWhitespacePattern + InitializerValuePattern + OptionalWhitespacePattern + ")?"

    ' Regular expression that matches an entire constant register declaration, including the preceding special comments:
    Private ReadOnly RegisterConstantDeclarationRegex As New Regex(SpecialCommentsPattern + RegisterTypePattern + RequiredWhitespacePattern + RegisterNamePattern + OptionalWhitespacePattern + ":" + OptionalWhitespacePattern + "register" + OptionalWhitespacePattern + "\(" + OptionalWhitespacePattern + RegisterConstantNumberPattern + OptionalWhitespacePattern + "\)" + OptionalWhitespacePattern + OptionalInitializerPattern + ";", RegexOptions.Compiled Or RegexOptions.Multiline)

    ''' <summary>
    ''' Returns a shader model constructed from the information found in the
    ''' given pixel shader file.
    ''' </summary>
    Public Function ParseShader(shaderFileName As String, shaderText As String) As ShaderModel
        ' Remove all double-slash comments (but not triple-slash comments).
        ' This helps us avoid parsing register declarations that are commented out.
        shaderText = CommentRegex.Replace(shaderText, [String].Empty)

        ' Find all header comments.
        Dim headerMatch As Match = HeaderCommentsRegex.Match(shaderText)

        ' Determine the class name, namespace, description, and target platform.
        Dim defaultClassName As String = Path.GetFileNameWithoutExtension(shaderFileName)
        defaultClassName = [Char].ToUpperInvariant(defaultClassName(0)) + defaultClassName.Substring(1) + "Effect"
        Dim className As String = GetValidId(headerMatch.Groups("class").Value, defaultClassName, False)
        '   Dim namespaceName As String = GetValidId(headerMatch.Groups("namespace").Value, Settings.[Default].GeneratedNamespace, True)
        Dim description As String = If(headerMatch.Groups("description").Success, headerMatch.Groups("description").Value, Nothing)
        '    Dim targetFrameworkName As String = If(headerMatch.Groups("target").Success, headerMatch.Groups("target").Value, Settings.[Default].TargetFramework)

        ' Find all register declarations.
        Dim matches As MatchCollection = RegisterConstantDeclarationRegex.Matches(shaderText)

        ' Create a list of shader model constant registers.
        Dim registers As New List(Of ShaderModelConstantRegister)()
        For Each match As Match In matches
            Dim register As ShaderModelConstantRegister = CreateRegister(match)
            If register IsNot Nothing Then
                registers.Add(register)
            End If
        Next

        ' Return a new shader model.
        Return New ShaderModel() With { _
             .ShaderFileName = shaderFileName, _
             .GeneratedClassName = className, _
             .Description = description, _
             .Registers = registers _
        }
    End Function

    ''' <summary>
    ''' Returns a ShaderModelConstantRegister object with the information contained in
    ''' the given regular expression match.
    ''' </summary>
    Private Function CreateRegister(match As Match) As ShaderModelConstantRegister
        Dim register As ShaderModelConstantRegister = Nothing

        ' Figure out the .NET type that corresponds to the register type.
        Dim registerTypeInHLSL As String = match.Groups("registerType").Value
        Dim registerType As Type = GetRegisterType(registerTypeInHLSL)
        If registerType IsNot Nothing Then
            ' See if the user prefers to specify a different type in a comment.
            If match.Groups("type").Success Then
                OverrideTypeIfAllowed(match.Groups("type").Value, registerType)
            End If

            ' Capitalize the first letter of the variable name.  Leave the rest alone.
            Dim registerName As String = match.Groups("registerName").Value
            registerName = [Char].ToUpperInvariant(registerName(0)) + registerName.Substring(1)

            ' Get the register number and the optional summary comment.
            Dim registerNumber As Integer = Int32.Parse(match.Groups("registerNumber").Value)

            Dim GPUType As Char
            Dim GPUTypeNumber As Integer
            SetGPUREgisterType(match.Value, GPUType, GPUTypeNumber)

            If GetType(Brush).IsAssignableFrom(registerType) AndAlso (registerNumber = 0) Then
                Return Nothing
            End If
            ' ignore the implicit input sampler
            Dim summary As String = match.Groups("summary").Value

            ' Get the standard min, max, and default value for the register type.
            Dim minValue As New Object
            minValue = Nothing

            Dim maxValue As New Object
            maxValue = Nothing

            Dim defaultValue As New Object
            defaultValue = Nothing

            GetStandardValues(registerType, minValue, maxValue, defaultValue)

            ' Allow the user to override the defaults with values from their comments.
            ConvertValue(match.Groups("minValue").Value, registerType, minValue)
            ConvertValue(match.Groups("maxValue").Value, registerType, maxValue)
            ConvertValue(match.Groups("defaultValue").Value, registerType, defaultValue)

            If defaultValue Is Nothing Then
                GetStandardValues(registerType, minValue, maxValue, defaultValue)
            End If


            ' And if the user specified an initializer for the register value in HLSL,
            ' that value overrides any other default value.
            If match.Groups("initializer").Success Then
                    ParseInitializerValue(match.Groups("initializerValue").Value, registerType, defaultValue)
                End If

                ' Create a structure to hold the register information.
                register = New ShaderModelConstantRegister(registerName, registerType, registerNumber, summary, minValue, maxValue,
                    defaultValue, GPUType, GPUTypeNumber)
            End If
            Return register
    End Function

    Private Sub SetGPUREgisterType(ByVal Input As String, ByRef GPUType As Char, ByRef GPUTypeNumber As Integer)
        If Input.Contains(":") Then
            Dim str As String = Input.Split(":")(1)
            Dim StartIndex, EndIndex As Integer
            StartIndex = str.IndexOf("(")
            EndIndex = str.IndexOf(")")

            GPUType = str(StartIndex + 1)
            GPUTypeNumber = Integer.Parse(str.Substring(StartIndex + 2, EndIndex - StartIndex - 2))
        End If
    End Sub


    ''' <summary>
    ''' Returns the CLR type used to represent the given HLSL register type.
    ''' </summary>
    Private Function GetRegisterType(registerTypeInHLSL As String) As Type
        Select Case registerTypeInHLSL.ToLower()
            Case "float", "float1"
                Return GetType(Double)
            Case "float2"
                Return GetType(Point)
            Case "float3"
                Return GetType(Point3D)
            Case "float4"
                Return GetType(Color)

            Case "sampler1d"
                Return GetType(Brush)

            Case "sampler2d"
                Return GetType(Brush)
        End Select
        Return Nothing
    End Function

    ''' <summary>
    ''' Replaces the register type with the desired type, if they are compatible.
    ''' </summary>
    Private Sub OverrideTypeIfAllowed(desiredTypeName As String, ByRef registerType As Type)
        Select Case desiredTypeName
            Case "float", "Single"
                If registerType = GetType(Double) Then
                    registerType = GetType(Single)
                End If
                Exit Select
            Case "Size"
                If registerType = GetType(Point) Then
                    registerType = GetType(Size)
                End If
                Exit Select
            Case "Vector"
                If registerType = GetType(Point) Then
                    registerType = GetType(Vector)
                End If
                Exit Select
            Case "Vector3D"
                registerType = GetType(Vector3D)
                Exit Select
            Case "Point4D"
                registerType = GetType(Point4D)
                Exit Select
        End Select
    End Sub

    ''' <summary>
    ''' Sets the out parameters to the standard min, max, and default values for the given type.
    ''' </summary>
    Private Sub GetStandardValues(ByVal registerType As Type, ByRef minValue As Object, ByRef maxValue As Object, ByRef defaultValue As Object)
        If registerType = GetType(Double) Then
            minValue = 0.0
            maxValue = 1.0
            defaultValue = 0.0
        ElseIf registerType = GetType(Single) Then
            minValue = 0.0F
            maxValue = 1.0F
            defaultValue = 0.0F
        ElseIf registerType = GetType(Point) Then
            minValue = New Point(0, 0)
            maxValue = New Point(1, 1)
            defaultValue = New Point(0, 0)
        ElseIf registerType = GetType(Size) Then
            minValue = New Size(0, 0)
            maxValue = New Size(1, 1)
            defaultValue = New Size(0, 0)
        ElseIf registerType = GetType(Vector) Then
            minValue = New Vector(0, 0)
            maxValue = New Vector(1, 1)
            defaultValue = New Vector(0, 0)
        ElseIf registerType = GetType(Point3D) Then
            minValue = New Point3D(0, 0, 0)
            maxValue = New Point3D(1, 1, 1)
            defaultValue = New Point3D(0, 0, 0)
        ElseIf registerType = GetType(Vector3D) Then
            minValue = New Vector3D(0, 0, 0)
            maxValue = New Vector3D(1, 1, 1)
            defaultValue = New Vector3D(0, 0, 0)
        ElseIf registerType = GetType(Point4D) Then
            minValue = New Point4D(0, 0, 0, 0)
            maxValue = New Point4D(1, 1, 1, 1)
            defaultValue = New Point4D(0, 0, 0, 0)
        ElseIf registerType = GetType(Color) Then
            minValue = Color.FromArgb(0, 0, 0, 0)
            maxValue = Color.FromArgb(255, 255, 255, 255)
            defaultValue = Colors.Black
        Else
            minValue = InlineAssignHelper(maxValue, InlineAssignHelper(defaultValue, Nothing))
        End If
    End Sub

    ''' <summary>
    ''' Converts the given string value into a double, Point, Point3D, or Color.
    ''' </summary>
    Private Sub ConvertValue(valueText As String, type As Type, ByRef value As Object)
        Try
            If type = GetType(Double) Then
                value = [Double].Parse(valueText, NumberStyles.Any, NumberFormatInfo.InvariantInfo)
            ElseIf type = GetType(Single) Then
                value = [Single].Parse(valueText, NumberStyles.Any, NumberFormatInfo.InvariantInfo)
            ElseIf type = GetType(Point) Then
                value = Point.Parse(valueText)
            ElseIf type = GetType(Size) Then
                value = Size.Parse(valueText)
            ElseIf type = GetType(Vector) Then
                value = Vector.Parse(valueText)
            ElseIf type = GetType(Point3D) Then
                value = Point3D.Parse(valueText)
            ElseIf type = GetType(Vector3D) Then
                value = Vector3D.Parse(valueText)
            ElseIf type = GetType(Point4D) Then
                value = Point4D.Parse(valueText)
            ElseIf type = GetType(Color) Then
                value = ColorConverter.ConvertFromString(valueText)
            End If
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Parses the string representation of an HLSL float, float2, float3, or float4 value,
    ''' setting the final parameter to the corresponding double, Point, Point3D, or Color if possible.
    ''' </summary>
    Private Sub ParseInitializerValue(initializerValueText As String, registerType As Type, ByRef initializerValue As Object)
        Dim numbers As Double() = ParseNumbers(initializerValueText)
        If registerType = GetType(Double) AndAlso numbers.Length >= 1 Then
            initializerValue = numbers(0)
        ElseIf registerType = GetType(Single) AndAlso numbers.Length >= 1 Then
            initializerValue = CSng(numbers(0))
        ElseIf registerType = GetType(Point) AndAlso numbers.Length >= 2 Then
            initializerValue = New Point(numbers(0), numbers(1))
        ElseIf registerType = GetType(Size) AndAlso numbers.Length >= 2 Then
            initializerValue = New Size(Math.Max(0, numbers(0)), Math.Max(0, numbers(1)))
        ElseIf registerType = GetType(Vector) AndAlso numbers.Length >= 2 Then
            initializerValue = New Vector(numbers(0), numbers(1))
        ElseIf registerType = GetType(Point3D) AndAlso numbers.Length >= 3 Then
            initializerValue = New Point3D(numbers(0), numbers(1), numbers(2))
        ElseIf registerType = GetType(Vector3D) AndAlso numbers.Length >= 3 Then
            initializerValue = New Vector3D(numbers(0), numbers(1), numbers(2))
        ElseIf registerType = GetType(Point4D) AndAlso numbers.Length >= 4 Then
            initializerValue = New Point4D(numbers(0), numbers(1), numbers(2), numbers(3))
        ElseIf registerType = GetType(Color) AndAlso numbers.Length >= 4 Then
            initializerValue = Color.FromArgb(ConvertToByte(numbers(3)), ConvertToByte(numbers(0)), ConvertToByte(numbers(1)), ConvertToByte(numbers(2)))
        End If
    End Sub

    ''' <summary>
    ''' Parses the string representation of an HLSL float, float2, float3, or float4 value,
    ''' returning an array of doubles (possibly empty).
    ''' </summary>
    Private Function ParseNumbers(text As String) As Double()
        ' Get rid of any leading "float(", "float2(", "float3(", or "float4" and trailing ")".
        text = Regex.Replace(text, "^\s*float[1234]?\s*\((.*)\)\s*$", "$1")

        ' Split at commas.
        Dim textValues As String() = text.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)

        ' Parse the numbers.
        Dim numbers As New List(Of Double)()
        For Each textValue As String In textValues
            Dim trimmedValue As String = textValue.Trim()
            Dim number As Double
            If [Double].TryParse(trimmedValue, number) Then
                numbers.Add(number)
            Else
                Exit For
            End If
        Next
        Return numbers.ToArray()
    End Function

    ''' <summary>
    ''' Converts a double-precision floating point number between 0 and 1 to a byte.
    ''' </summary>
    Private Function ConvertToByte(number As Double) As Byte
        Return CByte(Math.Max(0, Math.Min(Math.Round(255 * number), 255)))
    End Function

    ''' <summary>
    ''' Returns a valid C# or Visual Basic identifier based on the given string.
    ''' </summary>
    Private Function GetValidId(firstChoice As String, secondChoice As String, isDotAllowed As Boolean) As String
        If [String].IsNullOrEmpty(firstChoice) Then
            firstChoice = secondChoice
        End If

        Dim stringBuilder As New StringBuilder()
        For Each c As Char In firstChoice
            If c = "_"c OrElse [Char].IsLetter(c) OrElse (stringBuilder.Length > 0 AndAlso ([Char].IsDigit(c) OrElse (c = "."c AndAlso isDotAllowed))) Then
                stringBuilder.Append(c)
            Else
                stringBuilder.Append("_"c)
            End If
        Next
        Return stringBuilder.ToString()
    End Function
    Private Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
        target = value
        Return value
    End Function
End Class





