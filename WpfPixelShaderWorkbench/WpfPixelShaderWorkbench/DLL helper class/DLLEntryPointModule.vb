Imports System.Runtime.InteropServices

Module DLLEntryPointModule
    <Guid("8BA5FB08-5195-40e2-AC58-0D989C3A0102"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Public Interface ID3DBlob
        <PreserveSig>
        Function GetBufferPointer() As IntPtr
        <PreserveSig>
        Function GetBufferSize() As Integer
    End Interface

    <DllImport("d3dcompiler_47.dll", CharSet:=CharSet.Auto)> _
    Public Function D3DCompileFromFile(<MarshalAs(UnmanagedType.LPWStr)> pFilename As String,
                                                          pDefines As IntPtr,
                                                          pInclude As IntPtr,
                                                          <MarshalAs(UnmanagedType.LPStr)> pEntrypoint As String,
                                                          <MarshalAs(UnmanagedType.LPStr)> pTarget As String,
                                                          flags1 As Integer,
                                                          flags2 As Integer,
                                                          ByRef ppCode As ID3DBlob,
                                                          ByRef ppErrorMsgs As ID3DBlob) As Integer
    End Function

    <DllImport("d3dcompiler_47.dll", CharSet:=CharSet.Auto)> _
    Public Function D3DCompile(<MarshalAs(UnmanagedType.LPStr)> pSrcData As String,
                                                         SrcDataSize As Integer,
                                                        <MarshalAs(UnmanagedType.LPStr)> pSourceName As String,
                                                         pDefines As IntPtr,
                                                         pInclude As IntPtr,
                                                        <MarshalAs(UnmanagedType.LPStr)> pEntrypoint As String,
                                                        <MarshalAs(UnmanagedType.LPStr)> pTarget As String,
                                                         flags1 As Integer,
                                                         flags2 As Integer,
                                                        ByRef ppCode As ID3DBlob,
                                                        ByRef ppErrorMsgs As ID3DBlob) As Integer
    End Function


End Module
