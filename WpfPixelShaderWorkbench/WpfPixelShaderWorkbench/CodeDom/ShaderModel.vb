
Public Class ShaderModel
    Public Property ShaderFileName() As String
        Get
            Return m_ShaderFileName
        End Get
        Set(value As String)
            m_ShaderFileName = value
        End Set
    End Property
    Private m_ShaderFileName As String

    Public Property GeneratedClassName() As String
        Get
            Return m_GeneratedClassName
        End Get
        Set(value As String)
            m_GeneratedClassName = value
        End Set
    End Property
    Private m_GeneratedClassName As String

    Public Property GeneratedNamespace() As String
        Get
            Return m_GeneratedNamespace
        End Get
        Set(value As String)
            m_GeneratedNamespace = value
        End Set
    End Property
    Private m_GeneratedNamespace As String

    Public Property Description() As String
        Get
            Return m_Description
        End Get
        Set(value As String)
            m_Description = value
        End Set
    End Property
    Private m_Description As String

    Public Property Registers() As List(Of ShaderModelConstantRegister)
        Get
            Return m_Registers
        End Get
        Set(value As List(Of ShaderModelConstantRegister))
            m_Registers = value
        End Set
    End Property
    Private m_Registers As List(Of ShaderModelConstantRegister)
End Class


