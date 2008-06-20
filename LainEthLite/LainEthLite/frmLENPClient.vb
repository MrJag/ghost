Option Explicit On
Option Strict On


Imports LainSocket
Imports LainBnetCore
Imports LainHelper

Public Class frmLENPClient
    Private WithEvents sockClient As clsSocketTCPClient
    Private buffer As ArrayList


#Region "sock Event"
    Private Sub sockClient_OnEventError(ByVal errorFunction As String, ByVal errorString As String, ByVal socket As LainSocket.clsSocketTCP) Handles sockClient.EventError
        Try
            [Stop]()
            VerifyConnectionStatus("Connection Error")
            Debug.WriteLine(errorFunction & " : " & errorString)
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub
    Private Sub sockClient_OnEventMessage(ByVal socketEvent As LainSocket.clsSocketTCP.SocketEvent, ByVal data As Object, ByVal socket As LainSocket.clsSocketTCP) Handles sockClient.EventMessage
        Dim dataQ As Queue
        Dim client As clsSocketTCPClient
        Dim buffer As Byte()
        Dim list As ArrayList

        Try
            client = CType(socket, clsSocketTCPClient)
            Select Case socketEvent
                Case clsSocketTCP.SocketEvent.ConnectionEstablished
                    VerifyConnectionStatus("Connection Established")
                Case clsSocketTCP.SocketEvent.ConnectionFailed
                    [Stop]()
                    VerifyConnectionStatus("Connection Failed")
                Case clsSocketTCP.SocketEvent.ConnectionClosed
                    [Stop]()
                    VerifyConnectionStatus("Connection Closed")
                Case clsSocketTCP.SocketEvent.DataArrival
                    dataQ = client.GetReceiveQueue
                    list = New ArrayList

                    Do Until dataQ.Count = 0
                        list.Add(dataQ.Dequeue())
                    Loop

                    If list.Count > 0 Then
                        buffer = CType(list.ToArray(GetType(Byte)), Byte())

                        If checkDec.Checked Then
                            txtChatLog.Invoke(New clsHelper.DelegateTextBoxAppend(AddressOf clsHelper.ControlTextBoxAppend), New Object() {txtChatLog, Environment.NewLine})
                            txtChatLog.Invoke(New clsHelper.DelegateTextBoxAppend(AddressOf clsHelper.ControlTextBoxAppend), New Object() {txtChatLog, clsHelper.PrintArray(buffer, clsHelper.PrintType.DEC)})
                        End If

                        If checkHex.Checked Then
                            txtChatLog.Invoke(New clsHelper.DelegateTextBoxAppend(AddressOf clsHelper.ControlTextBoxAppend), New Object() {txtChatLog, Environment.NewLine})
                            txtChatLog.Invoke(New clsHelper.DelegateTextBoxAppend(AddressOf clsHelper.ControlTextBoxAppend), New Object() {txtChatLog, clsHelper.PrintArray(buffer, clsHelper.PrintType.HEX)})
                        End If

                        If checkASCII.Checked Then
                            txtChatLog.Invoke(New clsHelper.DelegateTextBoxAppend(AddressOf clsHelper.ControlTextBoxAppend), New Object() {txtChatLog, Environment.NewLine})
                            txtChatLog.Invoke(New clsHelper.DelegateTextBoxAppend(AddressOf clsHelper.ControlTextBoxAppend), New Object() {txtChatLog, clsHelper.PrintArray(buffer, clsHelper.PrintType.ASCII)})
                        End If

                        txtChatLog.Invoke(New clsHelper.DelegateTextBoxAppend(AddressOf clsHelper.ControlTextBoxAppend), New Object() {txtChatLog, Environment.NewLine})
                    End If

            End Select
        Catch ex As Exception
            Debug.Write(ex)
        End Try

    End Sub
