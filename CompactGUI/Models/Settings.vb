Imports Microsoft.Toolkit.Mvvm.ComponentModel

Public Class Settings : Inherits ObservableObject

    Public Property SettingsVersion As Decimal = SettingsHandler.SettingsVersion
    Public Property ResultsDBLastUpdated As DateTime = DateTime.UnixEpoch
    Public Property SelectedCompressionMode As Integer = 0
    Public Property SkipNonCompressable As Boolean = False
    Public Property SkipUserNonCompressable As Boolean = False
    Public Property WatchFolderForChanges As Boolean = False
    Public Property NonCompressableList As New List(Of String) From {".dl_", ".gif", ".jpg", ".jpeg", ".png", ".wmf", ".mkv", ".mp4", ".wmv", ".avi", ".bik", ".bk2", ".flv", ".ogg", ".mpg", ".m2v", ".m4v", ".vob", ".mp3", ".aac", ".wma", ".flac", ".zip", ".xap", ".rar", ".7z", ".cab", ".lzx", ".docx", ".xlsx", ".pptx", ".vssx", ".vstx", ".onepkg", ".tar", ".gz", ".dmg", ".bz2", ".tgz", ".lz", ".xz", ".txz"}
    Public Property IsContextIntegrated As Boolean = False
    Public Property IsStartMenuEnabled As Boolean = False
    Public Property SkipUserFileTypesLevel As Integer = 0
    Public Property ShowNotifications As Boolean = False
    Public Property StartInSystemTray As Boolean = False

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

    Private _WindowScalingFactor = 1
    Public Property WindowScalingFactor As Double
        Get
            Return _WindowScalingFactor
        End Get
        Set(value As Double)
            _WindowScalingFactor = value
            WindowWidth = 500 * value
            WindowHeight = 800 * value

            OnPropertyChanged()
        End Set
    End Property

    Public Property WindowWidth As Decimal = 500
    Public Property WindowHeight As Decimal = 800

    Public Property AllowMultiInstance As Boolean = False
    Public Property EnablePreReleaseUpdates As Boolean = True

    'TODO: Add local saving of per-folder skip list
    Public Shared Sub Save()
        SettingsHandler.WriteToFile()
    End Sub

End Class
