Imports System.Windows
Imports System.Windows.Media.Effects

Public Class MagnifierEffect
    Inherits ShaderEffectBase

    Public Shared ReadOnly CenterProperty As DependencyProperty =
        DependencyProperty.Register("Center",
                                    GetType(Point),
                                    GetType(MagnifierEffect),
                                    New PropertyMetadata(New Point(0.5, 0.5),
                                                         PixelShaderConstantCallback(0)))

    Public Shared ReadOnly InnerRadiusProperty As DependencyProperty =
        DependencyProperty.Register("InnerRadius",
                                    GetType(Double),
                                    GetType(MagnifierEffect),
                                    New PropertyMetadata(0.2, PixelShaderConstantCallback(2)))

    ''' <summary>
    ''' Gets or sets the magnification.
    ''' </summary>
    Public Shared ReadOnly MagnificationProperty As DependencyProperty = DependencyProperty.Register("Magnification", GetType(Double), GetType(MagnifierEffect), New PropertyMetadata(2.0, PixelShaderConstantCallback(3)))

    ''' <summary>
    ''' Gets or sets the magnifier outer radius.
    ''' </summary>
    Public Shared ReadOnly OuterRadiusProperty As DependencyProperty = DependencyProperty.Register("OuterRadius", GetType(Double), GetType(MagnifierEffect), New PropertyMetadata(0.27, PixelShaderConstantCallback(4)))

    Sub New()
        PixelShader = New PixelShader
        PixelShader.UriSource = New Uri(AppDomain.CurrentDomain.BaseDirectory & "\ShaderFiles\Magnifier.ps")

        Me.UpdateShaderValue(CenterProperty)
        Me.UpdateShaderValue(InnerRadiusProperty)
        Me.UpdateShaderValue(OuterRadiusProperty)
        Me.UpdateShaderValue(MagnificationProperty)
    End Sub

    ''' <summary>
    ''' Gets or sets the magnifier center.
    ''' </summary>
    Public Property Center() As Point
        Get
            Return DirectCast(Me.GetValue(CenterProperty), Point)
        End Get
        Set(value As Point)
            Me.SetValue(CenterProperty, value)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the magnifier inner radius.
    ''' </summary>
    Public Property InnerRadius() As Double
        Get
            Return CDbl(Me.GetValue(InnerRadiusProperty))
        End Get
        Set(value As Double)
            Me.SetValue(InnerRadiusProperty, value)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the magnification.
    ''' </summary>
    Public Property Magnification() As Double
        Get
            Return CDbl(Me.GetValue(MagnificationProperty))
        End Get
        Set(value As Double)
            Me.SetValue(MagnificationProperty, value)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the magnifier outer radius.
    ''' </summary>
    Public Property OuterRadius() As Double
        Get
            Return CDbl(Me.GetValue(OuterRadiusProperty))
        End Get
        Set(value As Double)
            Me.SetValue(OuterRadiusProperty, value)
        End Set
    End Property
End Class

