Imports System.Collections.ObjectModel
Imports CommunityToolkit.Mvvm.ComponentModel
Imports System.Text.RegularExpressions

Public Class CompressionRuleEngine : Inherits ObservableObject
    
    Public Property Rules As New ObservableCollection(Of CompressionRule)
    
    Public Sub New()
        LoadDefaultRules()
    End Sub
    
    Private Sub LoadDefaultRules()
        ' Default rules for common scenarios
        Rules.Add(New CompressionRule With {
            .Name = "Skip System Files",
            .Description = "Skip Windows system files and executables",
            .RuleType = CompressionRuleType.FileExtension,
            .Pattern = "exe|dll|sys|msi|cab",
            .Action = CompressionAction.Skip,
            .IsEnabled = True,
            .Priority = 1
        })
        
        Rules.Add(New CompressionRule With {
            .Name = "Compress Large Media",
            .Description = "Use maximum compression for large media files",
            .RuleType = CompressionRuleType.FileSize,
            .Pattern = ">100MB",
            .Action = CompressionAction.UseProfile,
            .ProfileName = "Maximum Compression",
            .IsEnabled = False,
            .Priority = 2
        })
        
        Rules.Add(New CompressionRule With {
            .Name = "Skip Already Compressed",
            .Description = "Skip files that are already compressed",
            .RuleType = CompressionRuleType.FileExtension,
            .Pattern = "zip|rar|7z|gz|bz2|xz",
            .Action = CompressionAction.Skip,
            .IsEnabled = True,
            .Priority = 3
        })
    End Sub
    
    Public Function EvaluateFile(filePath As String, fileInfo As IO.FileInfo) As CompressionRuleResult
        Dim applicableRules = Rules.Where(Function(r) r.IsEnabled).OrderBy(Function(r) r.Priority)
        
        For Each rule In applicableRules
            If rule.Matches(filePath, fileInfo) Then
                Return New CompressionRuleResult With {
                    .Rule = rule,
                    .Action = rule.Action,
                    .ProfileName = rule.ProfileName,
                    .CustomSettings = rule.CustomSettings
                }
            End If
        Next
        
        ' Default action if no rules match
        Return New CompressionRuleResult With {
            .Action = CompressionAction.Compress
        }
    End Function
    
End Class

Public Class CompressionRule : Inherits ObservableObject
    
    Public Property Name As String
    Public Property Description As String
    Public Property RuleType As CompressionRuleType
    Public Property Pattern As String
    Public Property Action As CompressionAction
    Public Property ProfileName As String = ""
    Public Property CustomSettings As New Dictionary(Of String, Object)
    Public Property IsEnabled As Boolean = True
    Public Property Priority As Integer = 0
    
    Public Function Matches(filePath As String, fileInfo As IO.FileInfo) As Boolean
        Select Case RuleType
            Case CompressionRuleType.FileExtension
                Dim extension = Path.GetExtension(filePath).TrimStart("."c).ToLower()
                Dim patterns = Pattern.ToLower().Split("|"c)
                Return patterns.Contains(extension)
                
            Case CompressionRuleType.FileName
                Return Regex.IsMatch(Path.GetFileName(filePath), Pattern, RegexOptions.IgnoreCase)
                
            Case CompressionRuleType.FilePath
                Return Regex.IsMatch(filePath, Pattern, RegexOptions.IgnoreCase)
                
            Case CompressionRuleType.FileSize
                Return EvaluateSizePattern(fileInfo.Length, Pattern)
                
            Case CompressionRuleType.FileAge
                Return EvaluateAgePattern(fileInfo.LastWriteTime, Pattern)
                
            Case Else
                Return False
        End Select
    End Function
    
    Private Function EvaluateSizePattern(fileSize As Long, pattern As String) As Boolean
        ' Pattern examples: ">100MB", "<1GB", "=500KB"
        Dim match = Regex.Match(pattern, "([<>=])(\d+)(KB|MB|GB)", RegexOptions.IgnoreCase)
        If Not match.Success Then Return False
        
        Dim operator = match.Groups(1).Value
        Dim value = Long.Parse(match.Groups(2).Value)
        Dim unit = match.Groups(3).Value.ToUpper()
        
        ' Convert to bytes
        Dim bytes As Long = value
        Select Case unit
            Case "KB" : bytes *= 1024
            Case "MB" : bytes *= 1024 * 1024
            Case "GB" : bytes *= 1024 * 1024 * 1024
        End Select
        
        Select Case operator
            Case ">" : Return fileSize > bytes
            Case "<" : Return fileSize < bytes
            Case "=" : Return fileSize = bytes
            Case Else : Return False
        End Select
    End Function
    
    Private Function EvaluateAgePattern(lastWrite As DateTime, pattern As String) As Boolean
        ' Pattern examples: ">30d", "<1y", "=7d"
        Dim match = Regex.Match(pattern, "([<>=])(\d+)(d|m|y)", RegexOptions.IgnoreCase)
        If Not match.Success Then Return False
        
        Dim operator = match.Groups(1).Value
        Dim value = Integer.Parse(match.Groups(2).Value)
        Dim unit = match.Groups(3).Value.ToLower()
        
        Dim targetDate As DateTime
        Select Case unit
            Case "d" : targetDate = DateTime.Now.AddDays(-value)
            Case "m" : targetDate = DateTime.Now.AddMonths(-value)
            Case "y" : targetDate = DateTime.Now.AddYears(-value)
            Case Else : Return False
        End Select
        
        Select Case operator
            Case ">" : Return lastWrite < targetDate ' Older than
            Case "<" : Return lastWrite > targetDate ' Newer than
            Case "=" : Return Math.Abs((lastWrite - targetDate).TotalDays) < 1
            Case Else : Return False
        End Select
    End Function
    
End Class

Public Class CompressionRuleResult
    Public Property Rule As CompressionRule
    Public Property Action As CompressionAction
    Public Property ProfileName As String = ""
    Public Property CustomSettings As Dictionary(Of String, Object)
End Class

Public Enum CompressionRuleType
    FileExtension
    FileName
    FilePath
    FileSize
    FileAge
End Enum

Public Enum CompressionAction
    Compress
    Skip
    UseProfile
    CustomSettings
End Enum