Imports System.IO
Imports System.Management
Imports System.Text

Imports CommunityToolkit.Mvvm.Input

Imports CompactGUI.Core.SharedMethods

Imports Gameloop.Vdf

Imports IWshRuntimeLibrary

Imports Wpf.Ui.Controls

Module Helper
    Function GetSteamNameAndIDFromFolder(path As String) As (appID As Integer, gameName As String, installDir As String)

        Dim workingDir = New DirectoryInfo(path)
        Dim parentfolder = workingDir.Parent.Parent

        If Not parentfolder?.Name = "steamapps" Then Return Nothing

        For Each fl In parentfolder.EnumerateFiles("*.acf").Where(Function(f) f.Length > 0)
            Try
                Dim vConv = VdfConvert.Deserialize(IO.File.ReadAllText(fl.FullName))
                If vConv.Value.Item("installdir").ToString = workingDir.Name Then
                    Dim appID = CInt(vConv.Value.Item("appid").ToString)
                    Dim sName = vConv.Value.Item("name").ToString
                    Dim sInstallDir = vConv.Value.Item("installdir").ToString
                    Return (appID, sName, sInstallDir)
                    'TODO: Maybe add check to see when game was last updated?
                End If
            Catch
                Debug.WriteLine($"VDF file unsupported: {fl.FullName}")
            End Try
        Next

        Return Nothing
    End Function


    Function getUID() As String
        Dim MacID As String = String.Empty
        Dim mc As ManagementClass = New ManagementClass("Win32_NetworkAdapterConfiguration")
        Dim moc As ManagementObjectCollection = mc.GetInstances()
        For Each mo As ManagementObject In moc

            If mo.Properties("IPEnabled") IsNot Nothing AndAlso mo.Properties("IPEnabled").Value IsNot Nothing Then

                If CBool(mo.Properties("IPEnabled").Value) = True AndAlso mo.Properties("MacAddress") IsNot Nothing AndAlso mo.Properties("MacAddress").Value IsNot Nothing Then
                    MacID = mo.Properties("MacAddress").Value.ToString()
                    Exit For
                End If
            End If
        Next
        Return Convert.ToBase64String(Encoding.UTF8.GetBytes(MacID))
    End Function


    Function LoadImageFromDisk(imagePath As String) As BitmapImage
        Dim bImg As New BitmapImage(New Uri(imagePath))
        Return bImg
    End Function

    Function LoadImageFromMemoryStream(imageData As Byte()) As BitmapImage
        Dim bImg As New BitmapImage()
        Using ms As New MemoryStream(imageData)
            bImg.BeginInit()
            bImg.CacheOption = BitmapCacheOption.OnLoad
            bImg.StreamSource = ms
            bImg.EndInit()
        End Using
        Return bImg
    End Function


    Public Function GetInvalidFolders(folderPaths() As String) As (InvalidFolders As List(Of String), InvalidMessages As List(Of FolderVerificationResult))

        Dim invalidFolders As New List(Of String)
        Dim invalidMessages As New List(Of FolderVerificationResult)

        For Each folder In folderPaths
            Dim validation = Core.verifyFolder(folder)

            If validation <> FolderVerificationResult.Valid Then
                invalidFolders.Add(folder)
                invalidMessages.Add(validation)
            End If
        Next


        Return (invalidFolders, invalidMessages)
    End Function



    Public Sub RunAsAdmin(FolderName As String)
        Dim myproc As New Process With {
            .StartInfo = New ProcessStartInfo With {
                .FileName = Environment.ProcessPath,
                .UseShellExecute = True,
                .Arguments = $"""{FolderName}""",
                .Verb = "runas"}
        }
        Dim app As Application = Application.Current

        app.ShutdownPipeServer().ContinueWith(
            Sub()
                app.Dispatcher.Invoke(
                    Sub()
                        Application.mutex.ReleaseMutex()
                        Application.mutex.Dispose()
                    End Sub
                )
                myproc.Start()
                app.Dispatcher.Invoke(Sub() app.Shutdown())
            End Sub
        )
    End Sub


End Module
