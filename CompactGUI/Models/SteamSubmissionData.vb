
' Object used to build submission data to send online after compression
Public Class SteamSubmissionData
    Public Property UID As String
    Public Property SteamID As Integer
    Public Property GameName As String
    Public Property FolderName As String
    Public Property CompressionMode As Integer
    Public Property BeforeBytes As Long
    Public Property AfterBytes As Long
    Public Property PoorlyCompressedExt As List(Of Core.ExtensionResult)

End Class

