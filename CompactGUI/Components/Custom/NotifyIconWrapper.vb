Imports System.ComponentModel
Imports System.Drawing
Imports System.Reflection
Imports System.Windows.Forms

Public Class NotifyIconWrapper : Inherits FrameworkElement : Implements IDisposable


    Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register("Text", GetType(String), GetType(NotifyIconWrapper), New PropertyMetadata(Nothing, Sub(d, e)
                                                                                                                                                                                   Dim notifyIcon = CType(d, NotifyIconWrapper)._notifyIcon
                                                                                                                                                                                   If notifyIcon Is Nothing Then Return
                                                                                                                                                                                   notifyIcon.Text = CType(e.NewValue, String)
                                                                                                                                                                               End Sub))

    Private Shared ReadOnly NotifyRequestProperty As DependencyProperty = DependencyProperty.Register("NotifyRequest", GetType(NotifyRequestRecord), GetType(NotifyIconWrapper), New PropertyMetadata(Nothing, Sub(d, e)
                                                                                                                                                                                                                   Dim r = CType(e.NewValue, NotifyRequestRecord)
                                                                                                                                                                                                                   CType(d, NotifyIconWrapper)._notifyIcon?.ShowBalloonTip(r.Duration, r.Title, r.Text, r.Icon)
                                                                                                                                                                                                               End Sub))

    Private Shared ReadOnly OpenSelectedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("OpenSelected", RoutingStrategy.Direct, GetType(RoutedEventHandler), GetType(NotifyIconWrapper))

    Private Shared ReadOnly ExitSelectedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("ExitSelected", RoutingStrategy.Direct, GetType(RoutedEventHandler), GetType(NotifyIconWrapper))

    Private ReadOnly _notifyIcon As NotifyIcon


    Public Sub New()

        If DesignerProperties.GetIsInDesignMode(Me) Then Return
        _notifyIcon = New NotifyIcon With {
            .Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
            .Visible = True,
            .ContextMenuStrip = CreateContextMenu()}

        AddHandler _notifyIcon.DoubleClick, AddressOf OpenItemOnClick
        AddHandler Application.Current.Exit, Sub(obj, args)
                                                 _notifyIcon.Dispose()
                                             End Sub

    End Sub

    Public Property Text As String
        Get
            Return GetValue(TextProperty)
        End Get
        Set(value As String)
            SetValue(TextProperty, value)
        End Set
    End Property

    Public Property NotifyRequest As NotifyRequestRecord
        Get
            Return GetValue(NotifyRequestProperty)
        End Get
        Set(value As NotifyRequestRecord)
            SetValue(NotifyRequestProperty, value)
        End Set
    End Property


    Public Sub Dispose() Implements IDisposable.Dispose
        _notifyIcon.Dispose()
    End Sub


    Public Custom Event OpenSelected As RoutedEventHandler
        AddHandler(ByVal value As RoutedEventHandler)
            [AddHandler](OpenSelectedEvent, value)
        End AddHandler
        RemoveHandler(ByVal value As RoutedEventHandler)
            [RemoveHandler](OpenSelectedEvent, value)
        End RemoveHandler
        RaiseEvent(sender As Object, e As RoutedEventArgs)
            [RaiseEvent](e)
        End RaiseEvent
    End Event

    Public Custom Event ExitSelected As RoutedEventHandler
        AddHandler(ByVal value As RoutedEventHandler)
            [AddHandler](ExitSelectedEvent, value)
        End AddHandler
        RemoveHandler(ByVal value As RoutedEventHandler)
            [RemoveHandler](ExitSelectedEvent, value)
        End RemoveHandler
        RaiseEvent(sender As Object, e As RoutedEventArgs)
            [RaiseEvent](e)
        End RaiseEvent
    End Event


    Private Function CreateContextMenu() As ContextMenuStrip
        Dim openItem = New ToolStripMenuItem("Open")
        AddHandler openItem.Click, AddressOf OpenItemOnClick
        Dim exitItem = New ToolStripMenuItem("Exit")
        AddHandler exitItem.Click, AddressOf ExitItemOnClick
        Dim contextMenu = New ContextMenuStrip
        contextMenu.Items.AddRange({openItem, exitItem})

        Return contextMenu

    End Function

    Private Sub OpenItemOnClick(sender As Object, e As EventArgs)
        Dim args = New RoutedEventArgs(OpenSelectedEvent)
        [RaiseEvent](args)
    End Sub

    Private Sub ExitItemOnClick(sender As Object, e As EventArgs)
        Dim args = New RoutedEventArgs(ExitSelectedEvent)
        [RaiseEvent](args)
    End Sub

    Public Class NotifyRequestRecord

        Public Property Title As String = ""
        Public Property Text As String = ""
        Public Property Duration As Integer = 1000
        Public Property Icon As ToolTipIcon = ToolTipIcon.Info

    End Class


End Class
