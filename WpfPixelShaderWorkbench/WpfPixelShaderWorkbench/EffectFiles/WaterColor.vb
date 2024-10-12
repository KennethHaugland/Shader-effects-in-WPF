Imports System.Windows.Media.Effects
Imports System.Windows.Media.Media3D
Public Class WaterColor
    Inherits ShaderEffectBase

    Public Shared DxDyShiftProperty As DependencyProperty =
        DependencyProperty.Register("DxDyShift",
                                    GetType(Point4D),
                                    GetType(WaterColor),
                                    New PropertyMetadata(New Point4D(0.0F, 0.0F, 10.0F, 10.0F), PixelShaderConstantCallback(1)))

    Public Property DxDyShift() As Point4D
        Get
            Return DirectCast(Me.GetValue(DxDyShiftProperty), Point4D)
        End Get
        Set(value As Point4D)
            Me.SetValue(DxDyShiftProperty, value)
        End Set
    End Property

    Public Shared ReadOnly DetailProperty As DependencyProperty =
    DependencyProperty.Register("Detail",
                                GetType(Single),
                                GetType(WaterColor),
                                New PropertyMetadata(3.0F, PixelShaderConstantCallback(2)))

    Public Property Detail() As Single
        Get
            Return DirectCast(Me.GetValue(DetailProperty), Single)
        End Get
        Set(value As Single)
            Me.SetValue(DetailProperty, value)
        End Set
    End Property

    Public Shared ReadOnly edgeColorAmountProperty As DependencyProperty =
DependencyProperty.Register("edgeColorAmount",
                            GetType(Single),
                            GetType(WaterColor),
                            New PropertyMetadata(2.0F, PixelShaderConstantCallback(3)))

    Public Property edgeColorAmount() As Single
        Get
            Return DirectCast(Me.GetValue(edgeColorAmountProperty), Single)
        End Get
        Set(value As Single)
            Me.SetValue(edgeColorAmountProperty, value)
        End Set
    End Property


    Sub New()

        PixelShader = New PixelShader With {.UriSource = New Uri(AppDomain.CurrentDomain.BaseDirectory & "\ShaderFiles\Watercolor.ps")}

        Me.UpdateShaderValue(DxDyShiftProperty)
        Me.UpdateShaderValue(DetailProperty)
        Me.UpdateShaderValue(edgeColorAmountProperty)
        Me.UpdateShaderValue(InputProperty)
    End Sub
End Class
