Imports System.Globalization
Imports System.Reflection
Imports System.Windows.Media.Effects

Class MainWindow

    Private FXCHelperPointer As FXC_CompileHelper
    Private CodeTypeList As New List(Of String)

    Public Sub New(ByRef DirectXHelper As FXC_CompileHelper)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        FXCHelperPointer = DirectXHelper

    End Sub


    Private Sub Image_Loaded(sender As Object, e As RoutedEventArgs)

        CodeTypeList.Add("C# code")
        CodeTypeList.Add("VB.NET code")

        Dim b As New Binding()
        b.Source = FXCHelperPointer
        b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        b.Path = New PropertyPath("OutPutString")
        Me.txtOutput.SetBinding(TextBox.TextProperty, b)

        cmbFxFiles.ItemsSource = FXCHelperPointer.FilesToCompile
        cmbFxFiles.SelectedIndex = 0
        cmbCodeType.ItemsSource = CodeTypeList
        cmbCodeType.SelectedIndex = 1

    End Sub

    Private Sub btnGenerateCode_Click(sender As Object, e As RoutedEventArgs)

        If cmbFxFiles.SelectedItem Is Nothing Then
            Exit Sub
        End If

        Dim Item As HLSLFileHelperClass = cmbFxFiles.SelectedItem

        Dim CodeDomeFileGenerator As New CreatePixelShaderClass
        Dim VariableList As New ShaderModel
        Dim VariablesFromFxFile As New EffectClassToShader
        VariableList = VariablesFromFxFile.ParseShader(Item.GetSourceFileFullName, Item.GetFileAsATextString)

        If cmbCodeType.SelectedIndex = 0 Then
            ' Code in C# format 
            Dim provider As New Microsoft.CSharp.CSharpCodeProvider
            txtGeneratedCode.Text = CodeDomeFileGenerator.GetSourceText(provider, VariableList, True)
        Else
            ' Code in VB.NET format
            Dim provider As New Microsoft.VisualBasic.VBCodeProvider()
            txtGeneratedCode.Text = CodeDomeFileGenerator.GetSourceText(provider, VariableList, True)
        End If

    End Sub

    Private Sub btnAddANewFxFile_Click(sender As Object, e As RoutedEventArgs)
        Dim dlg As New System.Windows.Forms.OpenFileDialog

        If dlg.ShowDialog Then
            For Each file As String In dlg.FileNames
                Dim d As String = IO.Path.GetExtension(file).ToLower
                If IO.Path.GetExtension(file).ToLower = ".fx" Then
                    Dim ExeFileDirectory As String = AppDomain.CurrentDomain.BaseDirectory

                    'The name of the folder that will be opened or created at the exe folder location
                    Dim FolderNameForTheCompiledPsFiles As String = "ShaderFiles"

                    ' Create the Directory were the shader files will be
                    Dim FolderWithCompiledShaderFiles As String = ExeFileDirectory & FolderNameForTheCompiledPsFiles

                    'Find the resource folder where the uncompiled fx filer are
                    Dim ShaderSourceFiles As String = IO.Path.Combine(GetPerantDirectory(ExeFileDirectory, 2), "ShaderSourceFiles")


                    Dim s As String = IO.File.ReadAllText(file)
                    IO.File.WriteAllText(IO.Path.Combine(ShaderSourceFiles, IO.Path.GetFileName(file)), s)

                    Dim TempFileInfo As New IO.FileInfo(file)

                    ' Get the file name
                    Dim FileNamsWithoutEx As String = IO.Path.GetFileNameWithoutExtension(TempFileInfo.Name)

                    Dim TheFxFile As New HLSLFileHelperClass
                    TheFxFile.FileNameWithoutExtension = FileNamsWithoutEx
                    TheFxFile.CompiledFileLocation = FolderWithCompiledShaderFiles
                    TheFxFile.SourceFileLocation = ShaderSourceFiles

                    Dim GetTheFxFilesVariables As New EffectClassToShader
                    Dim FxFileVariables As New ShaderModel
                    FxFileVariables = GetTheFxFilesVariables.ParseShader(TheFxFile.GetSourceFileFullName, TheFxFile.GetFileAsATextString)

                    Dim provider As Object
                    If cmbCodeType.SelectedIndex = 0 Then
                        ' Code in C# format 
                        provider = New Microsoft.CSharp.CSharpCodeProvider
                    Else
                        ' Code in VB.NET format
                        provider = New Microsoft.VisualBasic.VBCodeProvider()
                    End If

                    Dim CodeDomGenerator As New CreatePixelShaderClass
                    TheFxFile.GeneretedCSCode = CodeDomGenerator.GetSourceText(provider, FxFileVariables, True)

                    FXCHelperPointer.FilesToCompile.Add(TheFxFile)
                    cmbFxFiles.ItemsSource = FXCHelperPointer.FilesToCompile
                    cmbFxFiles.SelectedIndex = 0
                End If
            Next
        End If
    End Sub

    Private Function GetPerantDirectory(ByVal FolderNAme As String, Optional ByVal ParentNumber As Integer = 1) As String

        If ParentNumber = 0 Then
            Return FolderNAme
        End If

        Dim result As IO.DirectoryInfo
        Dim CurrentFolderNAme As String = FolderNAme
        For i As Integer = 1 To ParentNumber + 1
            result = IO.Directory.GetParent(CurrentFolderNAme)
            CurrentFolderNAme = result.FullName
        Next
        Return CurrentFolderNAme
    End Function

    Private Shared Function CaptureScreen(target As Visual, Optional dpiX As Double = 96, Optional dpiY As Double = 96) As BitmapSource
        If target Is Nothing Then
            Return Nothing
        End If
        Dim bounds As Rect = VisualTreeHelper.GetDescendantBounds(target)
        Dim rtb As New RenderTargetBitmap(CInt(bounds.Width * dpiX / 96.0), CInt(bounds.Height * dpiY / 96.0), dpiX, dpiY, PixelFormats.Pbgra32)
        Dim dv As New DrawingVisual()
        Using ctx As DrawingContext = dv.RenderOpen()
            Dim vb As New VisualBrush(target)
            ctx.DrawRectangle(vb, Nothing, New Rect(New Point(), bounds.Size))
        End Using
        rtb.Render(dv)
        Return rtb
    End Function

    ' Create a DrawingVisual that contains a rectangle.
    Private Function CreateDrawingVisualRectangle() As DrawingVisual
        Dim drawingVisual As New DrawingVisual()
        drawingVisual.Effect = New GaussianEffect

        ' Retrieve the DrawingContext in order to create new drawing content.
        Dim drawingContext As DrawingContext = drawingVisual.RenderOpen()

        ' Create a rectangle and draw it in the DrawingContext.
        Dim rect As New Rect(New Point(160, 100), New Size(320, 80))
        drawingContext.DrawRectangle(Brushes.LightBlue, CType(Nothing, Pen), rect)

        ' Persist the drawing content.
        drawingContext.Close()

        Return drawingVisual
    End Function

    Private IsDraggingRect As Boolean = False
    Private StartPoint As New Point(0, 0)
    Private CurrentPosision As New Point(0, 0)
    Private StoredPosision As New Point(0, 0)

    Private Sub BoundImageRect_MouseDown(sender As Object, e As MouseButtonEventArgs)
        IsDraggingRect = True
        StartPoint = e.GetPosition(Img)
    End Sub

    Private Sub BoundImageRect_MouseMove(sender As Object, e As MouseEventArgs)
        If IsDraggingRect Then
            Dim p As Point = e.GetPosition(Img)
            CurrentPosision = p - StartPoint

            Trans.X = CurrentPosision.X + StoredPosision.X
            Trans.Y = CurrentPosision.Y + StoredPosision.Y
        End If
    End Sub

    Private Sub BoundImageRect_MouseUp(sender As Object, e As MouseButtonEventArgs)
        IsDraggingRect = False
        StoredPosision += CurrentPosision
    End Sub
