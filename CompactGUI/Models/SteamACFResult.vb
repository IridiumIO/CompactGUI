Public Structure SteamACFResult
    Public AppID As Integer
    Public GameName As String
    Public InstallDirectory As String

    ' Special placeholder for "no result"
    Public Shared ReadOnly NoResult As New SteamACFResult With {
        .AppID = -1,
        .GameName = String.Empty,
        .InstallDirectory = String.Empty
    }

End Structure