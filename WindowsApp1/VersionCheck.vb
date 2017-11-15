Imports System.Net

Public Class VersionCheck
    Shared Sub VC(version As String)

        Dim wc As New WebClient With {.Encoding = Text.Encoding.UTF8}

        Try
            Dim versionDoc As XDocument = XDocument.Load("https://raw.githubusercontent.com/ImminentFate/CompactGUI/master/Version.xml")
            If versionDoc.ToString <> Nothing Then XMLParse(versionDoc, version)
        Catch ex As WebException
        End Try

    End Sub

    Shared Sub XMLParse(versionDoc As XDocument, version As String)
        Dim info As XElement = versionDoc.Root

        Dim xml_MajorVer As Single = info.Element("VersionMajor").Value
        Dim xml_MinorVer As Integer = info.Element("VersionMinor").Value
        Dim xml_IsPrerelease As Boolean = info.Element("IsPrerelease").Value
        Dim xml_Changes() = info.Element("Changes").Value.Split("|")
        Dim xml_Fixes() = info.Element("Fixes").Value.Split("|")
        Console.WriteLine(xml_MajorVer)

        Dim exe_MajorVer As Single = CSng(version.Substring(0, version.LastIndexOf(".")))
        Dim exe_MinorVer As Integer = CInt(version.Substring(version.LastIndexOf(".") + 1))


        If xml_MajorVer > exe_MajorVer Then
            MsgBox("New Major")
        ElseIf xml_MajorVer = exe_MajorVer Then
            If xml_MinorVer > exe_MinorVer Then
                MsgBox("New Minor")
            End If
        End If


    End Sub
End Class
