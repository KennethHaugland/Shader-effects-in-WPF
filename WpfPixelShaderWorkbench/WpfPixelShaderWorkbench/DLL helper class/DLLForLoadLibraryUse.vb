Imports System.Runtime.InteropServices

Module DLLForLoadLibraryUse
    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3DCompileFromFile(<MarshalAs(UnmanagedType.LPWStr)> pFilename As String,
                                                          pDefines As IntPtr,
                                                          pInclude As IntPtr,
                                                          <MarshalAs(UnmanagedType.LPStr)> pEntrypoint As String,
                                                          <MarshalAs(UnmanagedType.LPStr)> pTarget As String,
                                                          flags1 As Integer,
                                                          flags2 As Integer,
                                                          ByRef ppCode As ID3DBlob,
                                                          ByRef ppErrorMsgs As ID3DBlob) As Integer

    <UnmanagedFunctionPointer(CallingConvention.StdCall)>
    Public Delegate Function D3DCompile(<MarshalAs(UnmanagedType.LPStr)> pSrcData As String,
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

End Module
