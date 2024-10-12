Public Class FXC_CompileHelper
    Implements ComponentModel.INotifyPropertyChanged

    Private pOnlyShowWarningsAndErrorCompileResult As Boolean = True
    Public Property OnlyShowWarningsAndErrorCompileResult() As Boolean
        Get
            Return pOnlyShowWarningsAndErrorCompileResult
        End Get
        Set(ByVal value As Boolean)
            pOnlyShowWarningsAndErrorCompileResult = value
        End Set
    End Property

    Private pFXCFileLocation As String = "C:\Program Files\Windows Kits\8.1\bin\x86\fxc.exe"
    Public Property FXCFileLocation() As String
        Get
            Return pFXCFileLocation
        End Get
        Set(ByVal value As String)
            pFXCFileLocation = value
        End Set
    End Property

    Private pFilesToCompile As New List(Of HLSLFileHelperClass)
    Public Property FilesToCompile() As List(Of HLSLFileHelperClass)
        Get
            Return pFilesToCompile
        End Get
        Set(ByVal value As List(Of HLSLFileHelperClass))
            pFilesToCompile = value
        End Set
    End Property

    Public Sub CompileWithFXC()
        Dim p As New Process()
        Dim info As New ProcessStartInfo()
        info.FileName = "cmd.exe"
        info.CreateNoWindow = True
        info.WindowStyle = ProcessWindowStyle.Hidden
        info.RedirectStandardInput = True
        info.UseShellExecute = False
        info.RedirectStandardOutput = True
        info.RedirectStandardError = True

        p.StartInfo = info

        p.Start()
        p.BeginOutputReadLine()
        p.BeginErrorReadLine()

        AddHandler p.OutputDataReceived, AddressOf NormalOutputHandler
        AddHandler p.ErrorDataReceived, AddressOf ErrorAndWarningOutputHandler

        Dim sw As IO.StreamWriter = p.StandardInput

        Dim result As String = ""
        For Each File As HLSLFileHelperClass In FilesToCompile
            CompileFile(sw, File)
        Next

        p.WaitForExit(1000)
    End Sub

    Public Sub CompileWithDLL()
        For Each File As HLSLFileHelperClass In FilesToCompile
            Dim g As New ShaderCompiler
            '  g.CompileWithFXC(File)
            g.CompileTextString(File)
            If Not g.IsCompiled Then
                OutPutString = Environment.NewLine & SplitUpCompilerWarningsAndErrors(g.ErrorText)
            Else
                If g.HasWarnings Then
                    OutPutString = Environment.NewLine & SplitUpCompilerWarningsAndErrors(g.WarningText)
                Else
                    OutPutString = Environment.NewLine & "The file " & File.FileNameWithoutExtension & " is compiled."
                End If
            End If
        Next
    End Sub

    Public OutpuitString As New Text.StringBuilder

    Public Property OutPutString As String
        Get
            Return OutpuitString.ToString()
        End Get
        Set(value As String)
            If (Not String.IsNullOrEmpty(value)) Then
                OutpuitString.Append(value)
                OnPropertyChanged("OutPutString")
            End If
        End Set
    End Property

    Private Sub ErrorAndWarningOutputHandler(ByVal sendingProcess As Object, outLine As DataReceivedEventArgs)
        If (Not String.IsNullOrEmpty(outLine.Data)) Then
            OutPutString = SplitUpCompilerWarningsAndErrors(outLine.Data)
        End If
    End Sub

    Private Sub NormalOutputHandler(ByVal sendingProcess As Object, outLine As DataReceivedEventArgs)
        If (Not String.IsNullOrEmpty(outLine.Data)) Then
            OutPutString = SucsessFXC(outLine.Data)
        End If
    End Sub

    Private Function SucsessFXC(ByVal str As String) As String
        Dim result As New Text.StringBuilder
        If Not OnlyShowWarningsAndErrorCompileResult Then
            If str.Contains("; see") Then
                Dim FileName As String

                Dim myDelims As String() = New String() {"; see"}
                FileName = IO.Path.GetFileNameWithoutExtension(str.Split(myDelims, StringSplitOptions.None)(1))
                result.AppendLine("The file " & FileName & " compiled")

            End If
        End If
        Return result.ToString
    End Function

    Private Function SplitUpCompilerWarningsAndErrors(ByVal str As String) As String
        Dim result As New Text.StringBuilder
        If str = "compilation failed; no code produced" Then
            Return result.ToString
        End If

        Dim int() As String = str.Split(":")
        Dim index As Integer
        For index = int(1).Length - 1 To 0 Step -1
            If int(1)(index) = "(" Then
                Exit For
            End If
        Next

        Dim FilePAth As String = int(0) & ":" & int(1).Substring(0, index)

        Dim f As String = IO.Path.GetFileNameWithoutExtension(FilePAth)
        If int(2).Contains("error") Then
            result.AppendLine("The file " & f & " was not compiled")
        Else
            result.AppendLine("The file " & f & " was compiled with:")
        End If

        result.AppendLine(int(2))

        If int.Length = 5 Then
            result.AppendLine(int(3) & ":" & int(4))
        Else
            result.AppendLine(int(3))
        End If
        If int(2).Contains("error") Then
            result.AppendLine("The error occured at line: " & int(1).Substring(index))
        Else
            result.AppendLine("The warning occured at line: " & int(1).Substring(index))
        End If


        Return result.ToString
    End Function


    Sub CompileFile(ByVal sw As IO.StreamWriter, ByVal sender As HLSLFileHelperClass)
        If sw.BaseStream.CanWrite Then
            Dim s As String = """" & FXCFileLocation & """ /T " & sender.ShaderCompilerVersion.ToString & " /E " & sender.HLSLEntryPoint & " /Fo""" & sender.GetCompiledFileFullName & """ """ & sender.GetSourceFileFullName & """"
            sw.WriteLine(s)
        End If
    End Sub

    Private Function GetParentDirectory(ByVal FolderNAme As String, Optional ByVal ParentNumber As Integer = 1) As String

        If ParentNumber = 0 Then
            Return FolderNAme
        End If

        Dim result As IO.DirectoryInfo
        Dim CurrentFolderNAme As String = FolderNAme
        For i As Integer = 1 To ParentNumber + 1
            result = IO.Directory.GetParent(CurrentFolderNAme)
            CurrentFolderNAme = result.FullName
        Next
        Return CurrentFolderNAme
    End Function

    Public Sub OnPropertyChanged(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New ComponentModel.PropertyChangedEventArgs(info))
    End Sub

    Public Event PropertyChanged(sender As Object, e As ComponentModel.PropertyChangedEventArgs) Implements ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class
