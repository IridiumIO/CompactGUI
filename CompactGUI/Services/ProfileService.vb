Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.Json

Public Class ProfileService
    Private ReadOnly _profiles As New ObservableCollection(Of CompressionProfile)
    Private ReadOnly _profileFilePath As String
    
    Public ReadOnly Property Profiles As ObservableCollection(Of CompressionProfile)
        Get
            Return _profiles
        End Get
    End Property
    
    Public Sub New()
        _profileFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CompactGUI", "compression_profiles.json")
        
        ' Create directory if it doesn't exist
        Dim directory = Path.GetDirectoryName(_profileFilePath)
        If Not Directory.Exists(directory) Then
            Directory.CreateDirectory(directory)
        End If
        
        LoadProfiles()
        
        ' Create default profiles if none exist
        If _profiles.Count = 0 Then
            CreateDefaultProfiles()
        End If
    End Sub
    
    Private Sub CreateDefaultProfiles()
        ' Game profile
        Dim gameProfile = New CompressionProfile With {
            .Name = "Game Optimized",
            .Description = "Optimized for game files with balanced compression and performance",
            .CompressionMode = 1, ' XPRESS8K
            .SkipNonCompressable = True,
            .SkipUserNonCompressable = True,
            .WatchFolderForChanges = True,
            .IsDefault = True
        }
        
        ' Maximum compression profile
        Dim maxCompressionProfile = New CompressionProfile With {
            .Name = "Maximum Compression",
            .Description = "Maximum compression with LZX algorithm (slower but better compression)",
            .CompressionMode = 3, ' LZX
            .SkipNonCompressable = True,
            .SkipUserNonCompressable = True,
            .WatchFolderForChanges = False
        }
        
        ' Fast compression profile
        Dim fastCompressionProfile = New CompressionProfile With {
            .Name = "Fast Compression",
            .Description = "Faster compression with XPRESS4K algorithm (less compression but faster)",
            .CompressionMode = 0, ' XPRESS4K
            .SkipNonCompressable = True,
            .SkipUserNonCompressable = True,
            .WatchFolderForChanges = False
        }
        
        ' Document profile
        Dim documentProfile = New CompressionProfile With {
            .Name = "Document Optimized",
            .Description = "Optimized for document files with good compression",
            .CompressionMode = 2, ' XPRESS16K
            .SkipNonCompressable = True,
            .SkipUserNonCompressable = False,
            .WatchFolderForChanges = False
        }
        
        _profiles.Add(gameProfile)
        _profiles.Add(maxCompressionProfile)
        _profiles.Add(fastCompressionProfile)
        _profiles.Add(documentProfile)
        
        SaveProfiles()
    End Sub
    
    Public Sub AddProfile(profile As CompressionProfile)
        ' If this is set as default, unset any other default
        If profile.IsDefault Then
            For Each p In _profiles
                If p.IsDefault Then
                    p.IsDefault = False
                End If
            Next
        End If
        
        _profiles.Add(profile)
        SaveProfiles()
    End Sub
    
    Public Sub UpdateProfile(profile As CompressionProfile)
        Dim existingProfile = _profiles.FirstOrDefault(Function(p) p.Id = profile.Id)
        If existingProfile IsNot Nothing Then
            ' If this is set as default, unset any other default
            If profile.IsDefault AndAlso Not existingProfile.IsDefault Then
                For Each p In _profiles
                    If p.IsDefault AndAlso p.Id <> profile.Id Then
                        p.IsDefault = False
                    End If
                Next
            End If
            
            Dim index = _profiles.IndexOf(existingProfile)
            _profiles(index) = profile
            SaveProfiles()
        End If
    End Sub
    
    Public Sub RemoveProfile(profile As CompressionProfile)
        _profiles.Remove(profile)
        SaveProfiles()
    End Sub
    
    Public Function GetDefaultProfile() As CompressionProfile
        Return _profiles.FirstOrDefault(Function(p) p.IsDefault)
    End Function
    
    Public Sub SaveProfiles()
        Try
            Dim options = New JsonSerializerOptions With {
                .WriteIndented = True
            }
            Dim json = JsonSerializer.Serialize(_profiles, options)
            File.WriteAllText(_profileFilePath, json)
        Catch ex As Exception
            Debug.WriteLine($"Error saving compression profiles: {ex.Message}")
        End Try
    End Sub
    
    Public Sub LoadProfiles()
        Try
            If File.Exists(_profileFilePath) Then
                Dim json = File.ReadAllText(_profileFilePath)
                Dim loadedProfiles = JsonSerializer.Deserialize(Of List(Of CompressionProfile))(json)
                _profiles.Clear()
                For Each profile In loadedProfiles
                    _profiles.Add(profile)
                Next
            End If
        Catch ex As Exception
            Debug.WriteLine($"Error loading compression profiles: {ex.Message}")
        End Try
    End Sub
End Class