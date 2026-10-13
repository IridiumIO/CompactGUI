using CompactGUI.Core;

namespace CompactGUI.Tests;

public class PathVerificationTests
{
    [Theory]
    [InlineData(@"C:\Windows", @"C:\Windows", true)]
    [InlineData(@"C:\Windows\System32", @"C:\Windows", true)]
    [InlineData(@"C:\WindowsBackup", @"C:\Windows", false)]
    [InlineData(@"C:\Users\Sam\OneDrive - Personal\Games", @"C:\Users\Sam\OneDrive - Personal", true)]
    [InlineData(@"C:\Users\Sam\OneDrive - Personal Backup", @"C:\Users\Sam\OneDrive - Personal", false)]
    public void DirectoryMatchRequiresPathBoundary(string path, string directory, bool expected)
    {
        Assert.Equal(expected, SharedMethods.IsPathWithinDirectory(path, directory));
    }
}
