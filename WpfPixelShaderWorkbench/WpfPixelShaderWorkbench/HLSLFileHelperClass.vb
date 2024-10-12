Public Class HLSLFileHelperClass

    Public Function GetCompiledFileFullName() As String
        Return CompiledFileLocation & "\" & FileNameWithoutExtension & ExtensionOfCOmpiledFile
    End Function

    Public Function GetSourceFileFullName() As String
        Return SourceFileLocation & "\" & FileNameWithoutExtension & ExtensionOfSourceFile
    End Function

    Private pGetFileAsATextString As String = ""
    Public Property GetFileAsATextString() As String
        Get
            If pGetFileAsATextString = "" Then
                Return IO.File.ReadAllText(GetSourceFileFullName)
            Else
                Return pGetFileAsATextString
            End If

        End Get
        Set(ByVal value As String)
            pGetFileAsATextString = value
        End Set
    End Property

    Private pGeneretedCSCode As String
    Public Property GeneretedCSCode() As String
        Get
            Return pGeneretedCSCode
        End Get
        Set(ByVal value As String)
            pGeneretedCSCode = value
        End Set
    End Property

    Private pFileNameWithoutExtension As String
    Public Property FileNameWithoutExtension() As String
        Get
            Return pFileNameWithoutExtension
        End Get
        Set(ByVal value As String)
            pFileNameWithoutExtension = value
        End Set
    End Property

    Private pExtensionOfSourceFile As String = ".fx"
    Public Property ExtensionOfSourceFile() As String
        Get
            Return pExtensionOfSourceFile
        End Get
        Set(ByVal value As String)
            pExtensionOfSourceFile = value
        End Set
    End Property

    Private pExtensionOfCOmpiledFile As String = ".ps"
    Public Property ExtensionOfCOmpiledFile() As String
        Get
            Return pExtensionOfCOmpiledFile
        End Get
        Set(ByVal value As String)
            pExtensionOfCOmpiledFile = value
        End Set
    End Property

    Private pSourceFileLocation As String
    Public Property SourceFileLocation() As String
        Get
            Return pSourceFileLocation
        End Get
        Set(ByVal value As String)
            pSourceFileLocation = value
        End Set
    End Property

    Private pCompiledFileLocation As String
    Public Property CompiledFileLocation() As String
        Get
            Return pCompiledFileLocation
        End Get
        Set(ByVal value As String)
            pCompiledFileLocation = value
        End Set
    End Property

    Private pHLSLEntryPoint As String = "main"
    Public Property HLSLEntryPoint() As String
        Get
            Return pHLSLEntryPoint
        End Get
        Set(ByVal value As String)
            pHLSLEntryPoint = value
        End Set
    End Property

    Private pShaderCompilerVersion As ShaderVersion = ShaderVersion.ps_3_0
    Public Property ShaderCompilerVersion() As ShaderVersion
        Get
            Return pShaderCompilerVersion
        End Get
        Set(ByVal value As ShaderVersion)
            pShaderCompilerVersion = value
        End Set
    End Property

    Public Enum ShaderVersion
        ps_2_0
        ps_3_0
        ps_4_0
        ps_4_1
        ps_5_0
    End Enum



End Class
