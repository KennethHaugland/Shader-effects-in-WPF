Imports System.Windows.Media.Effects

Public Class SimpleMultiInputEffect
    Inherits ShaderEffectBase

    Sub New()
        Dim s As String = AppDomain.CurrentDomain.BaseDirectory
        PixelShader = New PixelShader With {.UriSource = New Uri(s & "\ShaderFiles\SimpleMultiInputEffect.ps")}
        Me.UpdateShaderValue(Input2Property)
        Me.UpdateShaderValue(MixInAmountProperty)
    End Sub

    Public Shared ReadOnly Input2Property As DependencyProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input2", GetType(SimpleMultiInputEffect), 1, SamplingMode.NearestNeighbor)

    Public Property Input2() As Brush
        Get
            Return TryCast(Me.GetValue(Input2Property), Brush)
        End Get
        Set(value As Brush)
            Me.SetValue(Input2Property, value)
        End Set
    End Property

    Public Shared MixInAmountProperty As DependencyProperty = DependencyProperty.Register("MixInAmount", GetType(Double), GetType(SimpleMultiInputEffect),
                    New PropertyMetadata(0.5, PixelShaderConstantCallback(0)))

    Public Property MixInAmount As Double
        Get
            Return DirectCast(Me.GetValue(MixInAmountProperty), Double)
        End Get
        Set(value As Double)
            Me.SetValue(MixInAmountProperty, value)
        End Set
    End Property

End Class