End Class

Module [Global]

    ''' <summary>
    ''' Helper method for generating a "pack://" URI for a given relative file based on the
    ''' assembly that this class is in.
    ''' </summary>
    Public Function MakePackUri(relativeFile As String) As Uri
        Dim uriString As String = Convert.ToString((Convert.ToString("pack://application:,,,/") & AssemblyShortName) + ";component/") & relativeFile
        Return New Uri(uriString)
    End Function

    Private ReadOnly Property AssemblyShortName() As String
        Get
            If _assemblyShortName Is Nothing Then
                Dim a As Assembly = GetType([Global]).Assembly

                ' Pull out the short name.
                _assemblyShortName = a.ToString().Split(","c)(0)
            End If

            Return _assemblyShortName
        End Get
    End Property

    Private _assemblyShortName As String
End Module

Public Class RectangleToRectConverter
    Implements IMultiValueConverter

    Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
        Return New System.Windows.Rect(values(2), values(3), DirectCast(values(0), Double), values(1))
    End Function

    Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

End Class


Public Class CoolDrawing
    Inherits FrameworkElement

    Implements System.ComponentModel.INotifyPropertyChanged

    ' Create a collection of child visual objects.
    Private _children As VisualCollection

    Public Sub New()
        _children = New VisualCollection(Me)

        Dim VisualImage As DrawingVisual
        VisualImage = CreateDrawingVisualCircle()
        VisualImage.Effect = New GaussianEffect
        _children.Add(VisualImage)

    End Sub



#Region "Overided properties"

    ' Provide a required override for the VisualChildrenCount property.
    Protected Overrides ReadOnly Property VisualChildrenCount() As Integer
        Get
            Return _children.Count
        End Get
    End Property

    ' Provide a required override for the GetVisualChild method.
    Protected Overrides Function GetVisualChild(ByVal index As Integer) As Visual
        If index < 0 OrElse index >= _children.Count Then
            Throw New ArgumentOutOfRangeException()
        End If

        Return _children(index)
    End Function

#End Region

#Region "Drawing"
    ' Create a DrawingVisual that contains a rectangle.
    Private Function CreateDrawingVisualCircle() As DrawingVisual
        Dim drawingVisual As New DrawingVisual()

        ' Retrieve the DrawingContext in order to create new drawing content.
        Dim drawingContext As DrawingContext = drawingVisual.RenderOpen()
        Dim mSides As Integer = 8
        Dim mRadius As Double = 10
        Dim mat As New PointCollection
        For i As Integer = 0 To mSides - 1
            Dim x As Double = mRadius * Math.Cos((2 * Math.PI / mSides) * i)
            Dim y As Double = mRadius * Math.Sin((2 * Math.PI / mSides) * i)
            mat.Add(New Point(x, y))
        Next

        For i As Integer = 0 To mSides - 2
            drawingContext.DrawLine(New Pen(Brushes.Red, 1.0), mat(i), mat(i + 1))
        Next

        ' Persist the drawing content.
        drawingContext.Close()

        Return drawingVisual
    End Function

#End Region

#Region "Events"
    Public Sub INotifyChange(ByVal info As String)
        RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(info))
    End Sub

    Public Event PropertyChanged(sender As Object,
           e As System.ComponentModel.PropertyChangedEventArgs) _
           Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
#End Region

End Class