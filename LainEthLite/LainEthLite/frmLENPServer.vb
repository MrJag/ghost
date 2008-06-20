Option Explicit On
Option Strict On

Imports LainHelper
Imports LainSocket

Public Class frmLENPServer

    Private lenp As clsLENPServer


    Private Sub buttonGo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonGo.Click
        If lenp.IsHostOnline Then
            lenp.HostStop()
        Else
            lenp.HostStart(CType(txtServerPort.Text, Integer))
        End If
        VerifyServerStatus()
    End Sub

    Private Sub frmLENPServer_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim IP As String

        lenp = frmLainEthLite.GetLENPServer
        For Each IP In clsSocketTCP.GetIP()
            comboServerIP.Items.Add(IP)
        Next

    End Sub

    Private Sub frmLENPServer_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        e.Cancel = True
        Me.Visible = False
    End Sub

    Private Sub VerifyServerStatus()
        Try
            If lenp.IsHostOnline Then
                labelStatus.BackColor = Color.LimeGreen
                groupServerConfig.Enabled = False
            Else
                labelStatus.BackColor = Color.DarkGray
                groupServerConfig.Enabled = True
            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try

    End Sub


End Class


































'