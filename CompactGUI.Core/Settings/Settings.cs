using CommunityToolkit.Mvvm.ComponentModel;
using CompactGUI.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CompactGUI.Core.Settings;

[NotifyPropertyChangedRecipients]
public partial class Settings : ObservableRecipient
{
    // TODO: Add local saving of per-folder skip list

    [ObservableProperty] private decimal settingsVersion = 1.2m; // You may want to inject this value
    [ObservableProperty] private LogLevel logLevel = LogLevel.Information;
    [ObservableProperty] private DateTime resultsDBLastUpdated = DateTime.UnixEpoch;
    [ObservableProperty] private int selectedCompressionMode = 0;
    [ObservableProperty] private bool skipNonCompressable = false;
    [ObservableProperty] private bool skipUserNonCompressable = false;
    [ObservableProperty] private bool watchFolderForChanges = false;
    [ObservableProperty] private List<string> nonCompressableList = new List<string> { ".dl_", ".gif", ".jpg", ".jpeg", ".png", ".wmf", ".mkv", ".mp4", ".wmv", ".avi", ".bik", ".bk2", ".flv", ".ogg", ".mpg", ".m2v", ".m4v", ".vob", ".mp3", ".aac", ".wma", ".flac", ".zip", ".xap", ".rar", ".7z", ".cab", ".lzx", ".docx", ".xlsx", ".pptx", ".vssx", ".vstx", ".onepkg", ".tar", ".gz", ".dmg", ".bz2", ".tgz", ".lz", ".xz", ".txz"};

    [ObservableProperty] private int skipUserFileTypesLevel = 0;
    [ObservableProperty] private bool showNotifications = false;
    [ObservableProperty] private bool startInSystemTray = false;

    [ObservableProperty] private int maxCompressionThreads = 0;
    [ObservableProperty] private bool lockHDDsToOneThread = true;
    [ObservableProperty] private bool estimateCompressionForNonSteamFolders = false;

    [ObservableProperty] private bool enableBackgroundWatcher = true;
    [ObservableProperty] private bool enableBackgroundAutoCompression = true;

    [ObservableProperty] private bool allowMultiInstance = false;
    [ObservableProperty] private bool enablePreReleaseUpdates = true;

    [ObservableProperty] private bool isContextIntegrated = true;
    [ObservableProperty] private bool isStartMenuEnabled = true;

    [ObservableProperty] private double windowTop = 0;
    [ObservableProperty] private double windowLeft = 0;
    [ObservableProperty] private double windowWidth = 1300;
    [ObservableProperty] private double windowHeight = 700;
    [ObservableProperty] private WindowState windowState = WindowState.Normal;

    //partial void OnEnableBackgroundWatcherChanged(bool value)
    //{
    //    Watcher.Watcher.IsWatchingEnabled = value;
    //}

    //partial void OnEnableBackgroundAutoCompressionChanged(bool value)
    //{
    //    Watcher.Watcher.IsBackgroundCompactingEnabled = value;
    //}

  
    //public static void Save()
    //{
    //    SettingsHandler.WriteToFile();
    //}
}

public enum WindowState
{
    Normal,
    Minimized,
    Maximized
}
