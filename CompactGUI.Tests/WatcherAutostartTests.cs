using Microsoft.Win32;

namespace CompactGUI.Tests;

public class WatcherAutostartTests
{
    [Fact]
    public void RunEntryFollowsWatchedFolderCountAndQuotesExecutablePath()
    {
        var subkeyPath = $@"Software\CompactGUI.WatcherTests.{Guid.NewGuid():N}";
        try
        {
            using var testRoot = Registry.CurrentUser.CreateSubKey(subkeyPath);
            Assert.NotNull(testRoot);
            const string executablePath = @"C:\Program Files\CompactGUI\CompactGUI.exe";

            Assert.True(CompactGUI.Watcher.Watcher.UpdateRegistryBasedOnWatchedFolders(testRoot, "Run", 0, executablePath));
            Assert.Null(testRoot.OpenSubKey("Run"));

            Assert.True(CompactGUI.Watcher.Watcher.UpdateRegistryBasedOnWatchedFolders(testRoot, "Run", 1, executablePath));
            using var testKey = testRoot.OpenSubKey("Run");
            Assert.NotNull(testKey);
            Assert.Equal(@"""C:\Program Files\CompactGUI\CompactGUI.exe"" -tray", testKey.GetValue("CompactGUI"));

            Assert.True(CompactGUI.Watcher.Watcher.UpdateRegistryBasedOnWatchedFolders(testRoot, "Run", 2, executablePath));
            Assert.Equal(@"""C:\Program Files\CompactGUI\CompactGUI.exe"" -tray", testKey.GetValue("CompactGUI"));

            Assert.True(CompactGUI.Watcher.Watcher.UpdateRegistryBasedOnWatchedFolders(testRoot, "Run", 1, executablePath));
            Assert.Equal(@"""C:\Program Files\CompactGUI\CompactGUI.exe"" -tray", testKey.GetValue("CompactGUI"));

            Assert.True(CompactGUI.Watcher.Watcher.UpdateRegistryBasedOnWatchedFolders(testRoot, "Run", 0, executablePath));
            Assert.Null(testKey.GetValue("CompactGUI"));

            using var readOnlyRoot = Registry.CurrentUser.OpenSubKey(subkeyPath, writable: false);
            Assert.NotNull(readOnlyRoot);
            Assert.False(CompactGUI.Watcher.Watcher.UpdateRegistryBasedOnWatchedFolders(readOnlyRoot, "Denied", 1, executablePath));
            Assert.Null(testRoot.OpenSubKey("Denied"));
        }
        finally
        {
            Registry.CurrentUser.DeleteSubKeyTree(subkeyPath, throwOnMissingSubKey: false);
        }
    }
}
