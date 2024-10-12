Imports System.Windows.Media.Effects

Public Class GaussianEffect
    Inherits ShaderEffectBase

    Sub New()
        PixelShader = New PixelShader With {.UriSource = New Uri(AppDomain.CurrentDomain.BaseDirectory & "\ShaderFiles\GaussianFilter.ps")}
        Me.UpdateShaderValue(ImageSizeProperty)
    End Sub

    Public Shared ImageSizeProperty As DependencyProperty = DependencyProperty.Register("ImageSize", GetType(Point), GetType(GaussianEffect),
                    New PropertyMetadata(New Point(300, 200), PixelShaderConstantCallback(0)))

    Public Property ImageSize As Point
        Get
            Return DirectCast(Me.GetValue(ImageSizeProperty), Point)
        End Get
        Set(value As Point)
            Me.SetValue(ImageSizeProperty, value)
        End Set
    End Property

End Class
