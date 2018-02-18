Imports System.Text
Imports System.Text.RegularExpressions

Partial Class Compact

    Dim CP As Encoding

    Private Sub CreateProcess(TargetMode As String)
        If CP Is Nothing Then CP = getEncoding()
        RunCompact(WorkingList.Item(0))
        outputbuffer.Add("Compressing: " & vbTab & WorkingList.item(0))
        ActionBegun(TargetMode)
    End Sub




    Dim compactArgs As String




    Private Sub RunCompact(desiredFile As String)

        If CurrentMode = "compact" Then

            compactArgs = "/C /I"

            'If checkRecursiveScan.Checked = True Then compactArgs &= " /S"
            If checkForceCompression.Checked = True Then compactArgs &= " /F"
            If checkHiddenFiles.Checked = True Then compactArgs &= " /A"
            If compressX4.Checked = True Then compactArgs &= " /EXE:XPRESS4K"
            If compressX8.Checked = True Then compactArgs &= " /EXE:XPRESS8K"
            If compressX16.Checked = True Then compactArgs &= " /EXE:XPRESS16K"
            If compressLZX.Checked = True Then compactArgs &= " /EXE:LZX"

            RunCompact_ProcessGen(compactArgs, desiredFile)

        ElseIf CurrentMode = "uncompact" Then

            outputbuffer.Add("Uncompressing: " & vbTab & desiredFile)
            compactArgs = "/U /EXE /I"

            If checkForceCompression.Checked = True Then compactArgs &= " /F"
            If checkHiddenFiles.Checked = True Then compactArgs &= " /A"

            RunCompact_ProcessGen(compactArgs, desiredFile)

        End If

    End Sub




    Private Sub RunCompact_ProcessGen(passthroughArgs As String, fileTarget As String)
        MyProcess = New Process
        With MyProcess.StartInfo
            .FileName = "compact.exe"
            .WorkingDirectory = workingDir
            .Arguments = passthroughArgs & " " & """" & fileTarget & """"
            .StandardOutputEncoding = CP
            .StandardErrorEncoding = CP
            .UseShellExecute = False
            .CreateNoWindow = True
            .RedirectStandardInput = True
            .RedirectStandardOutput = True
            .RedirectStandardError = True
        End With

        MyProcess.Start()
        MyProcess.PriorityClass = ProcessPriorityClass.BelowNormal
        MyProcess.EnableRaisingEvents = True
        MyProcess.BeginErrorReadLine()
        MyProcess.BeginOutputReadLine()
    End Sub




    Private Function getEncoding() As Encoding
        Dim CPGet = New Process
        With CPGet.StartInfo
            .FileName = "cmd.exe"
            .Arguments = "/c chcp"
            .UseShellExecute = False
            .CreateNoWindow = True
            .StandardOutputEncoding = Encoding.Default
            .StandardErrorEncoding = Encoding.Default
            .WorkingDirectory = workingDir
            .RedirectStandardInput = True
            .RedirectStandardOutput = True
            .RedirectStandardError = True
        End With
        CPGet.Start()

        Dim Res = CPGet.StandardOutput.ReadLine()
        Dim CPa = Integer.Parse(Regex.Replace(Res, "[^\d]", ""))
        CPGet.StandardInput.WriteLine("exit")
        CPGet.StandardInput.Flush()
        CPGet.WaitForExit()
        Return Encoding.GetEncoding(CPa)
    End Function




End Class