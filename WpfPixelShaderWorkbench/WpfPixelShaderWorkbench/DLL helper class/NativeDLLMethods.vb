Imports System.Runtime.InteropServices

Module NativeDLLMethods
    <DllImport("kernel32.dll")>
    Public Function LoadLibrary(dllToLoad As String) As IntPtr
    End Function

    <DllImport("kernel32.dll")>
    Public Function GetProcAddress(hModule As IntPtr, procedureName As String) As IntPtr
    End Function


    <DllImport("kernel32.dll")>
    Public Function FreeLibrary(hModule As IntPtr) As Boolean
    End Function
End Module
