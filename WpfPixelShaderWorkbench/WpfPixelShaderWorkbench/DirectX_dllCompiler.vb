
Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.InteropServices

Class ShaderCompiler
    Implements INotifyPropertyChanged

    Public Shared FxDllCompiler As IntPtr
    Public Shared pAddressOfFxByteCompiler As IntPtr
    Public Shared pAddressOfFxBFileompiler As IntPtr
    Public Shared pFxByteStreamCompilation As DLLForLoadLibraryUse.D3DCompile
    Public Shared pFxFileCompilation As DLLForLoadLibraryUse.D3DCompileFromFile

    Public Shared DllFilesLocation As String

    Public Shared Sub FreeDlls()
        Dim result As Boolean = NativeDLLMethods.FreeLibrary(FxDllCompiler)
    End Sub

    Public Shared Sub SetUpDlls()

        If IntPtr.Size = 4 Then
            FxDllCompiler = NativeDLLMethods.LoadLibrary(IO.Path.Combine(DllFilesLocation, "d3dcompiler_47_32bit.dll"))
        Else
            FxDllCompiler = NativeDLLMethods.LoadLibrary(IO.Path.Combine(DllFilesLocation, "d3dcompiler_47_64bit.dll"))
        End If

        If FxDllCompiler = IntPtr.Zero Then
            MessageBox.Show("Could not load the DLL file")
        End If

        pAddressOfFxByteCompiler = NativeDLLMethods.GetProcAddress(FxDllCompiler, "D3DCompile")
        If pAddressOfFxByteCompiler = IntPtr.Zero Then
            MessageBox.Show("Could not locate the function D3DCompile in the DLL")
        End If

        pAddressOfFxBFileompiler = NativeDLLMethods.GetProcAddress(FxDllCompiler, "D3DCompileFromFile")
        If pAddressOfFxBFileompiler = IntPtr.Zero Then
            MessageBox.Show("Could not locate the function D3DCompileFromFile in the DLL")
        End If


        pFxByteStreamCompilation = Marshal.GetDelegateForFunctionPointer(pAddressOfFxByteCompiler, GetType(DLLForLoadLibraryUse.D3DCompile))
        pFxFileCompilation = Marshal.GetDelegateForFunctionPointer(pAddressOfFxBFileompiler, GetType(DLLForLoadLibraryUse.D3DCompileFromFile))
    End Sub

    Public Sub CompileTextString(ByVal file As HLSLFileHelperClass)

        Dim s As String = file.GetFileAsATextString

        Dim pSrcData As String = s
        Dim SrcDataSize As Integer = s.Length

        Dim pSourceName As String = file.FileNameWithoutExtension
        Dim pDefines As IntPtr = IntPtr.Zero
        Dim pInclude As IntPtr = IntPtr.Zero

        Dim pEntrypoint As String = file.HLSLEntryPoint
        Dim pTarget As String = file.ShaderCompilerVersion.ToString

        Dim flags1 As Integer = 0
        Dim flags2 As Integer = 0
        Dim ppCode As DLLEntryPointModule.ID3DBlob = Nothing
        Dim ppErrorMsgs As DLLEntryPointModule.ID3DBlob = Nothing


        Dim CompileResult As Integer = 0

        If FxDllCompiler = IntPtr.Zero Then
            Dim ExeFileDirectory As String = AppDomain.CurrentDomain.BaseDirectory
            DllFilesLocation = IO.Path.Combine(GetParantDirectory(ExeFileDirectory, 2), "DLLs")
            SetUpDlls()
        End If


        CompileResult = pFxByteStreamCompilation(pSrcData,
                                                 SrcDataSize,
                                                 pSourceName,
                                                 pDefines,
                                                 pInclude,
                                                 pEntrypoint,
                                                 pTarget,
                                                 flags1,
                                                 flags2,
                                                 ppCode,
                                                 ppErrorMsgs)

        If CompileResult <> 0 Then
            Dim errors As IntPtr = ppErrorMsgs.GetBufferPointer()
            Dim size As Integer = ppErrorMsgs.GetBufferSize()

            ErrorText = Marshal.PtrToStringAnsi(errors)

            IsCompiled = False
        Else
            If ppErrorMsgs IsNot Nothing Then
                Dim errors As IntPtr = ppErrorMsgs.GetBufferPointer()
                Dim size As Integer = ppErrorMsgs.GetBufferSize()

                HasWarnings = True
                WarningText = Marshal.PtrToStringAnsi(errors)
            End If

            IsCompiled = True
            Dim psPath = file.GetCompiledFileFullName
            Dim pCompiledPs As IntPtr = ppCode.GetBufferPointer()
            Dim compiledPsSize As Integer = ppCode.GetBufferSize()

            Dim compiledPs = New Byte(compiledPsSize - 1) {}
            Marshal.Copy(pCompiledPs, compiledPs, 0, compiledPs.Length)
            Using psFile = IO.File.Open(psPath, FileMode.Create, FileAccess.Write)
                psFile.Write(compiledPs, 0, compiledPs.Length)
            End Using
        End If

        If ppCode IsNot Nothing Then
            Marshal.ReleaseComObject(ppCode)
        End If
        ppCode = Nothing

        If ppErrorMsgs IsNot Nothing Then
            Marshal.ReleaseComObject(ppErrorMsgs)
        End If
        ppErrorMsgs = Nothing


    End Sub

    Public Sub Compile(ByVal File As HLSLFileHelperClass)
        Dim pFilename As String = File.GetSourceFileFullName
        Dim pDefines As IntPtr = IntPtr.Zero
        Dim pInclude As IntPtr = IntPtr.Zero

        Dim pEntrypoint As String = File.HLSLEntryPoint
        Dim pTarget As String = File.ShaderCompilerVersion.ToString

        Dim flags1 As Integer = 0
        Dim flags2 As Integer = 0
        Dim ppCode As DLLEntryPointModule.ID3DBlob = Nothing
        Dim ppErrorMsgs As DLLEntryPointModule.ID3DBlob = Nothing

        Dim CompileResult As Integer = 0

        If FxDllCompiler = IntPtr.Zero Then
            Dim ExeFileDirectory As String = AppDomain.CurrentDomain.BaseDirectory
            DllFilesLocation = IO.Path.Combine(GetParantDirectory(ExeFileDirectory, 2), "DLLs")
            SetUpDlls()
        End If

        CompileResult = pFxFileCompilation(pFilename,
                                           pDefines,
                                           pInclude,
                                           pEntrypoint,
                                           pTarget,
                                           flags1,
                                           flags2,
                                           ppCode,
                                           ppErrorMsgs)

        If CompileResult <> 0 Then
            Dim errors As IntPtr = ppErrorMsgs.GetBufferPointer()
            Dim size As Integer = ppErrorMsgs.GetBufferSize()

            ErrorText = Marshal.PtrToStringAnsi(errors)

            IsCompiled = False
        Else
            If ppErrorMsgs IsNot Nothing Then
                Dim errors As IntPtr = ppErrorMsgs.GetBufferPointer()
                Dim size As Integer = ppErrorMsgs.GetBufferSize()

                HasWarnings = True
                WarningText = Marshal.PtrToStringAnsi(errors)
            End If

            IsCompiled = True
            Dim psPath = File.GetCompiledFileFullName
            Dim pCompiledPs As IntPtr = ppCode.GetBufferPointer()
            Dim compiledPsSize As Integer = ppCode.GetBufferSize()

            Dim compiledPs = New Byte(compiledPsSize - 1) {}
            Marshal.Copy(pCompiledPs, compiledPs, 0, compiledPs.Length)
            Using psFile = IO.File.Open(psPath, FileMode.Create, FileAccess.Write)
                psFile.Write(compiledPs, 0, compiledPs.Length)
            End Using
        End If

        If ppCode IsNot Nothing Then
            Marshal.ReleaseComObject(ppCode)
        End If
        ppCode = Nothing

        If ppErrorMsgs IsNot Nothing Then
            Marshal.ReleaseComObject(ppErrorMsgs)
        End If
        ppErrorMsgs = Nothing
    End Sub

    Public Sub Reset()
        ErrorText = "not compiled"
    End Sub
    Private _errorText As String
    Public Property ErrorText() As String
        Get
            Return _errorText
        End Get
        Set(value As String)
            _errorText = value
            RaiseNotifyChanged("ErrorText")
        End Set
    End Property

    Private _WarningText As String
    Public Property WarningText() As String
        Get
            Return _WarningText
        End Get
        Set(value As String)
            _WarningText = value
            RaiseNotifyChanged("WarningText")
        End Set
    End Property

    Private _isCompiled As Boolean
    Public Property IsCompiled() As Boolean
        Get
            Return _isCompiled
        End Get
        Set(value As Boolean)
            _isCompiled = value
            RaiseNotifyChanged("IsCompiled")
        End Set
    End Property

    Private _HasWarnings As Boolean
    Public Property HasWarnings() As Boolean
        Get
            Return _HasWarnings
        End Get
        Set(value As Boolean)
            _HasWarnings = value
            RaiseNotifyChanged("HasWarnings")
        End Set
    End Property

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

    Private Sub RaiseNotifyChanged(propName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propName))
    End Sub

    Public Event PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

End Class


