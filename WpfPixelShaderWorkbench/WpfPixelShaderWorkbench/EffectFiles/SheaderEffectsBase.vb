
Imports System.Windows.Media.Effects
Imports System.Windows
Imports System.Windows.Media

''' <summary>
''' Base class for all shader effects used in the application.
''' </summary>
Public MustInherit Class ShaderEffectBase
    Inherits ShaderEffect

    Public Shared ReadOnly InputProperty As DependencyProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", GetType(ShaderEffectBase), 0, SamplingMode.NearestNeighbor)

    Protected Sub New()
        Me.UpdateShaderValue(InputProperty)
    End Sub

    ''' <summary>
    ''' Gets, Sets the effect input.
    ''' </summary>
    Public Property Input() As Brush
        Get
            Return TryCast(Me.GetValue(InputProperty), Brush)
        End Get
        Set(value As Brush)
            Me.SetValue(InputProperty, value)
        End Set
    End Property

    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(value As String)
            m_Name = value
        End Set
    End Property
    Private m_Name As String
End Class


