'
'   Project:           Shaders
'
'   Description:       Pixel shader which produces random scratches like in old movies.
'
'   Changed by:        $Author: Rene $
'   Changed on:        $Date: 2010-01-09 21:29:04 +0100 (Sa, 09. Jan 2010) $
'   Changed in:        $Revision: 124 $
'   Project:           $URL: file:///U:/Data/Development/SVN/SilverlightDemos/trunk/OldCam/OldCam/Shader/ScratchedShader.cs $
'   Id:                $Id: ScratchedShader.cs 124 2010-01-09 20:29:04Z Rene $
'
'
'   Copyright (c) 2009 Rene Schulte
'
'   This program is open source software. Please read the License.txt.
'

Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Media.Effects
Imports System.Windows.Media.Imaging

Public Class ScratchedShader
    Inherits ShaderEffect
#Region "Fields"

    Private rand As New Random()

#End Region

#Region "DependencyProperties"

    Public Shared InputProperty As DependencyProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", GetType(ScratchedShader), 0)
    Public Shared NoiseProperty As DependencyProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("NoiseSampler", GetType(ScratchedShader), 1)
    Public Shared ScratchAmountProperty As DependencyProperty = DependencyProperty.Register("ScratchAmount", GetType(Single), GetType(ScratchedShader), New PropertyMetadata(0.0044F, ShaderEffect.PixelShaderConstantCallback(0)))
    Public Shared NoiseAmountProperty As DependencyProperty = DependencyProperty.Register("NoiseAmount", GetType(Single), GetType(ScratchedShader), New PropertyMetadata(0.000001F, ShaderEffect.PixelShaderConstantCallback(1)))
    Public Shared RandomCoord1Property As DependencyProperty = DependencyProperty.Register("RandomCoord1", GetType(Point), GetType(ScratchedShader), New PropertyMetadata(New Point(0, 0), ShaderEffect.PixelShaderConstantCallback(2)))
    Public Shared RandomCoord2Property As DependencyProperty = DependencyProperty.Register("RandomCoord2", GetType(Point), GetType(ScratchedShader), New PropertyMetadata(New Point(0, 0), ShaderEffect.PixelShaderConstantCallback(3)))
    Public Shared FrameProperty As DependencyProperty = DependencyProperty.Register("Frame", GetType(Single), GetType(ScratchedShader), New PropertyMetadata(0.0F, ShaderEffect.PixelShaderConstantCallback(4)))

#End Region

#Region "Properties"

    Public Overridable Property Input() As System.Windows.Media.Brush
        Get
            Return DirectCast(GetValue(InputProperty), System.Windows.Media.Brush)
        End Get
        Set(value As System.Windows.Media.Brush)
            SetValue(InputProperty, value)
        End Set
    End Property

    Public Overridable Property NoiseImage() As System.Windows.Media.Brush
        Get
            Return DirectCast(GetValue(NoiseProperty), System.Windows.Media.Brush)
        End Get
        Set(value As System.Windows.Media.Brush)
            SetValue(NoiseProperty, value)
        End Set
    End Property

    Public Overridable Property ScratchAmount() As Single
        Get
            Return CSng(GetValue(ScratchAmountProperty))
        End Get
        Set(value As Single)
            SetValue(ScratchAmountProperty, value)
        End Set
    End Property

    Public Overridable Property NoiseAmount() As Single
        Get
            Return CSng(GetValue(NoiseAmountProperty))
        End Get
        Set(value As Single)
            SetValue(NoiseAmountProperty, value)
        End Set
    End Property

    Public Overridable Property RandomCoord1() As Point
        Get
            Return DirectCast(GetValue(RandomCoord1Property), Point)
        End Get
        Set(value As Point)
            SetValue(RandomCoord1Property, value)
        End Set
    End Property

    Public Overridable Property RandomCoord2() As Point
        Get
            Return DirectCast(GetValue(RandomCoord2Property), Point)
        End Get
        Set(value As Point)
            SetValue(RandomCoord2Property, value)
        End Set
    End Property

    Public Overridable Property Frame() As Single
        Get
            Return CSng(GetValue(FrameProperty))
        End Get
        Set(value As Single)
            SetValue(FrameProperty, value)
        End Set
    End Property

#End Region

#Region "Contructors"

    Public Sub New()
        Me.PixelShader = New PixelShader() With { _
             .UriSource = New Uri(AppDomain.CurrentDomain.BaseDirectory & "\ShaderFiles\ScratchedShader.ps") _
        }
        Me.UpdateShaderValue(InputProperty)
        Me.UpdateShaderValue(NoiseProperty)
        Me.UpdateShaderValue(ScratchAmountProperty)
        Me.UpdateShaderValue(NoiseAmountProperty)
        Me.UpdateShaderValue(RandomCoord1Property)
        Me.UpdateShaderValue(RandomCoord2Property)
        Me.UpdateShaderValue(FrameProperty)

        ' Generate noise texture and attach to shader
        Me.NoiseImage = GenerateNoiseTexture()

        AddHandler CompositionTarget.Rendering, AddressOf CompositionTarget_Rendering
    End Sub

#End Region

#Region "Methods"

    Private Function GenerateNoiseTexture() As Brush
        ' Generate random image for pixel shader lookup
        Const NoiseTexSize As Integer = 1024
        Dim size As Integer = NoiseTexSize * NoiseTexSize
        Dim dpi As Integer = 96
        Dim width As Integer = NoiseTexSize
        Dim height As Integer = NoiseTexSize

        Dim pixelData(width * height) As Byte

        For y As Integer = 0 To height - 1
            Dim yIndex As Integer = y * width

            For x As Integer = 0 To width - 1
                pixelData(x + yIndex) = CByte(rand.NextDouble() * 255)
            Next
        Next

        Dim bmpSource As BitmapSource
        bmpSource = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Gray8, Nothing, pixelData, width)


        Dim noiseImg As New ImageBrush()
        noiseImg.ImageSource = bmpSource
        Return noiseImg

    End Function

#End Region

#Region "Eventhandler"
    Dim LastTiumeRendered As Double = 0
    Dim RenderPeriodInMS As Double = 10
    Private Sub CompositionTarget_Rendering(sender As Object, e As EventArgs)
        Dim rargs As RenderingEventArgs = DirectCast(e, RenderingEventArgs)
        If ((rargs.RenderingTime.TotalMilliseconds - LastTiumeRendered) > RenderPeriodInMS) Then
            ' Random seeds for shader noise texture lookup and frame increment
            Me.RandomCoord1 = New Point(rand.NextDouble(), rand.NextDouble())
            Me.RandomCoord2 = New Point(rand.NextDouble(), rand.NextDouble())
            Me.Frame += 1
            LastTiumeRendered = rargs.RenderingTime.TotalMilliseconds
        End If
    End Sub

#End Region
End Class


