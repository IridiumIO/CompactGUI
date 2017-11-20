Imports System.Text

Partial Class Compact
    Dim CP As Encoding
    Private Sub CreateProcess(passthrougharg As String)

        Try
            If passthrougharg.Contains("compact") Then isQueryMode = 0      'also catches "uncompact"
            If passthrougharg = "query" Then isQueryMode = 1
            progresspercent.Visible = True

            If CP Is Nothing Then CP = getEncoding()

            RunCompact(passthrougharg)

            If passthrougharg = "compact" Then sb_progresslabel.Text = "Compressing, Please Wait"
            If passthrougharg = "query" Then sb_progresslabel.Text = "Analyzing"
            If passthrougharg = "uncompact" Then sb_progresslabel.Text = "Uncompressing..."

            TabControl1.SelectedTab = ProgressPage

        Catch ex As Exception
            Console.WriteLine(ex.Data)
        End Try

    End Sub




    Private Sub Queryaftercompact()
        isQueryMode = 1
        hasqueryfinished = 1
        RunCompact("query")
    End Sub




    Dim hasqueryfinished = 0


    Private Sub RunCompact(desiredfunction As String)

        If desiredfunction = "compact" Then

            isQueryCalledByCompact = 0
            compactArgs = "/C /I"

            If checkRecursiveScan.Checked = True Then compactArgs &= " /S"
            If checkForceCompression.Checked = True Then compactArgs &= " /F"
            If checkHiddenFiles.Checked = True Then compactArgs &= " /A"
            If compressX4.Checked = True Then compactArgs &= " /EXE:XPRESS4K"
            If compressX8.Checked = True Then compactArgs &= " /EXE:XPRESS8K"
            If compressX16.Checked = True Then compactArgs &= " /EXE:XPRESS16K"
            If compressLZX.Checked = True Then compactArgs &= " /EXE:LZX"


            RunCompact_ProcessGen(compactArgs)

            isQueryCalledByCompact = 1
            hasqueryfinished = 0
            isActive = 1

        ElseIf desiredfunction = "uncompact" Then

            isQueryCalledByCompact = 0

            compactArgs = "/U /S /EXE /I"

            If checkForceCompression.Checked = True Then compactArgs &= " /F"

            If checkHiddenFiles.Checked = True Then compactArgs &= " /A"


            RunCompact_ProcessGen(compactArgs)

            isActive = 1


        ElseIf desiredfunction = "query" Then

            compactArgs = "/S /Q /EXE /I"

            RunCompact_ProcessGen(compactArgs)



        End If

    End Sub




    Private Sub RunCompact_ProcessGen(passthroughArgs As String)
        MyProcess = New Process
        With MyProcess.StartInfo
            .FileName = "compact.exe"
            .WorkingDirectory = workingDir
            .Arguments = passthroughArgs
            .StandardOutputEncoding = CP
            .StandardErrorEncoding = CP
            .UseShellExecute = False
            .CreateNoWindow = True
            .RedirectStandardInput = True
            .RedirectStandardOutput = True
            .RedirectStandardError = True
        End With

        Console.WriteLine(MyProcess.StartInfo.Arguments)
        MyProcess.Start()
        MyProcess.PriorityClass = ProcessPriorityClass.BelowNormal
        MyProcess.EnableRaisingEvents = True
        MyProcess.BeginErrorReadLine()
        MyProcess.BeginOutputReadLine()
    End Sub




End Class