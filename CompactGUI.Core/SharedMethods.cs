using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Windows.Win32;

using Windows.Win32.System.Power;

namespace CompactGUI.Core;

public static class SharedMethods
{


    public enum FolderVerificationResult
    {
        Valid = 0,
        DirectoryDoesNotExist,
        SystemDirectory,
        RootDirectory,
        DirectoryEmptyOrUnauthorized,
        OneDriveFolder,
        NonNTFSDrive,
        InsufficientPermission,
        LZNT1Compressed
    }

    public static FolderVerificationResult VerifyFolder(string folder)
    {
        if (!Directory.Exists(folder))
            return FolderVerificationResult.DirectoryDoesNotExist;
        else if (folder.ToLowerInvariant().Contains(Environment.GetFolderPath(Environment.SpecialFolder.Windows).ToLowerInvariant()))
            return FolderVerificationResult.SystemDirectory;
        else if (folder.EndsWith(":\\"))
            return FolderVerificationResult.RootDirectory;
        else if (IsDirectoryEmptySafe(folder))
            return FolderVerificationResult.DirectoryEmptyOrUnauthorized;
        else if (IsOneDriveFolder(folder))
            return FolderVerificationResult.OneDriveFolder;
        else if (DriveInfo.GetDrives().First(f => folder.StartsWith(f.Name)).DriveFormat != "NTFS")
            return FolderVerificationResult.NonNTFSDrive;
        else if (!HasDirectoryWritePermission(folder))
            return FolderVerificationResult.InsufficientPermission;
        else if (IsFolderLZNT1Compressed(folder))
            return FolderVerificationResult.LZNT1Compressed; // LZNT1 compressed folders are not supported for compression

        return FolderVerificationResult.Valid;
    }

    public static string GetFolderVerificationMessage(FolderVerificationResult result)
    {
        return result switch
        {
            FolderVerificationResult.Valid => "",
            FolderVerificationResult.DirectoryDoesNotExist => "Directory does not exist",
            FolderVerificationResult.SystemDirectory => "Cannot compress system directory",
            FolderVerificationResult.RootDirectory => "Cannot compress root directory",
            FolderVerificationResult.DirectoryEmptyOrUnauthorized => "This directory is either empty or you are not authorized to access its files.",
            FolderVerificationResult.OneDriveFolder => "Files synced with OneDrive cannot be compressed as they use a different storage structure",
            FolderVerificationResult.NonNTFSDrive => "Cannot compress a directory on a non-NTFS drive",
            FolderVerificationResult.InsufficientPermission => "Insufficient permission to access this folder.",
            FolderVerificationResult.LZNT1Compressed => "Folders using Windows' compression are not supported. Disable Windows compression on this folder first.",
            _ => "Unknown error"
        };
    }


    static bool IsDirectoryEmptySafe(string folderPath)
    {
        try
        {
            return !Directory.EnumerateFileSystemEntries(folderPath).Any();
        }
        catch (Exception)
        {
            return false; // Assume empty for any other exception
        }
    }


    static bool IsOneDriveFolder(string folderPath)
    {
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        List<string> oneDrivePaths = new List<string>
        {
            Path.Combine(userProfile, "OneDrive"), // Personal OneDrive
            Path.Combine(userProfile, "OneDrive - Personal"), // Alternative Personal OneDrive
            Path.Combine(userProfile, "OneDrive for Business"), // OneDrive for Business
            Path.Combine(userProfile, "OneDrive - Business") // Alternative OneDrive for Business
        };

        string normalizedFolderPath = Path.GetFullPath(folderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLowerInvariant();

        return oneDrivePaths.Any(oneDrivePath =>
                    normalizedFolderPath.StartsWith(Path.GetFullPath(oneDrivePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLowerInvariant()));

    }

    public static void PreventSleep()
    {
        PInvoke.SetThreadExecutionState(
            EXECUTION_STATE.ES_CONTINUOUS |
            EXECUTION_STATE.ES_SYSTEM_REQUIRED |
            EXECUTION_STATE.ES_DISPLAY_REQUIRED);
    }


    public static void RestoreSleep()
    {
        PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
    }












    public static IEnumerable<string> AsShortPathNames(this IEnumerable<string> filesList)
    {
        return filesList.Select(file => (file.Length >= 255 ? GetShortPath(file) ?? file : file));
    }


    private static string GetShortPath(string filePath)
    {
        const string LongPathPrefix = @"\\?\";
        ReadOnlySpan<char> longPathPrefixSpan = LongPathPrefix;

        if (string.IsNullOrWhiteSpace(filePath)) return filePath;
        ReadOnlySpan<char> filePathSpan = filePath;

        bool addPrefix = filePathSpan.Length >= 255 && !filePathSpan.StartsWith(longPathPrefixSpan, StringComparison.Ordinal);
        string pathToUse = addPrefix ? LongPathPrefix + filePath : filePath;

        Span<char> shortPath = stackalloc char[1024];
        uint res = PInvoke.GetShortPathName(pathToUse, shortPath);
        if (res == 0) return filePath;

        ReadOnlySpan<char> resultSpan = shortPath[..(int)res];
        return addPrefix && resultSpan.StartsWith(longPathPrefixSpan, StringComparison.Ordinal)
            ? resultSpan[longPathPrefixSpan.Length..].ToString()
            : resultSpan.ToString();
    }



    public static unsafe uint GetClusterSize(string folderPath)
    {
        UInt32 lpSectorsPerCluster;
        UInt32 lpBytesPerSector;

        PInvoke.GetDiskFreeSpace(
            new DirectoryInfo(folderPath).Root.ToString(),
            &lpSectorsPerCluster,
            &lpBytesPerSector,
            null,
            null
        );

        return lpSectorsPerCluster * lpBytesPerSector;

    }


    public static unsafe long GetFileSizeOnDisk(string file)
    {
        uint highOrder;
        uint lowOrder = PInvoke.GetCompressedFileSize(file, &highOrder);
        if (lowOrder == 0xFFFFFFFF && (Marshal.GetLastWin32Error() != 0)) return -1;
        return ((long)highOrder << 32) | lowOrder;
    }


    public static bool HasDirectoryWritePermission(string folderName)
    {
        try
        {
            DirectoryInfo dirInfo = new DirectoryInfo(folderName);
            DirectorySecurity dirSecurity = dirInfo.GetAccessControl();

            var user = WindowsIdentity.GetCurrent();
            var userSID = user.User;
            var userGroupSIDs = user.Groups;

            var rules = dirSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));

            bool writeAllowed = false;
            bool writeDenied = false;

            foreach (FileSystemAccessRule rule in rules)
            {
                var fileSystemRights = rule.FileSystemRights;

                if (!rule.FileSystemRights.HasFlag(FileSystemRights.Write)) continue;
                if (rule.IdentityReference is not SecurityIdentifier ruleSID) continue;
                if (!ruleSID.Equals(userSID) && !userGroupSIDs.Contains(ruleSID)) continue;

                if (rule.AccessControlType == AccessControlType.Deny)
                {
                    writeDenied = true;
                    break;
                }

                if (rule.AccessControlType == AccessControlType.Allow) writeAllowed = true;
            }

            return writeAllowed && !writeDenied;

        }
        catch (UnauthorizedAccessException) { return false; }
    }

    public static bool IsFolderLZNT1Compressed(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Directory not found: {folderPath}");

        var attributes = File.GetAttributes(folderPath);
        return attributes.HasFlag(FileAttributes.Compressed);
    }




}
