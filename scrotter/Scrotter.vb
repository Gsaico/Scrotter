﻿'Scrotter, a program designed by yttrium to frame mobile screenshots.
'Copyright (C) 2013 Alex and Serah West
'Version 0.9.6 Beta
'
'This work may be distributed and/or modified under the
'conditions of the LaTeX Project Public License, either version 1.3
'of this license or (at your option) any later version.
'The latest version of this license is in
'  http://www.latex-project.org/lppl.txt
'and version 1.3 or later is part of all distributions of LaTeX
'version 2005/12/01 or later.
'
'This work has the LPPL maintenance status `maintained'.
'
'The Current Maintainer of this work is Alex West.

Imports System.Net
Imports System.IO
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Threading
Imports System.Environment
Imports System.Drawing.Text
Imports System.Runtime.InteropServices

Public Class Scrotter
    Public Shared OpenPath(7), SavePath As String
    Public OpenStream As Stream = Nothing
    Public SaveStream As Stream = Nothing
    Public PhoneStream As Stream = Nothing
    Public SaveImg As Image
    Public CanvImg(7) As Image
    Public Image2 As New Bitmap(720, 1280)
    Public Shared IsMono As Boolean
    Public ReadOnly Version As String = "0.9.6"
    Public ReadOnly ReleaseDate As String = "2013-11-01"
    Private Image(7) As String
    Public AppData As String
    Public Database(,) As String

    Private Sub LoadDatabase()
        Dim line As String() = IO.File.ReadAllLines(System.IO.Path.Combine(Application.StartupPath, "db.xls"))
        For x = 0 To line.Length
            For y = 0 To 9
                If line(x).Contains(",") = True Then
                    Database(x, y) = line(x).Substring(0, line(x).IndexOf(","))
                    line(x) = line(x).Substring(line(x).IndexOf(",") + 1)
                Else
                    Database(x, y) = line(x)
                End If
            Next
        Next
    End Sub

    Private Sub LoadBtn_Click(sender As Object, e As EventArgs) Handles LoadBtn.Click
        Dim lastfolderopen As String = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        Dim openFileDialog1 As New OpenFileDialog()
        openFileDialog1.Title = "Please select your screenshot..."
        openFileDialog1.InitialDirectory = lastfolderopen
        openFileDialog1.Filter = "BMP Files(*.BMP)|*.BMP|PNG Files(*.PNG)|*.PNG|JPG Files(*.JPG)|*.JPG|GIF Files(*.GIF)|*.GIF|All Files(*.*)|*.*"
        openFileDialog1.FilterIndex = 5
        openFileDialog1.RestoreDirectory = True
        If openFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            LoadImage.Image = My.Resources.Loading
            Try
                OpenStream = openFileDialog1.OpenFile()
                If (OpenStream IsNot Nothing) Then
                    OpenPath(ScreenPicker.Value) = openFileDialog1.FileName
                    ScreenshotBox.Text = OpenPath(ScreenPicker.Value)
                    RefreshLists()
                End If
            Catch Ex As Exception
            Finally
                If (OpenStream IsNot Nothing) Then
                    OpenStream.Close()
                End If
            End Try
        End If
    End Sub

    Private Sub Save(sender As Object, e As EventArgs) Handles SaveBtn.Click
        If ScreenAmountPicker.Value > 1 Then
            Dim number As Integer = 1
            Do While number <= ScreenAmountPicker.Value
                If CanvImg(number) Is Nothing Then CanvImg(number) = New Bitmap(CanvImg(1).Width, CanvImg(1).Height)
                number = number + 1
            Loop
            ArrayPreview.ShowDialog()
            Exit Sub
        End If
        Dim saveFileDialog1 As New SaveFileDialog()
        saveFileDialog1.FileName = "Scrotter_" & DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") & ".png"
        saveFileDialog1.Filter = "BMP Files(*.BMP)|*.BMP|PNG Files(*.PNG)|*.PNG|JPG Files(*.JPG)|*.JPG|All Files(*.*)|*.*" '|GIF Files(*.GIF)|*.GIF"
        saveFileDialog1.FilterIndex = 2
        saveFileDialog1.RestoreDirectory = True
        If saveFileDialog1.ShowDialog() = DialogResult.OK Then
            SaveImg = CanvImg(1)
            SaveStream = saveFileDialog1.OpenFile()
            If (SaveStream IsNot Nothing) Then
                SavePath = saveFileDialog1.FileName
                SaveStream.Close()
                RefreshLists()
                Dim Filetype As Integer = saveFileDialog1.FilterIndex
                Dim bm As Bitmap = SaveImg
                If Filetype = 1 Then
                    Dim Image3 As New Bitmap(bm.Width, bm.Height)
                    Dim g As Graphics = Graphics.FromImage(Image3)
                    g.Clear(Color.White)
                    g.DrawImage(bm, New Point(0, 0))
                    g.Dispose()
                    g = Nothing
                    Image3.Save(SavePath, System.Drawing.Imaging.ImageFormat.Bmp)
                ElseIf Filetype = 2 Then
                    bm.Save(SavePath, System.Drawing.Imaging.ImageFormat.Png)
                ElseIf Filetype = 3 Then
                    Dim jgpEncoder As ImageCodecInfo = GetEncoder(ImageFormat.Jpeg)
                    Dim myEncoder As System.Drawing.Imaging.Encoder = System.Drawing.Imaging.Encoder.Quality
                    Dim myEncoderParameters As New EncoderParameters(1)
                    Dim myEncoderParameter As New EncoderParameter(myEncoder, 98&)
                    myEncoderParameters.Param(0) = myEncoderParameter
                    Dim Image3 As New Bitmap(bm.Width, bm.Height)
                    Dim g As Graphics = Graphics.FromImage(Image3)
                    g.Clear(Color.White)
                    g.DrawImage(bm, New Point(0, 0))
                    g.Dispose()
                    g = Nothing
                    Image3.Save(SavePath, jgpEncoder, myEncoderParameters)
                    'ElseIf Filetype = 2 Then
                    'Dim Image3 As New Bitmap(bm.Width, bm.Height)
                    'Dim g As Graphics = Graphics.FromImage(Image3)
                    'g.Clear(Color.White)
                    'g.DrawImage(bm, New Point(0, 0))
                    'g.Dispose()
                    'g = Nothing
                    'Image3.Save(SavePath, System.Drawing.Imaging.ImageFormat.Gif)
                End If
            End If
        End If
    End Sub

    Private Function GetEncoder(ByVal format As ImageFormat) As ImageCodecInfo
        Dim codecs As ImageCodecInfo() = ImageCodecInfo.GetImageDecoders()

        Dim codec As ImageCodecInfo
        For Each codec In codecs
            If codec.FormatID = format.Guid Then
                Return codec
            End If
        Next codec
        Return Nothing
    End Function

    Private Sub RefreshLists() Handles ModelBox.SelectedValueChanged
        UnderShadowCheckbox.Enabled = True
        UnderShadowCheckbox.Checked = False
        GlossCheckbox.Enabled = True
        GlossCheckbox.Checked = False
        ShadowCheckbox.Enabled = True
        ShadowCheckbox.Checked = False
        VariantBox.Enabled = False
        VariantBox.Items.Clear()
        VariantBox.Text = "Variant"
        Select Case ModelBox.Text
            Case "Samsung Galaxy SIII"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"White", "Blue", "Red", "Brown", "Black"})
                VariantBox.SelectedIndex = 0
            Case "Google Nexus 7", "Google Nexus 10", "Motorola Xoom"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"Portrait", "Landscape"})
                VariantBox.SelectedIndex = 0
            Case "HTC One X, HTC One X+", "HTC One"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"White", "Black"})
                VariantBox.SelectedIndex = 0
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
            Case "Apple iPhone 4", "Apple iPhone 4S", "Apple iPad Mini", "Sony Xperia S"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"Black", "White"})
                VariantBox.SelectedIndex = 0
            Case "Apple iPhone 5"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"Black", "Black Angled", "White", "White Angled"})
                VariantBox.SelectedIndex = 0
            Case "Apple iPhone 5S"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"Black", "Black Angled", "Gold", "Gold Angled", "White", "White Angled"})
                VariantBox.SelectedIndex = 0
                GlossCheckbox.Enabled = False
                GlossCheckbox.Checked = False
            Case "Apple iPhone 5C"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"Blue", "Green", "Red", "White", "Yellow"})
                VariantBox.SelectedIndex = 0
                GlossCheckbox.Enabled = False
                GlossCheckbox.Checked = False
            Case "Samsung Galaxy SII, Epic 4G Touch"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"Galaxy SII", "Galaxy SII T-Mobile", "Epic 4G Touch"})
                VariantBox.SelectedIndex = 0
            Case "Samsung Galaxy SIII Mini", "Motorola Droid RAZR", "Motorola Droid RAZR M", "Samsung Galaxy Player 5.0"
                GlossCheckbox.Enabled = False
                GlossCheckbox.Checked = False
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
            Case "HTC One S", "HTC One V", "Kyocera RiSE"
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
            Case "Google Nexus 4"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"Normal", "Angled", "Slant"})
                VariantBox.SelectedIndex = 0
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
            Case "Apple iPhone 3G, 3GS", "Google Nexus 5"
                GlossCheckbox.Enabled = False
                GlossCheckbox.Checked = False
            Case "BlackBerry Z10"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"Black", "White"})
                VariantBox.SelectedIndex = 0
                GlossCheckbox.Enabled = False
                GlossCheckbox.Checked = False
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
            Case "Samsung Galaxy Note II"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"White", "Gray"})
                VariantBox.SelectedIndex = 0
                GlossCheckbox.Enabled = False
                GlossCheckbox.Checked = False
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
            Case "HTC Desire HD, HTC Inspire 4G"
                GlossCheckbox.Enabled = False
                GlossCheckbox.Checked = True
            Case "LG Optimus 4X HD", "Sony Xperia Z"
                GlossCheckbox.Enabled = False
                GlossCheckbox.Checked = True
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
            Case "Samsung Galaxy SIV"
                GlossCheckbox.Enabled = False
                GlossCheckbox.Checked = False
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"Blue", "White"})
                VariantBox.SelectedIndex = 0
            Case "Motorola Moto X"
                VariantBox.Enabled = True
                VariantBox.Items.AddRange({"Black", "White"})
                VariantBox.SelectedIndex = 0
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
        End Select
        RefreshPreview()
    End Sub

    Private Sub RefreshPreview() Handles VariantBox.SelectedValueChanged, ShadowCheckbox.CheckedChanged, GlossCheckbox.CheckedChanged, UnderShadowCheckbox.CheckedChanged, ScreenPicker.ValueChanged, ReflectBox.CheckedChanged
        ScreenshotBox.Text = OpenPath(ScreenPicker.Value)
        If ModelBox.Text = "Samsung Galaxy SIII" Then
            Select Case VariantBox.Text
                Case "Black", "Red", "Brown"
                    GlossCheckbox.Enabled = False
                    GlossCheckbox.Checked = False
            End Select
        ElseIf ModelBox.Text = "Samsung Galaxy SII, Epic 4G Touch" Then
            If VariantBox.Text = "Galaxy SII T-Mobile" Then
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
            Else
                UnderShadowCheckbox.Enabled = True
            End If
        ElseIf ModelBox.Text = "Apple iPhone 5" Then
            If VariantBox.Text = "White Angled" Then
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
            Else
                UnderShadowCheckbox.Enabled = True
            End If
        ElseIf ModelBox.Text = "Google Nexus 4" Then
            If VariantBox.Text = "Slant" Then
                GlossCheckbox.Enabled = False
                GlossCheckbox.Checked = True
                UnderShadowCheckbox.Enabled = True
            ElseIf VariantBox.Text = "Angled" Then
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = True
                GlossCheckbox.Enabled = True
            Else
                GlossCheckbox.Checked = False
                GlossCheckbox.Enabled = False
                UnderShadowCheckbox.Enabled = False
                UnderShadowCheckbox.Checked = False
            End If
        End If
        If BackgroundDownloader.IsBusy = False Then
            LoadImage.Image = My.Resources.Loading
            Dim args As ArgumentType = New ArgumentType()
            args.type = 1
            args.var = VariantBox.Text
            args.model = ModelBox.Text
            BackgroundDownloader.RunWorkerAsync(args)
        End If
    End Sub

    Private Sub BackgroundDownloader_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundDownloader.DoWork
        Dim args As ArgumentType = e.Argument
        If args.type = 1 Then
            Dim ScreenCapBitmap As New Bitmap(720, 1280)
            Try
                If String.IsNullOrEmpty(OpenPath(ScreenPicker.Value)) = False Then
                    Image(ScreenPicker.Value) = (OpenPath(ScreenPicker.Value))
                    ScreenCapBitmap = New Bitmap(Image(ScreenPicker.Value))
                End If
            Catch ex As Exception
                MsgBox("Unable to load screenshot.")
                Exit Sub
            End Try
            Dim Image1 As New Bitmap(720, 1280)
            Dim Gloss As New Bitmap(720, 1280)
            Dim Undershadow As New Bitmap(720, 1280)
            Dim Shadow As New Bitmap(720, 1280)
            Dim IndexW As Integer = 0
            Dim IndexH As Integer = 0
            Dim DistortPt1 As New PointF
            Dim DistortPt2 As New PointF
            Dim DistortPt3 As New PointF
            Dim DistortPt4 As New PointF
            Dim Overlay As New Bitmap(720, 1280)
            Dim DeviceName As String = ""
            Dim ShadowRes As String = ""
            Dim GlossUsed As Boolean = False
            Dim UndershadowUsed As Boolean = False
            Dim Perspective As Boolean = False
            Dim OverlayUsed As Boolean = False
            Dim databaseurl As String = "https://raw.github.com/Yttrium-tYcLief/Scrotter/database/"
            Select Case args.model
                Case "Samsung Galaxy SIII Mini"
                    DeviceName = "SamsungGSIIIMini"
                    ShadowRes = "480x800"
                    IndexW = 78
                    IndexH = 182
                Case "HTC Desire HD, HTC Inspire 4G"
                    DeviceName = "DesireHD"
                    ShadowRes = "480x800"
                    IndexW = 86
                    IndexH = 130
                    UndershadowUsed = True
                Case "HTC One X, HTC One X+"
                    If args.var = "Black" Then
                        DeviceName = "OneXBlack"
                        IndexW = 113
                    ElseIf args.var = "White" Then
                        DeviceName = "OneXWhite"
                        IndexW = 115
                    End If
                    UndershadowUsed = True
                    GlossUsed = True
                    ShadowRes = "720x1280"
                    IndexH = 213
                    For i = 1 To 10
                        Console.WriteLine(i)
                    Next
                Case "Samsung Galaxy SIII"
                    IndexW = 88
                    If args.var = "Blue" Then
                        DeviceName = "GSIIIBlue"
                        GlossUsed = True
                    ElseIf args.var = "White" Then
                        DeviceName = "GSIIIWhite"
                        GlossUsed = True
                        IndexW = 84
                    ElseIf args.var = "Black" Then
                        DeviceName = "GSIIIBlack"
                    ElseIf args.var = "Red" Then
                        DeviceName = "GSIIIRed"
                    ElseIf args.var = "Brown" Then
                        DeviceName = "GSIIIBrown"
                    End If
                    UndershadowUsed = True
                    ShadowRes = "720x1280"
                    IndexH = 184
                Case "Google Nexus 10"
                    If args.var = "Portrait" Then
                        DeviceName = "Nexus10Port"
                        ShadowRes = "800x1280"
                        IndexW = 217
                        IndexH = 223
                        Dim imgtmp As New Bitmap(800, 1280)
                        Using graphicsHandle As Graphics = Graphics.FromImage(imgtmp)
                            graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic
                            graphicsHandle.DrawImage(ScreenCapBitmap, 0, 0, 800, 1280)
                            ScreenCapBitmap = imgtmp
                        End Using
                    ElseIf args.var = "Landscape" Then
                        DeviceName = "Nexus10Land"
                        ShadowRes = "1280x800"
                        IndexW = 227
                        IndexH = 217
                        Dim imgtmp As New Bitmap(1280, 800)
                        Using graphicsHandle As Graphics = Graphics.FromImage(imgtmp)
                            graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic
                            graphicsHandle.DrawImage(ScreenCapBitmap, 0, 0, 1280, 800)
                            ScreenCapBitmap = imgtmp
                        End Using
                    End If
                    GlossUsed = True
                    UndershadowUsed = True
                Case "Motorola Xoom"
                    If args.var = "Portrait" Then
                        DeviceName = "XoomPort"
                        ShadowRes = "800x1280"
                        IndexW = 199
                        IndexH = 200
                    ElseIf args.var = "Landscape" Then
                        DeviceName = "XoomLand"
                        ShadowRes = "1280x800"
                        IndexW = 218
                        IndexH = 191
                    End If
                    GlossUsed = True
                    UndershadowUsed = True
                Case "Samsung Galaxy SII, Epic 4G Touch"
                    If args.var = "Galaxy SII" Then
                        DeviceName = "GSII"
                        IndexW = 132
                        IndexH = 191
                        UndershadowUsed = True
                    ElseIf args.var = "Epic 4G Touch" Then
                        DeviceName = "Epic4GTouch"
                        IndexW = 132
                        IndexH = 175
                        UndershadowUsed = True
                    ElseIf args.var = "Galaxy SII T-Mobile" Then
                        DeviceName = "GSIItmo"
                        IndexW = 61
                        IndexH = 145
                    End If
                    ShadowRes = "480x800"
                    GlossUsed = True
                Case "Apple iPhone"
                    DeviceName = "iPhone"
                    ShadowRes = "320x480"
                    GlossUsed = True
                    UndershadowUsed = True
                    IndexW = 89
                    IndexH = 176
                Case "Apple iPhone 3G, 3GS"
                    DeviceName = "iPhone3Gand3GS"
                    ShadowRes = "320x480"
                    UndershadowUsed = True
                    IndexW = 88
                    IndexH = 176
                Case "Apple iPhone 4"
                    If args.var = "Black" Then
                        DeviceName = "iPhone4Black"
                    ElseIf args.var = "White" Then
                        DeviceName = "iPhone4White"
                    End If
                    ShadowRes = "640x960"
                    IndexW = 62
                    IndexH = 264
                    GlossUsed = True
                    UndershadowUsed = True
                Case "Apple iPhone 4S"
                    If args.var = "Black" Then
                        DeviceName = "iPhone4SBlack"
                    ElseIf args.var = "White" Then
                        DeviceName = "iPhone4SWhite"
                    End If
                    ShadowRes = "640x960"
                    IndexW = 62
                    IndexH = 264
                    GlossUsed = True
                    UndershadowUsed = True
                Case "Apple iPhone 5"
                    If args.var = "Black" Then
                        DeviceName = "iPhone5Black"
                        IndexW = 133
                        IndexH = 287
                    ElseIf args.var = "White" Then
                        DeviceName = "iPhone5White"
                        IndexW = 133
                        IndexH = 287
                    ElseIf args.var = "White Angled" Then
                        DeviceName = "iPhone5WhiteAngle"
                        IndexW = 336
                        IndexH = 293
                        DistortPt1.X = 0
                        DistortPt1.Y = 321
                        DistortPt2.X = 679
                        DistortPt2.Y = 0
                        DistortPt3.X = 1584
                        DistortPt3.Y = 826
                        DistortPt4.X = 865
                        DistortPt4.Y = 1243
                        Perspective = True
                    ElseIf args.var = "Black Angled" Then
                        DeviceName = "iPhone5BlackAngle"
                        IndexW = 415
                        IndexH = 192
                        DistortPt1.X = 974
                        DistortPt1.Y = 0
                        DistortPt2.X = 1511
                        DistortPt2.Y = 321
                        DistortPt3.X = 544
                        DistortPt3.Y = 936
                        DistortPt4.X = 0
                        DistortPt4.Y = 563
                        Perspective = True
                        OverlayUsed = True
                    End If
                    ShadowRes = "640x1136"
                    GlossUsed = True
                    UndershadowUsed = True
                Case "Samsung Google Galaxy Nexus"
                    DeviceName = "GalaxyNexus"
                    ShadowRes = "720x1280"
                    IndexW = 155
                    IndexH = 263
                    GlossUsed = True
                    UndershadowUsed = True
                Case "Samsung Galaxy Note II"
                    If args.var = "White" Then
                        DeviceName = "GalaxyNoteII"
                    ElseIf args.var = "Gray" Then
                        DeviceName = "GalaxyNoteIIGray"
                    End If
                    IndexW = 77
                    IndexH = 163
                    ShadowRes = "720x1280"
                Case "Motorola Droid RAZR"
                    DeviceName = "DroidRAZR"
                    ShadowRes = "r540960"
                    IndexW = 150
                    IndexH = 206
                Case "Google Nexus 7"
                    If args.var = "Portrait" Then
                        DeviceName = "Nexus7Port"
                        ShadowRes = "800x1280"
                        IndexW = 264
                        IndexH = 311
                    ElseIf args.var = "Landscape" Then
                        DeviceName = "Nexus7Land"
                        ShadowRes = "1280x800"
                        IndexW = 315
                        IndexH = 270
                    End If
                    GlossUsed = True
                    UndershadowUsed = True
                Case "HTC One S"
                    DeviceName = "OneS"
                    ShadowRes = "540x960"
                    IndexW = 106
                    IndexH = 228
                    GlossUsed = True
                Case "HTC One V"
                    DeviceName = "OneV"
                    ShadowRes = "480x800"
                    IndexW = 85
                    IndexH = 165
                    GlossUsed = True
                Case "Google Nexus S"
                    DeviceName = "NexusS"
                    ShadowRes = "480x800"
                    IndexW = 45
                    IndexH = 165
                    GlossUsed = True
                Case "Google Nexus 4"
                    If args.var = "Normal" Then
                        DeviceName = "Nexus4"
                        IndexW = 45
                        IndexH = 193
                    ElseIf args.var = "Angled" Then
                        DeviceName = "Nexus4Angle"
                        IndexW = 137
                        IndexH = 60
                        DistortPt1.X = 0
                        DistortPt1.Y = 240
                        DistortPt2.X = 521
                        DistortPt2.Y = 1
                        DistortPt3.X = 1304
                        DistortPt3.Y = 470
                        DistortPt4.X = 772
                        DistortPt4.Y = 789
                        Perspective = True
                    ElseIf args.var = "Slant" Then
                        DeviceName = "Nexus4Slant"
                        IndexW = 339
                        IndexH = 154
                        DistortPt1.X = 0
                        DistortPt1.Y = 0
                        DistortPt2.X = 506
                        DistortPt2.Y = 34
                        DistortPt3.X = 734
                        DistortPt3.Y = 991
                        DistortPt4.X = 212
                        DistortPt4.Y = 1021
                        Perspective = True
                        UndershadowUsed = True
                    End If
                    ShadowRes = "768x1280"
                    GlossUsed = True
                Case "Motorola Droid RAZR M"
                    DeviceName = "DroidRazrM"
                    ShadowRes = "540x960"
                    IndexW = 49
                    IndexH = 129
                Case "BlackBerry Z10"
                    If args.var = "Black" Then
                        DeviceName = "Z10Black"
                    ElseIf args.var = "White" Then
                        DeviceName = "Z10White"
                    End If
                    ShadowRes = "768x1280"
                    IndexW = 111
                    IndexH = 300
                Case "Samsung Galaxy Player 5.0"
                    DeviceName = "GalaxyPlay5"
                    ShadowRes = "480x800"
                    IndexW = 59
                    IndexH = 122
                Case "HTC One"
                    GlossUsed = True
                    If args.var = "Black" Then
                        DeviceName = "OneBlack"
                    ElseIf args.var = "White" Then
                        DeviceName = "OneWhite"
                    End If
                    ShadowRes = "1080x1920"
                    IndexW = 160
                    IndexH = 281
                Case "Motorola Moto X"
                    GlossUsed = True
                    If args.var = "Black" Then
                        DeviceName = "MotoXBlack"
                    ElseIf args.var = "White" Then
                        DeviceName = "MotoXWhite"
                    End If
                    ShadowRes = "720x1280"
                    IndexW = 52
                    IndexH = 194
                Case "Samsung Galaxy SIV"
                    If args.var = "Blue" Then
                        DeviceName = "GSIVBlue"
                        ShadowRes = "1080x1920"
                        IndexW = 58
                        IndexH = 218
                    ElseIf args.var = "White" Then
                        DeviceName = "GSIVWhite"
                        ShadowRes = "720x1280"
                        IndexW = 45
                        IndexH = 159
                        Dim imgtmp As New Bitmap(1280, 720)
                        Using graphicsHandle As Graphics = Graphics.FromImage(imgtmp)
                            graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic
                            graphicsHandle.DrawImage(ScreenCapBitmap, 0, 0, 1280, 720)
                            ScreenCapBitmap = imgtmp
                        End Using
                    End If
                Case "Kyocera RiSE"
                    GlossUsed = True
                    DeviceName = "KyoceraRise"
                    ShadowRes = "320x480"
                    IndexW = 65
                    IndexH = 132
                Case "LG Optimus 4X HD"
                    DeviceName = "Optimus4XHD"
                    ShadowRes = "720x1280"
                    IndexW = 92
                    IndexH = 215
                Case "Apple iPhone 5C"
                    If args.var = "Blue" Then
                        DeviceName = "iPhone5CBlue"
                    ElseIf args.var = "Green" Then
                        DeviceName = "iPhone5CGreen"
                    ElseIf args.var = "Red" Then
                        DeviceName = "iPhone5CRed"
                    ElseIf args.var = "White" Then
                        DeviceName = "iPhone5CWhite"
                    ElseIf args.var = "Yellow" Then
                        DeviceName = "iPhone5CYellow"
                    End If
                    ShadowRes = "640x1136"
                    IndexW = 220
                    IndexH = 283
                    UndershadowUsed = True
                Case "Apple iPhone 5S"
                    If args.var = "Black" Then
                        DeviceName = "iPhone5SBlack"
                    ElseIf args.var = "Gold" Then
                        DeviceName = "iPhone5SGold"
                    ElseIf args.var = "White" Then
                        DeviceName = "iPhone5SWhite"
                    ElseIf args.var = "Black Angled" Then
                        DeviceName = "iPhone5SBlackAngle"
                    ElseIf args.var = "Gold Angled" Then
                        DeviceName = "iPhone5SGoldAngle"
                    ElseIf args.var = "White Angled" Then
                        DeviceName = "iPhone5SWhiteAngle"
                    End If
                    If args.var = "Black" Or args.var = "Gold" Or args.var = "White" Then
                        IndexW = 198
                        IndexH = 416
                    ElseIf args.var = "Black Angled" Or args.var = "Gold Angled" Or args.var = "White Angled" Then
                        IndexW = 249
                        IndexH = 354
                        DistortPt1.X = 0
                        DistortPt1.Y = 0
                        DistortPt2.X = 467
                        DistortPt2.Y = 92
                        DistortPt3.X = 467
                        DistortPt3.Y = 1145
                        DistortPt4.X = 0
                        DistortPt4.Y = 1131
                        Perspective = True
                    End If
                    ShadowRes = "640x1136"
                    UndershadowUsed = True
                Case "Sony Xperia S"
                    If args.var = "Black" Then
                        DeviceName = "XperiaSBlack"
                    ElseIf args.var = "White" Then
                        DeviceName = "XperiaSWhite"
                    End If
                    ShadowRes = "720x1280"
                    GlossUsed = True
                    UndershadowUsed = True
                    IndexW = 281
                    IndexH = 301
                Case "Sony Xperia Z"
                    GlossUsed = True
                    DeviceName = "XperiaZ"
                    ShadowRes = "1080x1920"
                    IndexW = 107
                    IndexH = 196
                Case "LG G2"
                    DeviceName = "LGG2"
                    ShadowRes = "1080x1920"
                    GlossUsed = True
                    UndershadowUsed = True
                    IndexW = 190
                    IndexH = 218
                Case "Google Nexus 5"
                    DeviceName = "Nexus5"
                    ShadowRes = "1080x1920"
                    UndershadowUsed = True
                    IndexW = 366
                    IndexH = 282
            End Select
            If Perspective = True Then
                Image1 = FetchImage(databaseurl & "Device/" & DeviceName & ".png")
                If UndershadowUsed = True Then Undershadow = FetchImage(databaseurl & "Undershadow/" & DeviceName & ".png")
                If GlossUsed = True Then Gloss = FetchImage(databaseurl & "Gloss/" & DeviceName & ".png")
                Shadow = FetchImage(databaseurl & "Shadow/" & ShadowRes & ".png")
                If OverlayUsed = True Then Overlay = FetchImage(databaseurl & "Overlay/" & DeviceName & ".png")
                Dim imgtmp2 As New Bitmap(Shadow.Width, Shadow.Height)
                Using graphicsHandle As Graphics = Graphics.FromImage(imgtmp2)
                    graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic
                    graphicsHandle.DrawImage(ScreenCapBitmap, 0, 0, Shadow.Width, Shadow.Height)
                    If ShadowCheckbox.Checked = True Then graphicsHandle.DrawImage((Shadow), New Point(0, 0))
                    ScreenCapBitmap = imgtmp2
                End Using
                Dim filter As New YLScsDrawing.Imaging.Filters.FreeTransform()
                filter.Bitmap = ScreenCapBitmap
                ' assign FourCorners (the four X/Y coords) of the new perspective shape
                ' coords 0-3 are in a clockwise rotation from top left: top left, top right, bottom right, bottom left
                filter.FourCorners = New System.Drawing.PointF() {DistortPt1, DistortPt2, DistortPt3, DistortPt4}
                filter.IsBilinearInterpolation = True
                Dim Background As New Bitmap(Image1.Width, Image1.Height)
                Dim Image3 As New Bitmap(Image1.Width, Image1.Height, PixelFormat.Format32bppArgb)
                Dim g As Graphics = Graphics.FromImage(Image3)
                g.Clear(Color.Transparent)
                g.DrawImage(Background, New Point(0, 0))
                If UnderShadowCheckbox.Checked = True Then g.DrawImage(Undershadow, New Point(0, 0))
                g.DrawImage(Image1, New Point(0, 0))
                g.DrawImage(filter.Bitmap, New Point(IndexW, IndexH))
                If GlossCheckbox.Checked = True Then g.DrawImage(Gloss, New Point(0, 0))
                If OverlayUsed = True Then g.DrawImage(Overlay, New Point(0, 0))
                ' If (args.model = "Apple iPhone 5") Then g.DrawImage(Overlay, New Point(0, 0))
                g.Dispose()
                CanvImg(ScreenPicker.Value) = Image3
            Else
                Image1 = FetchImage(databaseurl & "Device/" & DeviceName & ".png")
                If UndershadowUsed = True Then Undershadow = FetchImage(databaseurl & "Undershadow/" & DeviceName & ".png")
                If GlossUsed = True Then Gloss = FetchImage(databaseurl & "Gloss/" & DeviceName & ".png")
                Shadow = FetchImage(databaseurl & "Shadow/" & ShadowRes & ".png")
                If OverlayUsed = True Then Overlay = FetchImage(databaseurl & "Overlay/" & DeviceName & ".png")
                Dim imgtmp2 As New Bitmap(Shadow.Width, Shadow.Height)
                Using graphicsHandle As Graphics = Graphics.FromImage(imgtmp2)
                    graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic
                    graphicsHandle.DrawImage(ScreenCapBitmap, 0, 0, Shadow.Width, Shadow.Height)
                    ScreenCapBitmap = imgtmp2
                End Using
                If args.model = "Sony Xperia Z" Then
                    Dim imgtmp As New Bitmap(676, 1194)
                    Using graphicsHandle As Graphics = Graphics.FromImage(imgtmp)
                        graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic
                        graphicsHandle.DrawImage(ScreenCapBitmap, 0, 0, 676, 1194)
                        If ShadowCheckbox.Checked = True Then graphicsHandle.DrawImage((Shadow), 0, 0, 676, 1194)
                        ScreenCapBitmap = imgtmp
                    End Using
                ElseIf args.model = "LG G2" Then
                    Dim imgtmp As New Bitmap(637, 1120)
                    Using graphicsHandle As Graphics = Graphics.FromImage(imgtmp)
                        graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic
                        graphicsHandle.DrawImage(ScreenCapBitmap, 0, 0, 637, 1120)
                        If ShadowCheckbox.Checked = True Then graphicsHandle.DrawImage((Shadow), 0, 0, 637, 1120)
                        ScreenCapBitmap = imgtmp
                    End Using
                End If
                Dim Background As New Bitmap(Image1.Width, Image1.Height)
                Dim Image3 As New Bitmap(Image1.Width, Image1.Height, PixelFormat.Format32bppArgb)
                If ReflectBox.Checked = True Then
                    Image3 = New Bitmap(Image1.Width, Image1.Height * (6 / 5), PixelFormat.Format32bppArgb)
                End If
                Dim g As Graphics = Graphics.FromImage(Image3)
                g.Clear(Color.Transparent)
                g.DrawImage(Background, New Point(0, 0))
                If UnderShadowCheckbox.Checked = True Then g.DrawImage(Undershadow, New Point(0, 0))
                g.DrawImage(Image1, New Point(0, 0))
                g.DrawImage(ScreenCapBitmap, New Point(IndexW, IndexH))
                If ShadowCheckbox.Checked = True And ((args.model = "Sony Xperia Z") Or (args.model = "LG G2")) = False Then g.DrawImage((Shadow), New Point(IndexW, IndexH))
                If GlossCheckbox.Checked = True Then g.DrawImage(Gloss, New Point(0, 0))
                If OverlayUsed = True Then g.DrawImage(Overlay, New Point(0, 0))
                ' If (args.model = "Apple iPhone 5") Then g.DrawImage(Overlay, New Point(0, 0))
                g.Dispose()
                If ReflectBox.Checked = True Then
                    Dim g2 As Graphics = Graphics.FromImage(Image3)
                    Dim bm_src1 As Bitmap = New Bitmap(Image1.Width, CType(Image1.Height * (1 / 5), Integer), PixelFormat.Format32bppArgb)
                    bm_src1 = CropBitmap(Image3, 0, Image1.Height * (4 / 5), Image1.Width, Image1.Height * (1 / 5))
                    Dim bm_out As New Bitmap(Image1.Width, CType(Image1.Height * (1 / 5), Integer))
                    Using gr As Graphics = Graphics.FromImage(bm_out)
                        gr.Clear(Color.Transparent)
                        'Flip image
                        gr.TranslateTransform(CSng(bm_src1.Width / 2), CSng(bm_src1.Height / 2))
                        gr.TranslateTransform(-CSng(bm_src1.Width / 2), -CSng(bm_src1.Height / 2))
                        Dim alpha As Integer
                        For x As Integer = 0 To bm_src1.Width - 1
                            For y As Integer = 0 To bm_src1.Height - 1
                                alpha = (255 * y) \ bm_src1.Height
                                Dim clr As Color = bm_src1.GetPixel(x, y)
                                clr = Color.FromArgb(alpha, clr.R, clr.G, clr.B)
                                bm_src1.SetPixel(x, y, clr)
                            Next
                        Next
                        gr.DrawImage(bm_src1, 0, 0)
                        gr.Dispose()
                    End Using
                    bm_out.RotateFlip(RotateFlipType.RotateNoneFlipY)
                    g2.DrawImage(bm_out, New Point(0, Image1.Height))
                    g2.Dispose()
                End If
                CanvImg(ScreenPicker.Value) = Image3
            End If
            
        End If
    End Sub

    Public Class ArgumentType
        Public type As Integer
        Public var As String
        Public model As String
    End Class

    Private Function FetchImage(ByVal url As String)
        Try
            Return New Bitmap(New System.Drawing.Bitmap(New IO.MemoryStream(New System.Net.WebClient().DownloadData(url))))
        Catch ex As Exception
        End Try
        Return New Bitmap(720, 1280)
    End Function

    Private Sub BackgroundDownloader_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundDownloader.RunWorkerCompleted
        Preview.Image = CanvImg(ScreenPicker.Value)
        LoadImage.Image = Nothing
    End Sub

    Private Sub CaptureBtn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CaptureBtn.Click
        adb.ShowDialog()
    End Sub

    Public Shared Sub ADBCapture()
        If Scrotter.IsMono = False Then OpenPath(Scrotter.ScreenPicker.Value) = (Environment.GetEnvironmentVariable("temp") & Path.DirectorySeparatorChar & "capture.png") Else OpenPath(Scrotter.ScreenPicker.Value) = Path.DirectorySeparatorChar & "tmp" & Path.DirectorySeparatorChar & "capture.png"
        Scrotter.RefreshLists()
    End Sub

    Private Sub Scrotter_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim t As Type = Type.[GetType]("Mono.Runtime")
        If t Is Nothing Then IsMono = False Else IsMono = True
        about.CheckForUpdates(False)
        'If IsMono = False Then AppData = System.IO.Directory.Exists(SpecialFolder.ApplicationData & ".scrotter/") Else System.IO.Directory.Exists(SpecialFolder.Personal & ".scrotter/") 'Per-platform specifics are not usually good as code should be consistent, but this is okay for directory structures
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        about.ShowDialog()
    End Sub

    Private Sub ScreenAmountPicker_ValueChanged(sender As Object, e As EventArgs) Handles ScreenAmountPicker.ValueChanged
        If ScreenAmountPicker.Value > 1 Then
            ScreenPicker.Maximum = ScreenAmountPicker.Value
            SaveBtn.Text = "Save Multiple Screens As..."
        Else
            ScreenPicker.Value = 1
            ScreenPicker.Maximum = 1
            SaveBtn.Text = "Save As..."
        End If
    End Sub

    Private Sub EnableMultipleScreens(sender As Object, e As EventArgs) Handles ModelBox.TextChanged
        ScreenAmountPicker.Enabled = True
        ScreenPicker.Enabled = True
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim bm_src1 As Bitmap = New Bitmap(CropBitmap(CanvImg(ScreenPicker.Value), 0, CanvImg(ScreenPicker.Value).Height * (2 / 3), CanvImg(ScreenPicker.Value).Width, CanvImg(ScreenPicker.Value).Height / 3))
        Dim bm_out As New Bitmap(bm_src1.Width, bm_src1.Height)
        Using gr As Graphics = Graphics.FromImage(bm_out)
            'Flip image
            gr.TranslateTransform(CSng(bm_src1.Width / 2), CSng(bm_src1.Height / 2))
            gr.RotateTransform(180)
            gr.TranslateTransform(-CSng(bm_src1.Width / 2), -CSng(bm_src1.Height / 2))
            ' Give the images alpha gradients.
            Dim alpha As Integer
            For x As Integer = 0 To bm_src1.Width - 1
                For y As Integer = 0 To bm_src1.Height - 1
                    alpha = (255 * y) \ bm_src1.Height
                    Dim clr As Color = bm_src1.GetPixel(x, y)
                    clr = Color.FromArgb(alpha, clr.R, clr.G, clr.B)
                    bm_src1.SetPixel(x, y, clr)
                Next
            Next
            ' Draw the images onto the result.
            gr.DrawImage(bm_src1, 0, 0)
        End Using
        ' Display the result.
        Preview.Image = bm_out 'picturebox output
    End Sub

    Private Function CropBitmap(ByRef bmp As Bitmap, ByVal cropX As Integer, ByVal cropY As Integer, ByVal cropWidth As Integer, ByVal cropHeight As Integer) As Bitmap
        Dim rect As New Rectangle(cropX, cropY, cropWidth, cropHeight)
        Dim cropped As Bitmap = bmp.Clone(rect, PixelFormat.Format32bppArgb)
        Return cropped
    End Function

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        My.Computer.Network.DownloadFile("https://github.com/Yttrium-tYcLief/Scrotter/raw/master/latest/scrotter.exe", "C:\scrotter.exe", vbNullString, vbNullString, True, 5000, True)
    End Sub
End Class
