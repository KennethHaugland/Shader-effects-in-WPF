
Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports System.Collections.Generic
Imports System.IO
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Media3D
Imports Microsoft.CSharp
Imports System.Globalization
Imports System.Windows.Data



'http://www.sirchristian.net/blog/2013/07/17/code-generation-with-c-part-3/
'http://blogs.msdn.com/b/bclteam/archive/2006/04/10/571096.aspx

NotInheritable Class CreatePixelShaderClass

    Public Function GetSourceText(currentProvider As CodeDomProvider, shaderModel As ShaderModel, includePixelShaderConstructor As Boolean) As String
        Return GenerateCode(currentProvider, BuildPixelShaderGraph(shaderModel, includePixelShaderConstructor))
    End Function

    Public Function CompileInMemory(code As String) As Assembly
        Dim provider = New CSharpCodeProvider(New Dictionary(Of String, String)() From { _
            {"CompilerVersion", "v3.5"} _
        })

        Dim options As New CompilerParameters()
        options.ReferencedAssemblies.Add("System.dll")
        options.ReferencedAssemblies.Add("System.Core.dll")
        options.ReferencedAssemblies.Add("WindowsBase.dll")
        options.ReferencedAssemblies.Add("PresentationFramework.dll")
        options.ReferencedAssemblies.Add("PresentationCore.dll")
        options.IncludeDebugInformation = False
        options.GenerateExecutable = False
        options.GenerateInMemory = True
        Dim results As CompilerResults = provider.CompileAssemblyFromSource(options, code)
        provider.Dispose()
        If results.Errors.Count = 0 Then
            Return results.CompiledAssembly
        Else
            Return Nothing
        End If
    End Function

    Private Function BuildPixelShaderGraph(shaderModel As ShaderModel, includePixelShaderConstructor As Boolean) As CodeCompileUnit
        ' Create a new CodeCompileUnit to contain
        ' the program graph.
        Dim codeGraph As New CodeCompileUnit()

        ' Create the namespace.
        Dim codeNamespace As CodeNamespace = AssignNamespacesToGraph(codeGraph, shaderModel.GeneratedNamespace)

        ' Create the appropriate constructor.
        Dim constructor As CodeConstructor = If(includePixelShaderConstructor, CreatePixelShaderConstructor(shaderModel), CreateDefaultConstructor(shaderModel))

        ' Declare a new type.
        Dim shader As New CodeTypeDeclaration() With { _
             .Name = shaderModel.GeneratedClassName
         }
        shader.BaseTypes.Add(New CodeTypeReference("ShaderEffect"))


        shader.Members.Add(constructor)
        shader.Members.Add(CreateSamplerDependencyProperty(shaderModel.GeneratedClassName, "Input", 0))
        shader.Members.Add(CreateCLRProperty("Input", GetType(Brush), Nothing))

        If Not [String].IsNullOrEmpty(shaderModel.Description) Then
            shader.Comments.Add(New CodeCommentStatement([String].Format("<summary>{0}</summary>", shaderModel.Description)))
        End If

        ' Add a dependency property and a CLR property for each of the shader's register variables
        For Each register As ShaderModelConstantRegister In shaderModel.Registers
            If register.GPURegisterType.ToString.ToUpper = "C" Then
                shader.Members.Add(CreateShaderRegisterDependencyProperty(shaderModel, register))
                shader.Members.Add(CreateCLRProperty(register.RegisterName, register.RegisterType, register.Description))
            Else
                shader.Members.Add(CreateSamplerDependencyProperty(shaderModel.GeneratedClassName, register.RegisterName, register.GPURegisterNumber))
                shader.Members.Add(CreateCLRProperty(register.RegisterName, GetType(Brush), Nothing))
            End If
        Next

        ' Add the new type to the namespace.
        codeNamespace.Types.Add(shader)

        Return codeGraph
    End Function

    Private Sub something()
        Dim myCodeTypeDecl As New CodeTypeDeclaration() With { _
           .Name = "MyClass"
       }
        myCodeTypeDecl.BaseTypes.Add(GetType(System.ComponentModel.INotifyPropertyChanged))

        Dim myEvent As New CodeMemberEvent()
        With myEvent
            .Name = "PropertyChanged"
            .Type = New CodeTypeReference(GetType(System.ComponentModel.PropertyChangedEventHandler))
            .Attributes = MemberAttributes.Public Or MemberAttributes.Final
            .ImplementationTypes.Add(GetType(System.ComponentModel.INotifyPropertyChanged))
        End With

        myCodeTypeDecl.Members.Add(myEvent)

        Dim myMethod As New CodeMemberMethod

        With myMethod
            .Name = "OnPropertyChanged"
            .Parameters.Add(New CodeParameterDeclarationExpression(GetType(String), "pPropName"))
            .ReturnType = New CodeTypeReference(GetType(Void))
            .Statements.Add(New CodeExpressionStatement(
                            New CodeDelegateInvokeExpression(
                                New CodeEventReferenceExpression(
                                    New CodeThisReferenceExpression(), "PropertyChanged"),
                                New CodeExpression() {
                                    New CodeThisReferenceExpression(),
                                    New CodeObjectCreateExpression(GetType(System.ComponentModel.PropertyChangedEventArgs),
                                                                   New CodeArgumentReferenceExpression("pPropName"))})))

            .Attributes = MemberAttributes.FamilyOrAssembly
        End With

        myCodeTypeDecl.Members.Add(myMethod)

        Dim myProperty As New CodeMemberProperty

        With myProperty
            .Name = "fldItemNr"
            .Attributes = MemberAttributes.Public Or MemberAttributes.Final
            .Type = New CodeTypeReference(GetType(String))
            .SetStatements.Add(New CodeAssignStatement(New CodeVariableReferenceExpression("m_fldItemNr"), New CodePropertySetValueReferenceExpression))
            .SetStatements.Add(New CodeExpressionStatement(New CodeMethodInvokeExpression(New CodeMethodReferenceExpression(New CodeThisReferenceExpression(), "OnPropertyChanged"), New CodeExpression() {New CodePrimitiveExpression("fldItemNr")})))
            .GetStatements.Add(New CodeMethodReturnStatement(New CodeVariableReferenceExpression("m_fldItemNr")))
        End With

        myCodeTypeDecl.Members.Add(myProperty)
    End Sub

    Private Function CreateSamplerDependencyProperty(className As String, propertyName As String, ByVal RegisterNumber As Integer) As CodeMemberField

        Dim RegisterDependencyProperty As New CodeMethodInvokeExpression
        Dim RegisterMethod As New CodeMethodReferenceExpression
        RegisterMethod.TargetObject = New CodeTypeReferenceExpression("ShaderEffect")
        RegisterMethod.MethodName = "RegisterPixelShaderSamplerProperty"

        RegisterDependencyProperty.Method = RegisterMethod
        RegisterDependencyProperty.Parameters.AddRange({New CodePrimitiveExpression(propertyName), New CodeTypeOfExpression(className), New CodePrimitiveExpression(RegisterNumber)})

        Dim result As New CodeMemberField
        result.Type = New CodeTypeReference("DependencyProperty")
        result.Name = String.Format("{0}Property", propertyName)
        result.Attributes = MemberAttributes.Public Or MemberAttributes.Static
        result.InitExpression = RegisterDependencyProperty

        Return result
    End Function

    Private Function CreateShaderRegisterDependencyProperty(shaderModel As ShaderModel, register As ShaderModelConstantRegister) As CodeMemberField

        Dim RegisterDependencyProperty As New CodeMethodInvokeExpression
        Dim RegisterMethod As New CodeMethodReferenceExpression
        RegisterMethod.TargetObject = New CodeTypeReferenceExpression("DependencyProperty")
        RegisterMethod.MethodName = "Register"

        RegisterDependencyProperty.Method = RegisterMethod

        Dim PropertyMetadataFunction As New CodeObjectCreateExpression
        PropertyMetadataFunction.CreateType = New CodeTypeReference("PropertyMetadata")
        PropertyMetadataFunction.Parameters.Add(CreateDefaultValue(register.DefaultValue))

        Dim PropertyMetadataCallback As New CodeMethodInvokeExpression
        PropertyMetadataCallback.Method = New CodeMethodReferenceExpression(Nothing, "PixelShaderConstantCallback")
        PropertyMetadataCallback.Parameters.Add(New CodePrimitiveExpression(register.RegisterNumber))
        PropertyMetadataFunction.Parameters.Add(PropertyMetadataCallback)

        RegisterDependencyProperty.Parameters.AddRange({New CodePrimitiveExpression(register.RegisterName), New CodeTypeOfExpression(register.RegisterType), New CodeTypeOfExpression(shaderModel.GeneratedClassName), PropertyMetadataFunction})

        Dim InitiateDependencyProperty As New CodeMemberField
        InitiateDependencyProperty.Type = New CodeTypeReference("DependencyProperty")
        InitiateDependencyProperty.Name = String.Format("{0}Property", register.RegisterName)
        InitiateDependencyProperty.Attributes = MemberAttributes.Public Or MemberAttributes.Static
        InitiateDependencyProperty.InitExpression = RegisterDependencyProperty

        Return InitiateDependencyProperty
    End Function

    Private Function CreateDefaultValue(defaultValue As Object) As CodeExpression
        If defaultValue Is Nothing Then
            Return New CodePrimitiveExpression(Nothing)
        Else
            Dim codeTypeReference As CodeTypeReference = CreateCodeTypeReference(defaultValue.GetType())
            If defaultValue.[GetType]().IsPrimitive Then
                Return New CodeCastExpression(codeTypeReference, New CodePrimitiveExpression(defaultValue))
            ElseIf TypeOf defaultValue Is Point OrElse TypeOf defaultValue Is Vector OrElse TypeOf defaultValue Is Size Then
                Dim point As Point = DirectCast(RegisterValueConverter.ConvertToUsualType(defaultValue), Point)
                Return New CodeObjectCreateExpression(codeTypeReference, New CodePrimitiveExpression(point.X), New CodePrimitiveExpression(point.Y))
            ElseIf TypeOf defaultValue Is Point3D OrElse TypeOf defaultValue Is Vector3D Then
                Dim point3D As Point3D = DirectCast(RegisterValueConverter.ConvertToUsualType(defaultValue), Point3D)
                Return New CodeObjectCreateExpression(codeTypeReference, New CodePrimitiveExpression(point3D.X), New CodePrimitiveExpression(point3D.Y), New CodePrimitiveExpression(point3D.Z))
            ElseIf TypeOf defaultValue Is Point4D Then
                Dim point4D As Point4D = DirectCast(defaultValue, Point4D)
                Return New CodeObjectCreateExpression(codeTypeReference, New CodePrimitiveExpression(point4D.X), New CodePrimitiveExpression(point4D.Y), New CodePrimitiveExpression(point4D.Z), New CodePrimitiveExpression(point4D.W))
            ElseIf TypeOf defaultValue Is Color Then
                Dim color As Color = DirectCast(defaultValue, Color)
                Return New CodeMethodInvokeExpression(New CodeTypeReferenceExpression(codeTypeReference), "FromArgb", New CodePrimitiveExpression(color.A), New CodePrimitiveExpression(color.R), New CodePrimitiveExpression(color.G), New CodePrimitiveExpression(color.B))
            Else
                Return New CodeDefaultValueExpression(codeTypeReference)
            End If
        End If
    End Function

    Private Function CreateCLRProperty(propertyName As String, type As Type, description As String) As CodeMemberProperty
        Dim [property] As New CodeMemberProperty() With { _
             .Name = propertyName, _
             .Type = CreateCodeTypeReference(type), _
             .Attributes = MemberAttributes.[Public] Or MemberAttributes.Final, _
             .HasGet = True,
             .HasSet = True
        }

        Dim GetValueCode As New CodeMethodInvokeExpression
        GetValueCode.Method = New CodeMethodReferenceExpression(New CodeThisReferenceExpression(), "GetValue")
        GetValueCode.Parameters.Add(New CodeVariableReferenceExpression([String].Format("{0}Property", propertyName)))

        Dim CastGetValueCode As New CodeCastExpression
        CastGetValueCode.TargetType = CreateCodeTypeReference(type)
        CastGetValueCode.Expression = GetValueCode

        Dim GetDependencyProperty As New CodeMethodReturnStatement
        GetDependencyProperty.Expression = CastGetValueCode
        [property].GetStatements.Add(GetDependencyProperty)

        Dim SetValue As New CodeMethodInvokeExpression
        SetValue.Method = New CodeMethodReferenceExpression(New CodeThisReferenceExpression(), "SetValue")
        SetValue.Parameters.AddRange({New CodeVariableReferenceExpression(propertyName & Convert.ToString("Property")),
                      New CodeVariableReferenceExpression("value")})
        [property].SetStatements.Add(SetValue)


        If Not [String].IsNullOrEmpty(description) Then
            [property].Comments.Add(New CodeCommentStatement([String].Format("<summary>{0}</summary>", description)))
        End If
        Return [property]
    End Function

    Private Function CreateCodeTypeReference(type As Type) As CodeTypeReference
        Return If(type.IsPrimitive, New CodeTypeReference(type), New CodeTypeReference(type.Name))
    End Function

    Private Function CreatePixelShaderConstructor(shaderModel As ShaderModel) As CodeConstructor
        ' Create a constructor that takes a PixelShader as its only parameter.
        Dim constructor As New CodeConstructor() With { _
             .Attributes = MemberAttributes.[Public]
        }

        Dim shaderRelativeUri As String = [String].Format("/{0};component/{1}.ps", shaderModel.GeneratedNamespace, shaderModel.GeneratedClassName)

        Dim CreateUri As New CodeObjectCreateExpression
        CreateUri.CreateType = New CodeTypeReference("Uri")
        CreateUri.Parameters.AddRange({New CodePrimitiveExpression(shaderRelativeUri), New CodeFieldReferenceExpression(New CodeTypeReferenceExpression("UriKind"), "Relative")})

        Dim ConnectUriSource As New CodeAssignStatement With {
                .Left = New CodeFieldReferenceExpression(New CodeThisReferenceExpression, "PixelShader.UriSource"),
                .Right = CreateUri}

        constructor.Statements.AddRange({New CodeAssignStatement() With {.Left = New CodePropertyReferenceExpression(New CodeThisReferenceExpression(), "PixelShader"),
                                                                         .Right = New CodeObjectCreateExpression(New CodeTypeReference("PixelShader"))},
                                         ConnectUriSource,
                                         New CodeSnippetStatement("")})


        constructor.Statements.Add(CreateUpdateMethod("Input"))

        For Each register As ShaderModelConstantRegister In shaderModel.Registers
            constructor.Statements.Add(CreateUpdateMethod(register.RegisterName))
        Next
        Return constructor
    End Function

    Private Function CreateDefaultConstructor(shaderModel As ShaderModel) As CodeConstructor
        ' Create a default constructor.
        Dim shaderRelativeUri As String = [String].Format("/{0};component/{1}.ps", shaderModel.GeneratedNamespace, shaderModel.GeneratedClassName)
        Dim constructor As New CodeConstructor()
        constructor.Attributes = MemberAttributes.Public

        Dim sg As New CodeVariableDeclarationStatement()
        sg.Type = New CodeTypeReference("PixelShader")
        sg.Name = "pixelShader"
        sg.InitExpression = New CodeObjectCreateExpression("PixelShader")

        Dim sg2 As New CodeAssignStatement()
        sg2.Left = New CodePropertyReferenceExpression(New CodeThisReferenceExpression(), "PixelShader")
        sg2.Right = New CodeArgumentReferenceExpression("pixelShader")


        Dim sg3 As New CodeSnippetStatement("")
        'CreateUpdateMethod("Input")
        constructor.Statements.AddRange({sg, sg2, sg3})
        constructor.Statements.Add(CreateUpdateMethod("Input"))

        For Each register As ShaderModelConstantRegister In shaderModel.Registers
            constructor.Statements.Add(CreateUpdateMethod(register.RegisterName))
        Next



        Return constructor
    End Function

    Private Function CreateUpdateMethod(propertyName As String) As CodeMethodInvokeExpression

        Dim result As New CodeMethodInvokeExpression() With { _
             .Method = New CodeMethodReferenceExpression(New CodeThisReferenceExpression(), "UpdateShaderValue")
        }

        result.Parameters.Add(New CodeVariableReferenceExpression(propertyName & "Property"))

        Return result
    End Function

    Private Function AssignNamespacesToGraph(codeGraph As CodeCompileUnit, namespaceName As String) As CodeNamespace
        ' Add imports to the global (unnamed) namespace.
        Dim globalNamespace As New CodeNamespace()
        globalNamespace.[Imports].AddRange({New CodeNamespaceImport("System"),
                                            New CodeNamespaceImport("System.Windows"),
                                            New CodeNamespaceImport("System.Windows.Media"),
                                            New CodeNamespaceImport("System.Windows.Media.Effects"),
                                            New CodeNamespaceImport("System.Windows.Media.Media3D")})

        codeGraph.Namespaces.Add(globalNamespace)

        ' Create a named namespace.
        Dim ns As New CodeNamespace(namespaceName)
        codeGraph.Namespaces.Add(ns)
        Return ns
    End Function

    Private Function GenerateCode(provider As CodeDomProvider, compileUnit As CodeCompileUnit) As String
        ' Generate source code using the code generator.
        Using writer As New StringWriter()
            Dim indentString As String = "" ' If(Settings.[Default].IndentUsingTabs, vbTab, [String].Format("{0," + Settings.[Default].IndentSpaces.ToString() + "}", " "))
            Dim options As New CodeGeneratorOptions() With { _
                 .IndentString = indentString, _
                 .BlankLinesBetweenMembers = True _
            }
            provider.GenerateCodeFromCompileUnit(compileUnit, writer, options)
            Dim text As String = writer.ToString()
            ' Fix up code: make static DP fields readonly, and use triple-slash or triple-quote comments for XML doc comments.
            If provider.FileExtension = "cs" Then
                text = text.Replace("public static DependencyProperty", "public static readonly DependencyProperty")
                text = Regex.Replace(text, "// <(?!/?auto-generated)", "/// <")
            ElseIf provider.FileExtension = "vb" Then
                text = text.Replace("Public Shared ", "Public Shared ReadOnly ")
                text = text.Replace("'<", "'''<")
            End If
            Return text
        End Using
    End Function