#End Region

    Private Sub frmLENPClient_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        sockClient.Stop()
    End Sub


    Private Sub frmLENPClient_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        sockClient = New clsSocketTCPClient()
        buffer = ArrayList.Synchronized(New ArrayList)
        comboDataType.SelectedIndex = comboDataType.Items.Count - 1
        VerifyConnectionStatus("LENP Client Simulator")
    End Sub

    Private Sub buttonGo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonGo.Click
        If sockClient.IsConnected Then
            sockClient.Stop()
        Else
            sockClient.Connect(clsSocketTCP.GetFirstIP(txtIP.Text), CType(txtPort.Text, Integer))
        End If

    End Sub


    Private Sub buttonAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonAdd.Click
        Dim datatype As String

        Try
            If txtInput.Text.Length > 0 Then
                datatype = CType(comboDataType.Items(comboDataType.SelectedIndex), String)
                Select Case datatype
                    Case "Byte"
                        If IsNumeric(txtInput.Text) AndAlso CType(txtInput.Text, Integer) >= 0 AndAlso CType(txtInput.Text, Integer) <= 255 Then
                            clsHelper.AddByteArray(buffer, New Byte() {CType(txtInput.Text, Byte)})
                        Else
                            MessageBox.Show("Value must be a byte", frmLainEthLite.ProjectLainVersion, MessageBoxButtons.OK, MessageBoxIcon.Error)
                        End If

                    Case "WORD"
                        If IsNumeric(txtInput.Text) AndAlso CType(txtInput.Text, Integer) >= 0 Then
                            clsHelper.AddByteArray(buffer, clsHelper.IntegerToWORD(CType(txtInput.Text, Integer), False))
                        Else
                            MessageBox.Show("Value must be a WORD", frmLainEthLite.ProjectLainVersion, MessageBoxButtons.OK, MessageBoxIcon.Error)
                        End If
                    Case "DWORD"
                        If IsNumeric(txtInput.Text) AndAlso CType(txtInput.Text, Integer) >= 0 Then
                            clsHelper.AddByteArray(buffer, clsHelper.LongToDWORD(CType(txtInput.Text, Integer), False))
                        Else
                            MessageBox.Show("Value must be a byte", frmLainEthLite.ProjectLainVersion, MessageBoxButtons.OK, MessageBoxIcon.Error)
                        End If
                    Case "String"
                        clsHelper.AddByteArray(buffer, txtInput.Text)
                    Case Else
                End Select
            End If
            FocusInputBox()
            ShowBufferContent()
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try


    End Sub
    Private Sub buttonDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonDelete.Click
        If buffer.Count > 0 Then
            buffer.RemoveAt(buffer.Count - 1)
        End If
        FocusInputBox()
        ShowBufferContent()
    End Sub
    Private Sub buttonClear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonClear.Click
        buffer.Clear()
        FocusInputBox()
        ShowBufferContent()
    End Sub
    Private Sub buttonAssignLength_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonAssignLength.Click
        AssignLength(buffer)
        FocusInputBox()
        ShowBufferContent()
    End Sub

    Private Sub buttonSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonSend.Click
        Dim data As Byte()

        data = CType(buffer.ToArray(GetType(Byte)), Byte())
        sockClient.Send(data)
        FocusInputBox()

    End Sub
    Private Sub comboDataType_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles comboDataType.SelectedIndexChanged
        FocusInputBox()
    End Sub

    Private Sub ShowBufferContent()
        Dim data As Byte()

        data = CType(buffer.ToArray(GetType(Byte)), Byte())
        txtBuffer.Text = clsHelper.PrintArray(data)
    End Sub

    Private Sub FocusInputBox()
        txtInput.Focus()
        txtInput.SelectionStart = 0
        txtInput.SelectionLength = txtInput.Text.Length
    End Sub

    Private Sub [Stop]()
        sockClient.Stop()
    End Sub
    Private Sub VerifyConnectionStatus(ByVal status As String)
        Try
            labelStatus.Invoke(New clsHelper.DelegateControlText(AddressOf clsHelper.ControlText), New Object() {labelStatus, status})
            groupIO.Invoke(New clsHelper.DelegateControlEnabled(AddressOf clsHelper.ControlEnabled), New Object() {groupIO, sockClient.IsConnected})
            groupConnection.Invoke(New clsHelper.DelegateControlEnabled(AddressOf clsHelper.ControlEnabled), New Object() {groupConnection, Not sockClient.IsConnected})

            If sockClient.IsConnected Then
                labelStatus.Invoke(New clsHelper.DelegateControlBackColor(AddressOf clsHelper.ControlBackColor), New Object() {labelStatus, Color.LimeGreen})
            Else
                labelStatus.Invoke(New clsHelper.DelegateControlBackColor(AddressOf clsHelper.ControlBackColor), New Object() {labelStatus, Color.DarkGray})
            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try

    End Sub
    Private Function AssignLength(ByVal content As ArrayList) As Boolean
        Dim length As Integer
        Dim lengthByte As Byte()
        Try
            If content Is Nothing = False AndAlso content.Count >= 5 AndAlso content.Count <= 65535 Then
                lengthByte = clsHelper.LongToDWORD(content.Count, False)
                length = CType(clsHelper.ByteArrayToLong(New Byte() {lengthByte(0), lengthByte(1)}), Integer)
                If length = content.Count Then
                    content.Item(3) = lengthByte(0)
                    content.Item(4) = lengthByte(1)
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function




End Class