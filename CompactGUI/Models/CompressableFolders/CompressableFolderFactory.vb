Public Class CompressableFolderFactory
    Public Shared Function CreateCompressableFolder(path As String) As CompressableFolder
        Dim folderInfo = New IO.DirectoryInfo(path)

        If IsSteamFolder(folderInfo) Then
            Return If(CreateSteamFolder(folderInfo), New StandardFolder(path))
        Else
            Return New StandardFolder(path)
        End If

    End Function


    Private Shared Function IsSteamFolder(folderPath As IO.DirectoryInfo) As Boolean
        Return folderPath.Parent?.Parent?.Name.ToLowerInvariant = "steamapps"
    End Function


    Private Shared Function CreateSteamFolder(folderInfo As IO.DirectoryInfo) As CompressableFolder

        Dim SteamFolderData? = SteamACFParser.GetSteamNameAndIDFromFolder(folderInfo)

        If SteamFolderData Is Nothing Then Return Nothing

        Dim steamFolder As New SteamFolder(folderInfo.FullName, If(SteamFolderData?.GameName, folderInfo.FullName), SteamFolderData?.AppID)

        Return steamFolder
    End Function




End Class

