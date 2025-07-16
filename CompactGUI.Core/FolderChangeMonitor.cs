using System;
using System.IO;
using System.Threading;

namespace CompactGUI.Core;

/// <summary>
/// Monitors a folder (and subfolders) for changes, with debouncing and proper disposal.
/// </summary>
public sealed class FolderChangeMonitor : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly Timer _debounceTimer;
    private readonly int _debounceMilliseconds;
    private bool _hasChanged;
    private DateTime _lastChanged;
    private bool _disposed;

    /// <summary>
    /// Raised when the folder has changed (debounced).
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    /// True if a change has been detected since the last reset.
    /// </summary>
    public bool HasChanged => _hasChanged;

    /// <summary>
    /// The last time a change was detected.
    /// </summary>
    public DateTime LastChanged => _lastChanged;

    /// <summary>
    /// Create a new FolderChangeMonitor.
    /// </summary>
    /// <param name="folderPath">The folder to monitor.</param>
    /// <param name="debounceMilliseconds">Debounce interval in ms (default: 1000).</param>
    public FolderChangeMonitor(string folderPath, int debounceMilliseconds = 1000)
    {
        _debounceMilliseconds = debounceMilliseconds;
        _watcher = new FileSystemWatcher(folderPath)
        {
            NotifyFilter = NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };
        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.Deleted += OnChanged;
        _watcher.Renamed += OnChanged;
        _watcher.Error += OnError;

        _debounceTimer = new Timer(DebounceCallback, null, Timeout.Infinite, Timeout.Infinite);
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        _debounceTimer.Change(_debounceMilliseconds, Timeout.Infinite);
    }

    private void DebounceCallback(object? state)
    {
        _hasChanged = true;
        _lastChanged = DateTime.Now;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        // Optionally log or handle errors here
    }

    /// <summary>
    /// Reset the change flag after handling.
    /// </summary>
    public void Reset()
    {
        _hasChanged = false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _watcher.Dispose();
        _debounceTimer.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}