Option Explicit On
Option Strict On

Imports System.Threading
Imports LainSocket
Imports LainHelper
Imports LainBnetCore


Public Class clsLENPServer
    Private serverPort As Integer

    Private listHost As ArrayList
    Private sockServer As clsSocketTCPServer
    Private hashClient As Hashtable     'Socket -> Arraylist
    Private queuePacket As Queue

    Private WithEvents protocol As clsProtocolLENPServer


    Public Sub New(ByVal games As ArrayList)
        serverPort = 0
        sockServer = New clsSocketTCPServer
        hashClient = Hashtable.Synchronized(New Hashtable)
        queuePacket = Queue.Synchronized(New Queue)
        protocol = New clsProtocolLENPServer
        listHost = games

        AddHandler sockServer.eventMessage, AddressOf sockServer_OnEventMessage
        AddHandler sockServer.eventError, AddressOf sockServer_OnEventError
    End Sub

#Region "protocol events"
    Private Sub protocol_LENP_PING(ByVal sock As clsSocketTCPClient, ByVal protocolID As Byte, ByVal commandID As Byte, ByVal cookie As Byte, ByVal pingValue As Byte()) Handles protocol.LENP_PING
        Try
            sock.Send(protocol.SEND_LENP_PING(cookie, pingValue))
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub
    Private Sub protocol_LENP_VERSION(ByVal sock As clsSocketTCPClient, ByVal protocolID As Byte, ByVal commandID As Byte, ByVal cookie As Byte) Handles protocol.LENP_VERSION
        Try
            sock.Send(protocol.SEND_LENP_VERSION(cookie, frmLainEthLite.ProjectLainVersion))
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub
    Private Sub protocol_LENP_MSG_USER(ByVal sock As clsSocketTCPClient, ByVal protocolID As Byte, ByVal commandID As Byte, ByVal cookie As Byte, ByVal userName As String, ByVal msg As String) Handles protocol.LENP_MSG_USER
        Dim game As clsGameHost
        Dim total As Integer

        Try
            total = 0
            For Each game In listHost
                total = total + game.SendMessage(userName, msg)
            Next
            sock.Send(protocol.SEND_LENP_MSG_USER(cookie, CType(total Mod 256, Byte)))
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub
    Private Sub protocol_LENP_MSG_GAME(ByVal sock As clsSocketTCPClient, ByVal protocolID As Byte, ByVal commandID As Byte, ByVal cookie As Byte, ByVal gameName As String, ByVal msg As String) Handles protocol.LENP_MSG_GAME
        Dim game As clsGameHost
        Dim total As Integer

        Try
            total = 0
            For Each game In listHost
                If gameName.ToLower = game.GetGameName.ToLower Then
                    total = total + game.SendMessage(msg)
                End If
            Next
            sock.Send(protocol.SEND_LENP_MSG_GAME(cookie, CType(total Mod 256, Byte)))
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub
#End Region

#Region "sock event"
    Private Sub sockServer_OnEventMessage(ByVal socketEvent As clsSocketTCP.SocketEvent, ByVal data As Object, ByVal socket As clsSocketTCP)
        Dim client As clsSocketTCPClient

        Select Case socketEvent
            Case clsSocketTCP.SocketEvent.ConnectionAccepted
                client = CType(data, clsSocketTCPClient)
                AddHandler client.eventMessage, AddressOf client_OnEventMessage
                AddHandler client.eventError, AddressOf client_OnEventError

                hashClient.Add(client, ArrayList.Synchronized(New ArrayList))
                client_OnEventMessage(clsSocketTCP.SocketEvent.DataArrival, New Byte() {}, client)  'force a data arrival event

                Debug.WriteLine("client join")
        End Select
    End Sub
    Private Sub sockServer_OnEventError(ByVal errorFunction As String, ByVal errorString As String, ByVal socket As clsSocketTCP)
        HostStop()
    End Sub
    Private Sub client_OnEventMessage(ByVal socketEvent As clsSocketTCP.SocketEvent, ByVal data As Object, ByVal socket As clsSocketTCP)
        Dim dataQ As Queue
        Dim client As clsSocketTCPClient
        Dim mutexPackageGamePacket As Mutex
        Dim mutexProcessAllPacket As Mutex

        client = CType(socket, clsSocketTCPClient)
        Select Case socketEvent
            Case clsSocketTCP.SocketEvent.ConnectionClosed
                ClientStop(client)
            Case clsSocketTCP.SocketEvent.ConnectionFailed
                ClientStop(client)
            Case clsSocketTCP.SocketEvent.DataArrival
                mutexPackageGamePacket = New Mutex(False, String.Format("mutex-PackageGamePacket-{0}", client.GetHashCode)) 'different client process will run parrallel, same client will wait for mutex
                mutexPackageGamePacket.WaitOne()
                dataQ = client.GetReceiveQueue
                Do Until dataQ.Count = 0
                    CType(hashClient.Item(client), ArrayList).Add(dataQ.Dequeue())
                Loop
                PackageGamePacket(client)
                mutexPackageGamePacket.ReleaseMutex()

                mutexProcessAllPacket = New Mutex(False, String.Format("mutex-ProcessAllPacket-{0}", Me.GetHashCode)) 'differnt class instance process will run parrallel, same class instance will wait for mutex
                mutexProcessAllPacket.WaitOne()
                ProcessAllPacket()
                mutexProcessAllPacket.ReleaseMutex()
        End Select
    End Sub
    Private Sub client_OnEventError(ByVal errorFunction As String, ByVal errorString As String, ByVal socket As clsSocketTCP)
        Debug.WriteLine("sock error")
        ClientStop(CType(socket, clsSocketTCPClient))
    End Sub
