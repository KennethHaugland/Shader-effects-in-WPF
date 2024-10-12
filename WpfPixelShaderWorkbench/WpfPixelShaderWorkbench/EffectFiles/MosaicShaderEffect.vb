Imports System.Windows.Media.Effects

Public Class MosaicShaderEffect
    Inherits ShaderEffectBase

    Public Shared BlockCountProperty As DependencyProperty =
        DependencyProperty.Register("BlockCount",
                                    GetType(Single),
                                    GetType(MosaicShaderEffect),
                                    New PropertyMetadata(100.0F, PixelShaderConstantCallback(0)))

    Public Property BlockCount() As Single
        Get
            Return DirectCast(Me.GetValue(BlockCountProperty), Single)
        End Get
        Set(value As Single)
            Me.SetValue(BlockCountProperty, value)
        End Set
    End Property

    Public Shared ReadOnly MinProperty As DependencyProperty =
    DependencyProperty.Register("Min",
                                GetType(Double),
                                GetType(MosaicShaderEffect),
                                New PropertyMetadata(0.2, PixelShaderConstantCallback(1)))

    Public Property Min() As Double
        Get
            Return DirectCast(Me.GetValue(MinProperty), Double)
        End Get
        Set(value As Double)
            Me.SetValue(MinProperty, value)
        End Set
    End Property

    Public Shared ReadOnly MaxProperty As DependencyProperty =
DependencyProperty.Register("Max",
                            GetType(Double),
                            GetType(MosaicShaderEffect),
                            New PropertyMetadata(0.5, PixelShaderConstantCallback(2)))

    Public Property Max() As Double
        Get
            Return DirectCast(Me.GetValue(MaxProperty), Double)
        End Get
        Set(value As Double)
            Me.SetValue(MaxProperty, value)
        End Set
    End Property

    Public Shared ReadOnly AspectRatioProperty As DependencyProperty =
DependencyProperty.Register("AspectRatio",
                        GetType(Single),
                        GetType(MosaicShaderEffect),
                        New PropertyMetadata(1.0F, PixelShaderConstantCallback(3)))

    Public Property AspectRatio() As Single
        Get
            Return DirectCast(Me.GetValue(AspectRatioProperty), Single)
        End Get
        Set(value As Single)
            Me.SetValue(AspectRatioProperty, value)
        End Set
    End Property

    Sub New()

        PixelShader = New PixelShader With {.UriSource = New Uri(AppDomain.CurrentDomain.BaseDirectory & "\ShaderFiles\MosaicShader.ps")}

        Me.UpdateShaderValue(BlockCountProperty)
        Me.UpdateShaderValue(MinProperty)
        Me.UpdateShaderValue(MaxProperty)
        Me.UpdateShaderValue(AspectRatioProperty)
        Me.UpdateShaderValue(InputProperty)
    End Sub

End Class
