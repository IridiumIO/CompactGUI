Imports System.IO

Public Class FileFolderDialog
    Inherits CommonDialog

    Private dg As New OpenFileDialog()

    Public Property Dialog() As OpenFileDialog = dg

    Public Shadows Function ShowDialog() As DialogResult
        Return ShowDialog(Nothing)
    End Function

    Public Shadows Function ShowDialog(owner As IWin32Window) As DialogResult
        With dg
            .ValidateNames = False
            .CheckFileExists = False
            .CheckPathExists = True
            .Multiselect = True
            .FileName = "#Folder"
            .Title = "Select Folder or Files"
            .Filter = "Folder|#Folder|All Files(*.*)|*.*"
        End With

        If owner Is Nothing Then
            Return dg.ShowDialog()
        Else
            Return dg.ShowDialog(owner)
        End If
    End Function


    Public Property SelectedPath() As String
        Get

            If dg.FileName.EndsWith("#Folder") Then
                Return Path.GetDirectoryName(dg.FileName)
            ElseIf File.Exists(dg.FileName) = True Then
                Return dg.FileName
            Else
                Return Path.GetDirectoryName(dg.FileName)
            End If

        End Get
        Set
            If Value IsNot Nothing AndAlso Value <> "" Then
                dg.FileName = Value
            End If
        End Set
    End Property


    Public ReadOnly Property MultipleFiles() As ArrayList
        Get
            If dg.FileNames IsNot Nothing AndAlso dg.FileNames.Length > 1 Then
                Dim sb As New ArrayList
                For Each fileName As String In dg.FileNames
                    If File.Exists(fileName) Then sb.Add(fileName)
                Next

                Return sb
            Else
                Return Nothing
            End If
        End Get
    End Property

    Public Overrides Sub Reset()
        dg.Reset()
    End Sub

    Protected Overrides Function RunDialog(hwndOwner As IntPtr) As Boolean
        Return True
    End Function
End Class



Public Class GraphicsPanel
    Inherits Panel
    Private Const WS_EX_COMPOSITED As Integer = &H2000000

    Protected Overrides ReadOnly Property CreateParams() As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_COMPOSITED
            Return cp
        End Get
    End Property

    Public Sub New()
        Me.DoubleBuffered = True
    End Sub
End Class

