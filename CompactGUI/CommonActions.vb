﻿Module CommmonActions

    Sub PrepareforCompact()

        With Compact

            .btnCompress.Visible = True
            .btnAnalyze.Enabled = True
            .btnUncompress.Visible = False

            .returnArrow.Visible = False
            .checkShutdownOnCompletion.Checked = False

            .CompResultsPanel.Visible = False
            .sb_AnalysisPanel.Visible = False
            .sb_ResultsPanel.Visible = False
            .TabControl1.SelectedTab = .InputPage

            .sb_labelCompressed.Text = "Estimated Compressed"

        End With

    End Sub


    Sub ActionBegun(Mode As String)
        With Compact

            Select Case Mode
                Case "C"
                    .isActive = True
                    .isQueryMode = False

                    .btnCompress.Visible = False
                    .btnAnalyze.Enabled = False

                    .sb_progresslabel.Text = "Compressing, Please Wait"
                    .sb_progresspercent.Visible = True

                    .sb_AnalysisPanel.Visible = True

                    .TabControl1.SelectedTab = .ProgressPage

                Case "U"
                    .isActive = True
                    .isQueryMode = False

                    .btnUncompress.Visible = False
                    .btnAnalyze.Enabled = False

                    .sb_progresslabel.Text = "Uncompressing..."
                    .sb_progresspercent.Visible = True

                    .CompResultsPanel.Visible = False

                Case "A"
                    .isQueryMode = True

                    .btnCompress.Visible = False
                    .btnAnalyze.Enabled = False


                    .conOut.Visible = True
                    .checkShowConOut.Checked = True

                    .sb_progresslabel.Text = "Analyzing..."
                    .sb_progressbar.Width = 0

                    .sb_Panel.Visible = True
                    .sb_AnalysisPanel.Visible = True

                    .AllFiles.Clear()
                    .TreeData.Clear()

                    .TabControl1.SelectedTab = .ProgressPage

            End Select

        End With



    End Sub

    Sub ActionCompleted(Mode As String, Optional AnalysisShowsFolderIsCompressed As Boolean = True)
        With Compact

            Select Case Mode
                Case "C"
                    .returnArrow.Visible = True
                    .btnAnalyze.Enabled = True
                    .btnUncompress.Visible = True
                    .isActive = False
                Case "U"
                    .returnArrow.Visible = True
                    .btnAnalyze.Enabled = True
                    .btnUncompress.Visible = False
                    .sb_ResultsPanel.Visible = False

                    .sb_progresslabel.Text = "Finished Uncompressing"
                Case "A"
                    If AnalysisShowsFolderIsCompressed Then
                        .sb_progresslabel.Text = "This Folder Contains Compressed Files"
                        .sb_labelCompressed.Text = "Compressed"

                        .btnUncompress.Visible = True

                        .CompResultsPanel.Visible = True
                        .sb_ResultsPanel.Visible = True

                    Else
                        .sb_progresslabel.Text = "This Folder is Not Compressed"
                        .btnUncompress.Visible = False
                    End If
                    .returnArrow.Visible = True
                    .isQueryMode = False

            End Select

            .isActive = False
            .WorkingList.Clear()
        End With

    End Sub




End Module
