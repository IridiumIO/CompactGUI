Imports System.Collections.ObjectModel

Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Public Class ProfilesViewModel : Inherits ObservableObject
    Private ReadOnly _profileService As ProfileService
    
    Public Property Profiles As New ObservableCollection(Of CompressionProfile)
    Public Property SelectedProfile As CompressionProfile
    Public Property EditingProfile As CompressionProfile
    Public Property IsEditing As Boolean = False
    
    Public Sub New(profileService As ProfileService)
        _profileService = profileService
        
        LoadProfiles()
    End Sub
    
    Private Sub LoadProfiles()
        Profiles.Clear()
        For Each profile In _profileService.Profiles
            Profiles.Add(profile)
        Next
    End Sub
    
    Public ReadOnly Property CreateProfileCommand As IRelayCommand = New RelayCommand(AddressOf CreateProfile)
    
    Private Sub CreateProfile()
        EditingProfile = New CompressionProfile With {
            .Name = "New Profile",
            .Description = "Description of the new profile",
            .CompressionMode = 1, ' XPRESS8K by default
            .SkipNonCompressable = True,
            .SkipUserNonCompressable = True,
            .WatchFolderForChanges = False,
            .MaxCompressionThreads = 0
        }
        
        IsEditing = True
        OnPropertyChanged(NameOf(EditingProfile))
    End Sub
    
    Public ReadOnly Property EditProfileCommand As IRelayCommand = New RelayCommand(AddressOf EditProfile, AddressOf CanEditProfile)
    
    Private Function CanEditProfile() As Boolean
        Return SelectedProfile IsNot Nothing
    End Function
    
    Private Sub EditProfile()
        EditingProfile = New CompressionProfile With {
            .Id = SelectedProfile.Id,
            .Name = SelectedProfile.Name,
            .Description = SelectedProfile.Description,
            .CompressionMode = SelectedProfile.CompressionMode,
            .SkipNonCompressable = SelectedProfile.SkipNonCompressable,
            .SkipUserNonCompressable = SelectedProfile.SkipUserNonCompressable,
            .WatchFolderForChanges = SelectedProfile.WatchFolderForChanges,
            .CustomSkipList = New List(Of String)(SelectedProfile.CustomSkipList),
            .MaxCompressionThreads = SelectedProfile.MaxCompressionThreads,
            .IsDefault = SelectedProfile.IsDefault,
            .CreatedAt = SelectedProfile.CreatedAt,
            .LastModifiedAt = DateTime.Now
        }
        
        IsEditing = True
        OnPropertyChanged(NameOf(EditingProfile))
    End Sub
    
    Public ReadOnly Property CloneProfileCommand As IRelayCommand = New RelayCommand(AddressOf CloneProfile, AddressOf CanEditProfile)
    
    Private Sub CloneProfile()
        EditingProfile = SelectedProfile.Clone()
        
        IsEditing = True
        OnPropertyChanged(NameOf(EditingProfile))
    End Sub
    
    Public ReadOnly Property DeleteProfileCommand As IRelayCommand = New RelayCommand(AddressOf DeleteProfile, AddressOf CanDeleteProfile)
    
    Private Function CanDeleteProfile() As Boolean
        Return SelectedProfile IsNot Nothing AndAlso Profiles.Count > 1
    End Function
    
    Private Async Sub DeleteProfile()
        Dim confirmed = Await Application.GetService(Of IWindowService)().ShowMessageBox("Delete Profile", $"Are you sure you want to delete the profile '{SelectedProfile.Name}'?")
        
        If confirmed Then
            _profileService.RemoveProfile(SelectedProfile)
            LoadProfiles()
        End If
    End Sub
    
    Public ReadOnly Property SaveProfileCommand As IRelayCommand = New RelayCommand(AddressOf SaveProfile)
    
    Private Sub SaveProfile()
        EditingProfile.LastModifiedAt = DateTime.Now
        
        If Profiles.Any(Function(p) p.Id = EditingProfile.Id) Then
            _profileService.UpdateProfile(EditingProfile)
        Else
            _profileService.AddProfile(EditingProfile)
        End If
        
        IsEditing = False
        LoadProfiles()
    End Sub
    
    Public ReadOnly Property CancelEditCommand As IRelayCommand = New RelayCommand(AddressOf CancelEdit)
    
    Private Sub CancelEdit()
        IsEditing = False
    End Sub
End Class