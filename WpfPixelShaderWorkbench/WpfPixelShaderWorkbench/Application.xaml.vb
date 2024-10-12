Imports System.IO
Imports WpfPixelShaderWorkbench
Imports System.CodeDom
Imports System.CodeDom.Compiler

Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.
    Public CompilerHelper As New FXC_CompileHelper

    Public Sub New()
        CompileTheFxFiles()
    End Sub

    Private Overloads Sub Application_Startup(ByVal sender As Object, ByVal e As StartupEventArgs)

        ' Create main application window, starting minimized if specified
        Dim mainWindow As New MainWindow(CompilerHelper)
        mainWindow.Show()
    End Sub

    Private Sub CompileTheFxFiles()

        'Get the folder where the exe file is currently compiled
        Dim ExeFileDirectory As String = AppDomain.CurrentDomain.BaseDirectory

        'The name of the folder that will be opened or created at the exe folder location
        Dim FolderNameForTheCompiledPsFiles As String = "ShaderFiles"

        ' Create the Directory were the shader files will be
        Dim FolderWithCompiledShaderFiles As String = ExeFileDirectory & FolderNameForTheCompiledPsFiles

        ' If it dosn't exitst creat it
        If (Not IO.Directory.Exists(FolderWithCompiledShaderFiles)) Then
            IO.Directory.CreateDirectory(FolderWithCompiledShaderFiles)
        End If

        'Find the resource folder where the uncompiled fx filer are
        Dim ShaderSourceFiles As String = IO.Path.Combine(GetParantDirectory(ExeFileDirectory, 2), "ShaderSourceFiles")

        ' Get all teh uncopiled files in the folder
        Dim directoryEntries As String() = System.IO.Directory.GetFileSystemEntries(ShaderSourceFiles, "*.fx")

        ' Making sure the fx files are stored in an encoding
        ' that fxc understands. (It does not understadn the default Visual studio encoding!)
        For Each file As String In directoryEntries

            'Open the .fx file
            Dim OpenFxFileInTextFormat() As String = IO.File.ReadAllLines(file)

            'Makeign sure its stored in a format that the fxc compiler understands
            IO.File.WriteAllLines(file, OpenFxFileInTextFormat, Text.Encoding.ASCII)
        Next

        '   Dim CompilerHelper As New FXC_CompileHelper

        For Each FileName As String In directoryEntries
            ' Store the file in a FileInfo
            Dim TempFileInfo As New IO.FileInfo(FileName)

            ' Get the file name
            Dim FileNamsWithoutEx As String = IO.Path.GetFileNameWithoutExtension(TempFileInfo.Name)

            Dim f As New HLSLFileHelperClass
            f.FileNameWithoutExtension = FileNamsWithoutEx
            f.CompiledFileLocation = FolderWithCompiledShaderFiles
            f.SourceFileLocation = ShaderSourceFiles

            Dim rr As New EffectClassToShader
            Dim g As New ShaderModel
            g = rr.ParseShader(f.GetSourceFileFullName, f.GetFileAsATextString)

            'Dim provider As New Microsoft.CSharp.CSharpCodeProvider()
            Dim provider As New Microsoft.VisualBasic.VBCodeProvider()
            '    provider.GenerateCodeFromCompileUnit(program, indentedTextWriter, New CodeGeneratorOptions() With {.BracingStyle = "C"})


            Dim fg As New CreatePixelShaderClass
            f.GeneretedCSCode = fg.GetSourceText(provider, g, True)

            CompilerHelper.FilesToCompile.Add(f)
        Next




        CompilerHelper.CompileWithDLL()
    End Sub

    Private Function GetParantDirectory(ByVal FolderNAme As String, Optional ByVal ParentNumber As Integer = 1) As String

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


End Class