End Class


Public Class RegisterValueConverter
    Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        ' Convert double to float, Point to Vector, Point to Size, and Point3D to Vector3D.
        ' Leave anythign else unchanged.
        If targetType = GetType(Single) Then
            Return CSng(CDbl(value))
        ElseIf targetType = GetType(Vector) Then
            Dim temp As Point = DirectCast(value, Point)
            Dim res As New Vector With {.X = temp.X, .Y = temp.Y}
            Return res
        ElseIf targetType = GetType(Size) Then
            Dim p As Point = DirectCast(value, Point)
            Return New Size(Math.Max(0, p.X), Math.Max(0, p.Y))
        ElseIf targetType = GetType(Vector3D) Then
            Dim temp As Point3D = DirectCast(value, Point3D)
            Return New Vector3D With {.X = temp.X, .Y = temp.Y}
        End If
        Return value
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return ConvertToUsualType(value)
    End Function

    Public Shared Function ConvertToUsualType(value As Object) As Object
        ' Convert float to double, Vector to Point, Size to Point, and Vector3D to Point3D.
        ' Leave anything else unchanged.
        If value.[GetType]() = GetType(Single) Then
            Return CDbl(CSng(value))
        ElseIf value.[GetType]() = GetType(Vector) Then
            Dim temp As Vector = DirectCast(value, Vector)
            Return New Point With {.X = temp.X, .Y = temp.Y}
        ElseIf value.[GetType]() = GetType(Size) Then
            Dim temp As Size = DirectCast(value, Size)
            Return New Point With {.X = temp.Width, .Y = temp.Height}
        ElseIf value.[GetType]() = GetType(Vector3D) Then
            Dim temp As Vector3D = DirectCast(value, Vector3D)
            Return New Point3D With {.X = temp.X, .Y = temp.Y, .Z = temp.Z}
        End If
        Return value
    End Function
End Class


