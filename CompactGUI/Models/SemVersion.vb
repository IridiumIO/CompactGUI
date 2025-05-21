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
        If other.Major - Me.Major <> 0 Then Return other.Major - Me.Major
        If other.Minor - Me.Minor <> 0 Then Return other.Minor - Me.Minor
        If other.Patch - Me.Patch <> 0 Then Return other.Patch - Me.Patch
        If Not String.Equals(Me.PreRelease, other.PreRelease) Then
            If Me.PreRelease = "" Then Return -1
            If other.PreRelease = "" Then Return 1
            Return String.Compare(other.PreRelease, Me.PreRelease)
        End If
        If other.PreReleaseMinor - Me.PreReleaseMinor <> 0 Then Return other.PreReleaseMinor - Me.PreReleaseMinor
        Return 0

    End Function

    Public Shared Operator <(ByVal lhs As SemVersion, ByVal rhs As SemVersion) As Boolean
        Dim comparer = lhs.CompareTo(rhs)
        If comparer <= 0 Then Return False
        Return True
    End Operator

    Public Shared Operator >(ByVal lhs As SemVersion, ByVal rhs As SemVersion) As Boolean
        Dim comparer = lhs.CompareTo(rhs)
        If comparer >= 0 Then Return False
        Return True
    End Operator

    Public Overrides Function ToString() As String
        Return $"{Major}.{Minor}.{Patch}-{PreRelease}.{PreReleaseMinor}"
    End Function

    Public Function IsPreRelease() As Boolean
        If PreRelease = "" Then Return True
        Return False
    End Function

    Public Function Friendly() As String
        Return If(PreRelease = "" OrElse PreRelease = Nothing OrElse PreRelease = "r",
            $"{Major}.{Minor}.{Patch}",
            $"{Major}.{Minor}.{Patch} - {PreRelease} {PreReleaseMinor}")

    End Function

End Class