#End Region


    Private Sub ProcessAllPacket()
        Dim command As clsCommandPacket
        Dim result As Boolean

        Try
            While queuePacket.Count > 0
                command = CType(queuePacket.Dequeue, clsCommandPacket)
                If command.GetPacketCommandType = clsCommandPacket.PacketType.LENP Then
                    Select Case command.GetPacketID
                        Case clsProtocolLENPServer.Protocol.LENP_PING : result = protocol.RECEIVE_LENP_PING(command.GetPacketSocket, command.GetPacketData())
                        Case clsProtocolLENPServer.Protocol.LENP_VERSION : result = protocol.RECEIVE_LENP_VERSION(command.GetPacketSocket, command.GetPacketData())
                        Case clsProtocolLENPServer.Protocol.LENP_MSG_USER : result = protocol.RECEIVE_LENP_MSG_USER(command.GetPacketSocket, command.GetPacketData())
                        Case clsProtocolLENPServer.Protocol.LENP_MSG_GAME : result = protocol.RECEIVE_LENP_MSG_GAME(command.GetPacketSocket, command.GetPacketData())
                        Case Else : result = False
                    End Select

                    If result = False Then
                        ClientStop(command.GetPacketSocket)
                    End If
                End If
            End While
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub
    Private Sub PackageGamePacket(ByVal client As clsSocketTCPClient)
        Dim length As Integer
        Dim listPacket As ArrayList
        Dim i As Integer
        Dim data As Byte()
        Dim listDataBuffer As ArrayList

        Try
            If hashClient.Contains(client) Then
                listDataBuffer = CType(hashClient.Item(client), ArrayList)

                While listDataBuffer.Count >= 5
                    If CByte(listDataBuffer.Item(0)) = 1 Then
                        length = CType(clsHelper.ByteArrayToLong(New Byte() {CByte(listDataBuffer.Item(3)), CByte(listDataBuffer.Item(4))}), Integer)
                        If listDataBuffer.Count >= length Then
                            listPacket = New ArrayList
                            For i = 1 To length
                                listPacket.Add(listDataBuffer.Item(0))
                                listDataBuffer.RemoveAt(0)
                            Next
                            data = CType(listPacket.ToArray(GetType(Byte)), Byte())
                            queuePacket.Enqueue(New clsCommandPacket(clsCommandPacket.PacketType.LENP, data(1), data, client))
                        Else
                            Exit While
                        End If
                    Else
                        ClientStop(client)
                        Exit While
                    End If
                End While
            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub
    Private Function ClientStop(ByVal client As clsSocketTCPClient) As Boolean

        Try
            Debug.WriteLine("client stop")

            hashClient.Remove(client)
            client.Stop()
            RemoveHandler client.EventMessage, AddressOf client_OnEventMessage
            RemoveHandler client.EventError, AddressOf client_OnEventError

            Return True
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return False
        End Try

    End Function

    Public Function HostStop() As Boolean
        Try
            sockServer.Stop()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function HostStart(ByVal port As Integer) As Boolean
        Try
            serverPort = port
            If sockServer.Listen(serverPort) Then
                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function IsHostOnline() As Boolean
        Return sockServer.IsListening
    End Function





End Class


























'