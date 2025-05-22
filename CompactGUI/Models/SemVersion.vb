Public Class SemVersion : Implements IComparable(Of SemVersion)
    Property Major As Integer
    Property Minor As Integer
    Property Patch As Integer
    Property PreRelease As String
    Property PreReleaseMinor As Integer

    Sub New()
    End Sub

    Sub New(major As Integer, minor As Integer, patch As Integer)
        Me.Major = major
        Me.Minor = minor
        Me.Patch = patch
    End Sub

    Sub New(major As Integer, minor As Integer, patch As Integer, prerelease As String, prereleaseminor As Integer)
        Me.Major = major
        Me.Minor = minor
        Me.Patch = patch
        Me.PreRelease = prerelease.ToLower
        Me.PreReleaseMinor = prereleaseminor
    End Sub

    Public Function CompareTo(other As SemVersion) As Integer Implements IComparable(Of SemVersion).CompareTo
        If other.Major - Major <> 0 Then Return other.Major - Major
        If other.Minor - Minor <> 0 Then Return other.Minor - Minor
        If other.Patch - Patch <> 0 Then Return other.Patch - Patch
        If Not String.Equals(PreRelease, other.PreRelease) Then
            If PreRelease = "" Then Return -1
            If other.PreRelease = "" Then Return 1
            Return String.Compare(other.PreRelease, PreRelease)
        End If
        If other.PreReleaseMinor - PreReleaseMinor <> 0 Then Return other.PreReleaseMinor - PreReleaseMinor
        Return 0

    End Function

    Public Shared Operator <(lhs As SemVersion, rhs As SemVersion) As Boolean
        Dim comparer = lhs.CompareTo(rhs)
        If comparer <= 0 Then Return False
        Return True
    End Operator

    Public Shared Operator >(lhs As SemVersion, rhs As SemVersion) As Boolean
        Dim comparer = lhs.CompareTo(rhs)
        If comparer >= 0 Then Return False
        Return True
    End Operator

    Public Overrides Function ToString() As String
        Return $"{Major}.{Minor}.{Patch}-{PreRelease}.{PreReleaseMinor}"
    End Function

    Public Function IsPreRelease() As Boolean
        If PreRelease = "" OrElse PreRelease = Nothing OrElse PreRelease = "r" Then Return False
        Return True
    End Function

    Public Function Friendly() As String
        Return If(PreRelease = "" OrElse PreRelease = Nothing OrElse PreRelease = "r",
            $"{Major}.{Minor}.{Patch}",
            $"{Major}.{Minor}.{Patch} {PreRelease} {PreReleaseMinor}")

    End Function

End Class
