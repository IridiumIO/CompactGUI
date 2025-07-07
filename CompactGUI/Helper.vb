Imports System.IO
Imports System.Management
Imports System.Runtime.InteropServices
Imports System.Text

Imports CommunityToolkit.Mvvm.Input

Imports CompactGUI.Core.SharedMethods

Imports Gameloop.Vdf


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
            Dim validation = Core.SharedMethods.VerifyFolder(folder)

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

'https://stackoverflow.com/questions/4897655/create-a-shortcut-on-desktop
Module ShortcutCreator

    <ComImport>
    <Guid("00021401-0000-0000-C000-000000000046")>
    Friend Class ShellLink
    End Class

    <ComImport>
    <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    <Guid("000214F9-0000-0000-C000-000000000046")>
    Friend Interface IShellLink
        Sub GetPath(<Out, MarshalAs(UnmanagedType.LPWStr)> pszFile As StringBuilder, cchMaxPath As Integer, ByRef pfd As IntPtr, fFlags As Integer)
        Sub GetIDList(ByRef ppidl As IntPtr)
        Sub SetIDList(pidl As IntPtr)
        Sub GetDescription(<Out, MarshalAs(UnmanagedType.LPWStr)> pszName As StringBuilder, cchMaxName As Integer)
        Sub SetDescription(<MarshalAs(UnmanagedType.LPWStr)> pszName As String)
        Sub GetWorkingDirectory(<Out, MarshalAs(UnmanagedType.LPWStr)> pszDir As StringBuilder, cchMaxPath As Integer)
        Sub SetWorkingDirectory(<MarshalAs(UnmanagedType.LPWStr)> pszDir As String)
        Sub GetArguments(<Out, MarshalAs(UnmanagedType.LPWStr)> pszArgs As StringBuilder, cchMaxPath As Integer)
        Sub SetArguments(<MarshalAs(UnmanagedType.LPWStr)> pszArgs As String)
        Sub GetHotkey(ByRef pwHotkey As Short)
        Sub SetHotkey(wHotkey As Short)
        Sub GetShowCmd(ByRef piShowCmd As Integer)
        Sub SetShowCmd(iShowCmd As Integer)
        Sub GetIconLocation(<Out, MarshalAs(UnmanagedType.LPWStr)> pszIconPath As StringBuilder, cchIconPath As Integer, ByRef piIcon As Integer)
        Sub SetIconLocation(<MarshalAs(UnmanagedType.LPWStr)> pszIconPath As String, iIcon As Integer)
        Sub SetRelativePath(<MarshalAs(UnmanagedType.LPWStr)> pszPathRel As String, dwReserved As Integer)
        Sub Resolve(hwnd As IntPtr, fFlags As Integer)
        Sub SetPath(<MarshalAs(UnmanagedType.LPWStr)> pszFile As String)
    End Interface

    <ComImport>
    <Guid("0000010B-0000-0000-C000-000000000046")>
    <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    Friend Interface IPersistFile
        Sub GetClassID(ByRef pClassID As Guid)
        Sub IsDirty()
        Sub Load(<MarshalAs(UnmanagedType.LPWStr)> pszFileName As String, dwMode As UInteger)
        Sub Save(<MarshalAs(UnmanagedType.LPWStr)> pszFileName As String, fRemember As Boolean)
        Sub SaveCompleted(<MarshalAs(UnmanagedType.LPWStr)> pszFileName As String)
        Sub GetCurFile(<MarshalAs(UnmanagedType.LPWStr)> ByRef ppszFileName As String)
    End Interface

    Public Sub CreateShortcut(shortcutPath As String, targetPath As String, Optional description As String = "", Optional workingDirectory As String = "", Optional iconPath As String = "")
        Dim link As IShellLink = CType(New ShellLink(), IShellLink)

        link.SetDescription(description)
        link.SetPath(targetPath)
        link.SetWorkingDirectory(If(String.IsNullOrWhiteSpace(workingDirectory), IO.Path.GetDirectoryName(targetPath), workingDirectory))

        If Not String.IsNullOrWhiteSpace(iconPath) Then
            link.SetIconLocation(iconPath, 0)
        End If

        Dim file As IPersistFile = CType(link, IPersistFile)
        file.Save(shortcutPath, True)
    End Sub

End Module