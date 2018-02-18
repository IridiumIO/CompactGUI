Imports System.Globalization
Imports System.Net

Class VersionCheck
    Shared Sub VC(version As String)

        Try
            Dim versionDoc As XDocument = XDocument.Load("https://raw.githubusercontent.com/ImminentFate/CompactGUI/master/Version.xml")
            If versionDoc.ToString <> Nothing Then
                If XMLParse(versionDoc, version) = True Then
                    Compact.updateBanner.Visible = True
                    Compact.dlUpdateLink.Text = "Update Available: Click to download " & xml_VersionStr
                    Info.lbl_CheckUpdates.Text = "Update to " & xml_VersionStr
                    Info.lbl_CheckUpdates.Visible = True
                End If
            End If
        Catch ex As WebException
        End Try

    End Sub

    Shared xml_MajorVer As Single
    Shared xml_MinorVer As Integer
    Shared xml_VersionStr As String
    Shared xml_ChocoVStr As String
    Shared xml_IsPrerelease As Boolean
    Shared xml_Changes()
    Shared xml_Fixes()

    Shared Function XMLParse(versionDoc As XDocument, version As String)
        Dim info As XElement = versionDoc.Root

        xml_MajorVer = Single.Parse(info.Element("VersionMajor").Value, CultureInfo.InvariantCulture)
        xml_MinorVer = info.Element("VersionMinor").Value
        xml_VersionStr = info.Element("VersionStr").Value
        xml_ChocoVStr = info.Element("ChocolateyVStr").Value
        xml_IsPrerelease = info.Element("IsPrerelease").Value
        xml_Changes = info.Element("Changes").Value.Split("|")
        xml_Fixes = info.Element("Fixes").Value.Split("|")

        Dim exe_MajorVer As Single = Single.Parse(version.Substring(0, version.LastIndexOf(".")), CultureInfo.InvariantCulture)
        Dim exe_MinorVer As Integer = CInt(version.Substring(version.LastIndexOf(".") + 1))

        If xml_MajorVer > exe_MajorVer Then
            Return True
        ElseIf xml_MajorVer = exe_MajorVer Then
            If xml_MinorVer > exe_MinorVer Then
                Return True
            End If
        End If

    End Function

End Class
