
''' <summary>
'''  Contains the details for each register described in a HLSL shader file
''' </summary>
Public Class ShaderModelConstantRegister

    Public Sub New(registerName As String, registerType As Type, registerNumber As Integer, description As String, minValue As Object, maxValue As Object,
        defaultValue As Object, gPUType As Char, gPUTypeNumber As Integer)
        Me.RegisterName = registerName
        Me.RegisterType = registerType
        Me.RegisterNumber = registerNumber
        Me.Description = description
        Me.MinValue = minValue
        Me.MaxValue = maxValue
        Me.DefaultValue = defaultValue
        Me.GPURegisterType = gPUType
        Me.pGPURegisterNumber = gPUTypeNumber
    End Sub

#Region "Properties"

    Private pGPURegisterType As Char
    Public Property GPURegisterType() As Char
        Get
            Return pGPURegisterType
        End Get
        Set(ByVal value As Char)
            pGPURegisterType = value
        End Set
    End Property

    Private pGPURegisterNumber As Integer
    Public Property GPURegisterNumber() As Integer
        Get
            Return pGPURegisterNumber
        End Get
        Set(ByVal value As Integer)
            pGPURegisterNumber = value
        End Set
    End Property


    ''' <summary>
    ''' The name of this register variable.
    ''' </summary>
    Public Property RegisterName() As String
        Get
            Return m_RegisterName
        End Get
        Private Set(value As String)
            m_RegisterName = value
        End Set
    End Property
    Private m_RegisterName As String

    ''' <summary>
    '''  The .NET type of this register variable.
    ''' </summary>
    Public Property RegisterType() As Type
        Get
            Return m_RegisterType
        End Get
        Private Set(value As Type)
            m_RegisterType = value
        End Set
    End Property
    Private m_RegisterType As Type

    ''' <summary>
    ''' The register number of this register variable.
    ''' </summary>
    Public Property RegisterNumber() As Integer
        Get
            Return m_RegisterNumber
        End Get
        Private Set(value As Integer)
            m_RegisterNumber = value
        End Set
    End Property
    Private m_RegisterNumber As Integer

    ''' <summary>
    ''' The description of this register variable.
    ''' </summary>
    Public Property Description() As String
        Get
            Return m_Description
        End Get
        Private Set(value As String)
            m_Description = value
        End Set
    End Property
    Private m_Description As String

    ''' <summary>
    ''' The minimum value for this register variable.
    ''' </summary>
    Public Property MinValue() As Object
        Get
            Return m_MinValue
        End Get
        Private Set(value As Object)
            m_MinValue = value
        End Set
    End Property
    Private m_MinValue As Object

    ''' <summary>
    ''' The maximum value for this register variable.
    ''' </summary>
    Public Property MaxValue() As Object
        Get
            Return m_MaxValue
        End Get
        Private Set(value As Object)
            m_MaxValue = value
        End Set
    End Property
    Private m_MaxValue As Object

    ''' <summary>
    ''' The default value of this register variable.
    ''' </summary>
    Public Property DefaultValue() As Object
        Get
            Return m_DefaultValue
        End Get
        Private Set(value As Object)
            m_DefaultValue = value
        End Set
    End Property
    Private m_DefaultValue As Object

    ''' <summary>
    ''' The user interface control associated with this register variable.
    ''' </summary>
    Public Property AffiliatedControl() As Control
        Get
            Return m_AffiliatedControl
        End Get
        Set(value As Control)
            m_AffiliatedControl = value
        End Set
    End Property
    Private m_AffiliatedControl As Control
#End Region
End Class


'
' The DependencyProperties that are bound to floating point shader constant registers can be any of the following types:
'
'Double
'Single ('float' in C#)
'Color
'Size
'Point
'Vector
'Point3D
'Vector3D
'Point4D
'
' * They each will go into their shader register filling up whatever number of components of that register are appropriate.  For instance, Double and Single go into one component, Color into 4, Size, Point and Vector into 2, etc.  Unfilled components are set to '1'.
'
'Some minutiae
'Register Limit: There is a limit of 32 floating point registers that can be used in PS 2.0.  In the unlikely event that you have more values than that that you want to pack in, you might consider tricks like packing, for instance, two Points into a single Point4D, etc.
'
' 
