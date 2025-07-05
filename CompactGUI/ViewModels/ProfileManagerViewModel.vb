Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Public Class ProfileManagerViewModel : Inherits ObservableObject
    
    Private ReadOnly _profileManager As CompressionProfileManager
    Private ReadOnly _snackbarService As CustomSnackBarService
    
    Public Property Profiles As ObservableCollection(Of CompressionProfile)
        Get
            Return _profileManager.Profiles
        End Get
    End Property
    
    Private _selectedProfile As CompressionProfile
    Public Property SelectedProfile As CompressionProfile
        Get
            Return _selectedProfile
        End Get
        Set(value As CompressionProfile)
            SetProperty(_selectedProfile, value)
            EditProfileCommand.NotifyCanExecuteChanged()
            DeleteProfileCommand.NotifyCanExecuteChanged()
        End Set
    End Property
    
    Private _isEditingProfile As Boolean
    Public Property IsEditingProfile As Boolean
        Get
            Return _isEditingProfile
        End Get
        Set(value As Boolean)
            SetProperty(_isEditingProfile, value)
        End Set
    End Property
    
    Private _editingProfile As CompressionProfile
    Public Property EditingProfile As CompressionProfile
        Get
            Return _editingProfile
        End Get
        Set(value As CompressionProfile)
            SetProperty(_editingProfile, value)
        End Set
    End Property
    
    Public ReadOnly Property CreateProfileCommand As IRelayCommand = New RelayCommand(Sub() CreateNewProfile())
    Public ReadOnly Property EditProfileCommand As IRelayCommand = New RelayCommand(Sub() EditProfile(), Function() CanEditProfile())
    Public ReadOnly Property DeleteProfileCommand As IRelayCommand = New RelayCommand(Sub() DeleteProfile(), Function() CanDeleteProfile())
    Public ReadOnly Property SaveProfileCommand As IRelayCommand = New RelayCommand(Sub() SaveProfile())
    Public ReadOnly Property CancelEditCommand As IRelayCommand = New RelayCommand(Sub() CancelEdit())
    Public ReadOnly Property DuplicateProfileCommand As IRelayCommand = New RelayCommand(Sub() DuplicateProfile(), Function() CanEditProfile())
    
    Public Sub New(profileManager As CompressionProfileManager, snackbarService As CustomSnackBarService)
        _profileManager = profileManager
        _snackbarService = snackbarService
    End Sub
    
    Private Sub CreateNewProfile()
        EditingProfile = New CompressionProfile With {
            .Name = "New Profile",
            .Description = "Custom compression profile",
            .CompressionMode = Core.CompressionMode.XPRESS8K,
            .SkipPoorlyCompressedFileTypes = True,
            .MaxThreads = Environment.ProcessorCount,
            .MinimumFileSize = 1024
        }
        IsEditingProfile = True
    End Sub
    
    Private Sub EditProfile()
        If SelectedProfile IsNot Nothing Then
            EditingProfile = SelectedProfile.Clone()
            IsEditingProfile = True
        End If
    End Sub
    
    Private Sub DeleteProfile()
        If SelectedProfile IsNot Nothing Then
            _profileManager.RemoveProfile(SelectedProfile)
            _snackbarService.Show("Profile Deleted", $"Profile '{SelectedProfile.Name}' has been deleted", Wpf.Ui.Controls.ControlAppearance.Info)
            SelectedProfile = Nothing
        End If
    End Sub
    
    Private Sub SaveProfile()
        If EditingProfile IsNot Nothing Then
            If String.IsNullOrWhiteSpace(EditingProfile.Name) Then
                _snackbarService.Show("Invalid Name", "Profile name cannot be empty", Wpf.Ui.Controls.ControlAppearance.Danger)
                Return
            End If
            
            ' Check for duplicate names
            If Profiles.Any(Function(p) p.Name = EditingProfile.Name AndAlso p IsNot SelectedProfile) Then
                _snackbarService.Show("Duplicate Name", "A profile with this name already exists", Wpf.Ui.Controls.ControlAppearance.Danger)
                Return
            End If
            
            If SelectedProfile Is Nothing Then
                ' Creating new profile
                _profileManager.AddProfile(EditingProfile)
                SelectedProfile = EditingProfile
                _snackbarService.Show("Profile Created", $"Profile '{EditingProfile.Name}' has been created", Wpf.Ui.Controls.ControlAppearance.Success)
            Else
                ' Updating existing profile
                SelectedProfile.Name = EditingProfile.Name
                SelectedProfile.Description = EditingProfile.Description
                SelectedProfile.CompressionMode = EditingProfile.CompressionMode
                SelectedProfile.SkipPoorlyCompressedFileTypes = EditingProfile.SkipPoorlyCompressedFileTypes
                SelectedProfile.SkipUserSubmittedFiletypes = EditingProfile.SkipUserSubmittedFiletypes
                SelectedProfile.WatchFolderForChanges = EditingProfile.WatchFolderForChanges
                SelectedProfile.MaxThreads = EditingProfile.MaxThreads
                SelectedProfile.CustomExclusionList = EditingProfile.CustomExclusionList
                SelectedProfile.TargetFileTypes = EditingProfile.TargetFileTypes
                SelectedProfile.MinimumFileSize = EditingProfile.MinimumFileSize
                
                _profileManager.UpdateProfile(SelectedProfile)
                _snackbarService.Show("Profile Updated", $"Profile '{SelectedProfile.Name}' has been updated", Wpf.Ui.Controls.ControlAppearance.Success)
            End If
            
            IsEditingProfile = False
            EditingProfile = Nothing
        End If
    End Sub
    
    Private Sub CancelEdit()
        IsEditingProfile = False
        EditingProfile = Nothing
    End Sub
    
    Private Sub DuplicateProfile()
        If SelectedProfile IsNot Nothing Then
            Dim duplicated = SelectedProfile.Clone()
            duplicated.Name = $"{SelectedProfile.Name} (Copy)"
            duplicated.IsDefault = False
            _profileManager.AddProfile(duplicated)
            SelectedProfile = duplicated
            _snackbarService.Show("Profile Duplicated", $"Profile duplicated as '{duplicated.Name}'", Wpf.Ui.Controls.ControlAppearance.Success)
        End If
    End Sub
    
    Private Function CanEditProfile() As Boolean
        Return SelectedProfile IsNot Nothing
    End Function
    
    Private Function CanDeleteProfile() As Boolean
        Return SelectedProfile IsNot Nothing AndAlso Not SelectedProfile.IsDefault
    End Function
    
End Class