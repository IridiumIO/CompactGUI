

Imports Gameloop.Vdf
Imports Gameloop.Vdf.Linq

Imports Microsoft.Extensions.Caching.Memory

Public Class SteamACFParser


    Private Shared ReadOnly SteamLibraryCache As New MemoryCache(New MemoryCacheOptions())

    Public Shared Function GetSteamNameAndIDFromFolder(SteamFolder As IO.DirectoryInfo) As SteamACFResult?
        Dim steamAppsFolder = SteamFolder.Parent.Parent

        Dim cachedResult = TryGetCachedGame(steamAppsFolder, SteamFolder)
        If cachedResult IsNot Nothing Then
            Return If(Not cachedResult.Equals(SteamACFResult.NoResult), cachedResult, Nothing)
        End If

        Dim allGames = LookupAllSteamGames(steamAppsFolder)
        CacheLibrary(steamAppsFolder, allGames)

        If allGames.ContainsKey(SteamFolder.Name) Then Return allGames(SteamFolder.Name)
        Return Nothing

    End Function

    Private Shared Sub CacheLibrary(steamAppsFolder As IO.DirectoryInfo, allGames As Dictionary(Of String, SteamACFResult?))
        Dim policy As New MemoryCacheEntryOptions With {
            .AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(2)
        }

        SteamLibraryCache.Set(steamAppsFolder.FullName, allGames, policy)
    End Sub

    Private Shared Function TryGetCachedGame(steamAppsFolder As IO.DirectoryInfo, SteamFolder As IO.DirectoryInfo) As SteamACFResult?

        Dim libraryCache = GetCachedLibrary(steamAppsFolder)
        If libraryCache Is Nothing Then Return Nothing

        If libraryCache.ContainsKey(SteamFolder.Name) Then Return libraryCache(SteamFolder.Name)
        Return SteamACFResult.NoResult

    End Function


    Private Shared Function GetCachedLibrary(steamAppsFolder As IO.DirectoryInfo) As Dictionary(Of String, SteamACFResult?)
        Return TryCast(SteamLibraryCache.Get(steamAppsFolder.FullName), Dictionary(Of String, SteamACFResult?))
    End Function

    Private Shared Function LookupAllSteamGames(steamAppsFolder As IO.DirectoryInfo) As Dictionary(Of String, SteamACFResult?)
        Dim allGames As New Dictionary(Of String, SteamACFResult?)

        For Each fl In steamAppsFolder.EnumerateFiles("*.acf").Where(Function(f) f.Length > 0)
            Try
                Dim ACFFile = VdfConvert.Deserialize(IO.File.ReadAllText(fl.FullName))
                If ACFFile IsNot Nothing Then
                    Dim game = ParseACFFile(ACFFile)
                    allGames(game.InstallDirectory) = game
                End If
            Catch
                Debug.WriteLine($"ACF file unsupported: {fl.FullName}")
            End Try
        Next

        Return allGames
    End Function


    Private Shared Function ParseACFFile(ACFFile As VProperty) As SteamACFResult
        Dim appID = CInt(ACFFile.Value.Item("appid").ToString)
        Dim sName = ACFFile.Value.Item("name").ToString
        Dim sInstallDir = ACFFile.Value.Item("installdir").ToString
        Return New SteamACFResult With {.AppID = appID, .GameName = sName, .InstallDirectory = sInstallDir}
    End Function


End Class

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