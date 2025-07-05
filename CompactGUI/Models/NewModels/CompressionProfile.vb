Imports System.Collections.ObjectModel
Imports CommunityToolkit.Mvvm.ComponentModel
Imports System.Text.Json.Serialization

Public Class CompressionProfile : Inherits ObservableObject
    
    Public Property Name As String
    Public Property Description As String
    Public Property CompressionMode As Core.CompressionMode
    Public Property SkipPoorlyCompressedFileTypes As Boolean
    Public Property SkipUserSubmittedFiletypes As Boolean
    Public Property WatchFolderForChanges As Boolean
    Public Property MaxThreads As Integer
    Public Property CustomExclusionList As New List(Of String)
    Public Property TargetFileTypes As New List(Of String) ' Specific file types to target
    Public Property MinimumFileSize As Long = 1024 ' Skip files smaller than this
    Public Property IsDefault As Boolean = False
    Public Property CreatedDate As DateTime = DateTime.Now
    Public Property LastUsed As DateTime = DateTime.MinValue
    
    ' Predefined profiles
    Public Shared ReadOnly Property GameProfile As CompressionProfile
        Get
            Return New CompressionProfile With {
                .Name = "Gaming Optimized",
                .Description = "Optimized for game files with balanced compression",
                .CompressionMode = Core.CompressionMode.XPRESS8K,
                .SkipPoorlyCompressedFileTypes = True,
                .SkipUserSubmittedFiletypes = True,
                .WatchFolderForChanges = True,
                .MaxThreads = Environment.ProcessorCount,
                .CustomExclusionList = New List(Of String) From {".exe", ".dll", ".pak", ".cache"},
                .MinimumFileSize = 4096
            }
        End Get
    End Property
    
    Public Shared ReadOnly Property MaxCompressionProfile As CompressionProfile
        Get
            Return New CompressionProfile With {
                .Name = "Maximum Compression",
                .Description = "Highest compression ratio, slower processing",
                .CompressionMode = Core.CompressionMode.LZX,
                .SkipPoorlyCompressedFileTypes = False,
                .SkipUserSubmittedFiletypes = False,
                .WatchFolderForChanges = False,
                .MaxThreads = 1,
                .MinimumFileSize = 1024
            }
        End Get
    End Property
    
    Public Shared ReadOnly Property FastProfile As CompressionProfile
        Get
            Return New CompressionProfile With {
                .Name = "Fast Compression",
                .Description = "Quick compression with minimal CPU impact",
                .CompressionMode = Core.CompressionMode.XPRESS4K,
                .SkipPoorlyCompressedFileTypes = True,
                .SkipUserSubmittedFiletypes = True,
                .WatchFolderForChanges = True,
                .MaxThreads = Environment.ProcessorCount,
                .MinimumFileSize = 8192
            }
        End Get
    End Property
    
    Public Function Clone() As CompressionProfile
        Return New CompressionProfile With {
            .Name = Name,
            .Description = Description,
            .CompressionMode = CompressionMode,
            .SkipPoorlyCompressedFileTypes = SkipPoorlyCompressedFileTypes,
            .SkipUserSubmittedFiletypes = SkipUserSubmittedFiletypes,
            .WatchFolderForChanges = WatchFolderForChanges,
            .MaxThreads = MaxThreads,
            .CustomExclusionList = New List(Of String)(CustomExclusionList),
            .TargetFileTypes = New List(Of String)(TargetFileTypes),
            .MinimumFileSize = MinimumFileSize
        }
    End Function
    
    Public Sub ApplyToCompressionOptions(options As CompressionOptions)
        options.SelectedCompressionMode = CompressionMode
        options.SkipPoorlyCompressedFileTypes = SkipPoorlyCompressedFileTypes
        options.SkipUserSubmittedFiletypes = SkipUserSubmittedFiletypes
        options.WatchFolderForChanges = WatchFolderForChanges
    End Sub
    
End Class

Public Class CompressionProfileManager : Inherits ObservableObject
    
    Public Property Profiles As New ObservableCollection(Of CompressionProfile)
    Private ReadOnly _profilesFile As String
    
    Public Sub New()
        _profilesFile = Path.Combine(SettingsHandler.DataFolder.FullName, "compression_profiles.json")
        LoadProfiles()
        
        ' Add default profiles if none exist
        If Not Profiles.Any() Then
            AddDefaultProfiles()
        End If
    End Sub
    
    Private Sub AddDefaultProfiles()
        Profiles.Add(CompressionProfile.GameProfile)
        Profiles.Add(CompressionProfile.MaxCompressionProfile)
        Profiles.Add(CompressionProfile.FastProfile)
        SaveProfiles()
    End Sub
    
    Public Sub AddProfile(profile As CompressionProfile)
        Profiles.Add(profile)
        SaveProfiles()
    End Sub
    
    Public Sub RemoveProfile(profile As CompressionProfile)
        If Not profile.IsDefault Then
            Profiles.Remove(profile)
            SaveProfiles()
        End If
    End Sub
    
    Public Sub UpdateProfile(profile As CompressionProfile)
        profile.LastUsed = DateTime.Now
        SaveProfiles()
    End Sub
    
    Private Sub LoadProfiles()
        Try
            If File.Exists(_profilesFile) Then
                Dim json = File.ReadAllText(_profilesFile)
                Dim loadedProfiles = System.Text.Json.JsonSerializer.Deserialize(Of List(Of CompressionProfile))(json)
                Profiles.Clear()
                For Each profile In loadedProfiles
                    Profiles.Add(profile)
                Next
            End If
        Catch ex As Exception
            Debug.WriteLine($"Error loading profiles: {ex.Message}")
        End Try
    End Sub
    
    Private Sub SaveProfiles()
        Try
            Dim json = System.Text.Json.JsonSerializer.Serialize(Profiles.ToList(), New System.Text.Json.JsonSerializerOptions With {.WriteIndented = True})
            File.WriteAllText(_profilesFile, json)
        Catch ex As Exception
            Debug.WriteLine($"Error saving profiles: {ex.Message}")
        End Try
    End Sub
    
End Class