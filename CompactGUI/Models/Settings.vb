Imports CommunityToolkit.Mvvm.ComponentModel

Imports IWshRuntimeLibrary

Public Class Settings : Inherits ObservableObject
    'TODO: Add local saving of per-folder skip list
    Public Property SettingsVersion As Decimal = SettingsHandler.SettingsVersion
    Public Property ResultsDBLastUpdated As DateTime = DateTime.UnixEpoch
    Public Property SelectedCompressionMode As Integer = 0
    Public Property SkipNonCompressable As Boolean = False
    Public Property SkipUserNonCompressable As Boolean = False
    Public Property WatchFolderForChanges As Boolean = False
    Public Property NonCompressableList As New List(Of String) From {".dl_", ".gif", ".jpg", ".jpeg", ".png", ".wmf", ".mkv", ".mp4", ".wmv", ".avi", ".bik", ".bk2", ".flv", ".ogg", ".mpg", ".m2v", ".m4v", ".vob", ".mp3", ".aac", ".wma", ".flac", ".zip", ".xap", ".rar", ".7z", ".cab", ".lzx", ".docx", ".xlsx", ".pptx", ".vssx", ".vstx", ".onepkg", ".tar", ".gz", ".dmg", ".bz2", ".tgz", ".lz", ".xz", ".txz"}

    Public Property SkipUserFileTypesLevel As Integer = 0
    Public Property ShowNotifications As Boolean = False
    Public Property StartInSystemTray As Boolean = False

    Public Property MaxCompressionThreads As Integer = 0
    Public Property LockHDDsToOneThread As Boolean = True
    Public Property EstimateCompressionForNonSteamFolders As Boolean = False
    
    ' New settings for advanced features
    Public Property EnableScheduledTasks As Boolean = True
    Public Property EnableBatchProcessing As Boolean = True
    Public Property EnableCompressionProfiles As Boolean = True
    Public Property EnableStatisticsTracking As Boolean = True
    Public Property EnableFileTypeAnalysis As Boolean = True
    Public Property SchedulerCheckInterval As Integer = 60 ' In seconds
    Public Property DefaultCompressionProfileId As Guid? = Nothing

    Private _EnableBackgroundWatcher As Boolean = True
    Public Property EnableBackgroundWatcher As Boolean
        Get
            Return _EnableBackgroundWatcher
        End Get
        Set(value As Boolean)
            _EnableBackgroundWatcher = value
            Watcher.Watcher.IsWatchingEnabled = value
        End Set
    End Property

    Private _EnableBackgroundAutoCompression As Boolean = True
    Public Property EnableBackgroundAutoCompression As Boolean
        Get
            Return _EnableBackgroundAutoCompression
        End Get
        Set(value As Boolean)
            _EnableBackgroundAutoCompression = value
            Watcher.Watcher.IsBackgroundCompactingEnabled = value
        End Set
    End Property



    Public Property AllowMultiInstance As Boolean = False
    Public Property EnablePreReleaseUpdates As Boolean = True

    Private _IsContextIntegrated As Boolean = True
    Public Property IsContextIntegrated As Boolean
        Get
            Return _IsContextIntegrated
        End Get
        Set(value As Boolean)
            _IsContextIntegrated = value
            If _IsContextIntegrated Then
                AddContextMenus()
            Else
                RemoveContextMenus()
            End If
        End Set
    End Property

    Private _IsStartMenuEnabled As Boolean = True
    Public Property IsStartMenuEnabled As Boolean
        Get
            Return _IsStartMenuEnabled
        End Get
        Set(value As Boolean)
            _IsStartMenuEnabled = value
            If _IsStartMenuEnabled Then
                CreateStartMenuShortcut()
            Else
                DeleteStartMenuShortcut()
            End If
        End Set
    End Property


    Public Property WindowTop As Double = 0
    Public Property WindowLeft As Double = 0
    Public Property WindowWidth As Double = 1300
    Public Property WindowHeight As Double = 700
    Public Property WindowState As WindowState = WindowState.Normal


    Public Shared Async Function AddContextMenus() As Task
        Await Task.Run(Sub()
                           Try
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "", "Compress Folder")
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI", "Icon", Environment.ProcessPath)
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\shell\CompactGUI\command", "", Environment.ProcessPath & " " & """%1""")
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\Background\shell\CompactGUI", "", "Compress Folder")
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\Background\shell\CompactGUI", "Icon", Environment.ProcessPath)
                               Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\Software\Classes\Directory\Background\shell\CompactGUI\command", "", Environment.ProcessPath & " " & """%V""")

                           Catch ex As Exception
                               Debug.WriteLine(ex.Message)
                           End Try
                       End Sub)
    End Function

    Public Shared Async Function RemoveContextMenus() As Task
        Await Task.Run(Sub()
                           Try
                               Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI\command")
                               Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\shell\\CompactGUI")
                               Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\Background\\shell\\CompactGUI\command")
                               Microsoft.Win32.Registry.CurrentUser.DeleteSubKey("Software\\Classes\\Directory\\Background\\shell\\CompactGUI")
                           Catch ex As Exception
                               Debug.WriteLine(ex.Message)
                           End Try
                       End Sub)
    End Function


    Public Shared Sub CreateStartMenuShortcut()
        Dim wshShell As New WshShell()
        Dim startMenuPath As String = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)
        Dim shortcutPath As String = IO.Path.Combine(startMenuPath, "CompactGUI.lnk")

        Dim shortcut As IWshShortcut = DirectCast(wshShell.CreateShortcut(shortcutPath), IWshShortcut)
        With shortcut
            .TargetPath = Environment.ProcessPath ' Path to the executable
            .WorkingDirectory = IO.Path.GetDirectoryName(Environment.ProcessPath)
            .IconLocation = Environment.ProcessPath ' Path to the executable or icon file
            .Description = "CompactGUI"
            .Save()
        End With
    End Sub

    Public Shared Sub DeleteStartMenuShortcut()
        Dim startMenuPath As String = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)
        Dim shortcutPath As String = IO.Path.Combine(startMenuPath, "CompactGUI.lnk")

        If IO.File.Exists(shortcutPath) Then
            IO.File.Delete(shortcutPath)
        End If
    End Sub

    Public Shared Sub Save()
        SettingsHandler.WriteToFile()
    End Sub

End Class
