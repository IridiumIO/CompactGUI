Imports System.Text

Partial Class Compact
    Dim CP As Encoding
    Private Sub CreateProcess(passthrougharg As String)

        Try
            MyProcess.Kill()
        Catch ex As Exception
        End Try

        Try
            If passthrougharg = "compact" Then isQueryMode = 0
            If passthrougharg = "query" Then isQueryMode = 1
            If passthrougharg = "uncompact" Then isQueryMode = 0
            progresspercent.Visible = True

            If CP Is Nothing Then CP = getEncoding()

            Try
                RunCompact(passthrougharg, CP)

                If passthrougharg = "compact" Then sb_progresslabel.Text = "Compressing, Please Wait"
                If passthrougharg = "query" Then sb_progresslabel.Text = "Analyzing"

                TabControl1.SelectedTab = ProgressPage

            Catch ex As Exception
            End Try

        Catch ex As Exception
            Console.WriteLine(ex.Data)
        End Try

    End Sub




    Private Sub Queryaftercompact()
        isQueryMode = 1
        hasqueryfinished = 1
        RunCompact("query", CP)
    End Sub




    Dim hasqueryfinished = 0


    Private Sub RunCompact(desiredfunction As String, EC As Encoding)

        If desiredfunction = "compact" Then

            isQueryCalledByCompact = 0
            compactArgs = "/C /I"

            If checkRecursiveScan.Checked = True Then
                compactArgs = compactArgs + " /S"
            End If
            If checkForceCompression.Checked = True Then
                compactArgs = compactArgs + " /F"
            End If
            If checkHiddenFiles.Checked = True Then
                compactArgs = compactArgs + " /A"
            End If
            If compressX4.Checked = True Then
                compactArgs = compactArgs + " /EXE:XPRESS4K"
            End If
            If compressX8.Checked = True Then
                compactArgs = compactArgs + " /EXE:XPRESS8K"
            End If
            If compressX16.Checked = True Then
                compactArgs = compactArgs + " /EXE:XPRESS16K"
            End If
            If compressLZX.Checked = True Then
                compactArgs = compactArgs + " /EXE:LZX"
            End If

            RunCompact_ProcessGen(compactArgs, EC)

            isQueryCalledByCompact = 1
            hasqueryfinished = 0
            isActive = 1

        ElseIf desiredfunction = "uncompact" Then

            isQueryCalledByCompact = 0

            compactArgs = "/U /S /EXE /I"

            If checkForceCompression.Checked = True Then
                compactArgs = compactArgs + " /F"
            End If
            If checkHiddenFiles.Checked = True Then
                compactArgs = compactArgs + " /A"
            End If

            RunCompact_ProcessGen(compactArgs, EC)

            isActive = 1


        ElseIf desiredfunction = "query" Then

            compactArgs = "/S /Q /EXE /I"

            RunCompact_ProcessGen(compactArgs, EC)



        End If

    End Sub




    Private Sub RunCompact_ProcessGen(passthroughArgs As String, EC As Encoding)
        MyProcess = New Process
        With MyProcess.StartInfo
            .FileName = "compact.exe"
            .WorkingDirectory = workingDir                              'Set working directory via argument, allows Encoding to be passed directly.
            .Arguments = passthroughArgs
            .StandardOutputEncoding = CP                                'Allow console output to use the System's encoding for localization support
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