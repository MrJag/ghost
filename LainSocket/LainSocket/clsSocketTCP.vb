Option Explicit On
Option Strict On

Imports System.Net
Imports System.Net.Sockets

Public Class clsSocketData
    Private buffer As Byte()
    Private sock As Socket

    Public Sub New(ByVal socket As Socket)
        sock = socket
        buffer = New Byte(2048 - 1) {}
    End Sub
    Public Sub New(ByVal socket As Socket, ByVal data As Byte())
        sock = socket
        buffer = data
    End Sub

    Public Function GetBuffer() As Byte()
        Return buffer
    End Function
    Public Function GetSocket() As Socket
        Return sock
    End Function
End Class
Public MustInherit Class clsSocketTCP
    Public Enum SocketEvent
        ConnectionEstablished
        ConnectionAccepted
        ConnectionFailed
        ConnectionClosed
        DataArrival
    End Enum

    Public Shared Function ConvertIP(ByVal ip As String) As Byte()
        Dim octets As String()
        Try
            octets = ip.Split(Convert.ToChar("."))
            If octets.Length = 4 AndAlso IsNumeric(octets(0)) AndAlso IsNumeric(octets(1)) AndAlso IsNumeric(octets(2)) AndAlso IsNumeric(octets(3)) Then
                Return New Byte() {CType(octets(0), Byte), CType(octets(1), Byte), CType(octets(2), Byte), CType(octets(3), Byte)}
            End If
            Return New Byte() {}
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return New Byte() {}
        End Try
    End Function
    Public Shared Function GetFirstIP(ByVal name As String) As String
        Dim IPs As String()
        Try
            IPs = GetIP(name)
            If IPs.Length > 0 Then
                Return IPs(0)
            End If
            Return ""
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return ""
        End Try
    End Function

    Public Shared Function GetIP() As String()
        Return GetIP(Environment.MachineName)
    End Function
    Public Shared Function GetIP(ByVal name As String) As String()
        Dim list As ArrayList
        Dim IP As IPAddress
        Try
            list = New ArrayList
            For Each IP In Dns.GetHostAddresses(name)
                list.Add(IP.ToString)
            Next
            Return CType(list.ToArray(GetType(String)), String())
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return New String() {}
        End Try
    End Function
End Class

Public Class clsSocketTCPClient
    Inherits clsSocketTCP

    Private client As Socket
    Private bufferReceiveQueue As Queue
    Private bufferSendQueue As Queue
    Private sendingInProgress As Boolean

    Public Event EventMessage(ByVal socketEvent As SocketEvent, ByVal data As Object, ByVal socket As clsSocketTCP)
    Public Event EventError(ByVal errorFunction As String, ByVal errorString As String, ByVal socket As clsSocketTCP)

    Public Sub New()
        client = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        sendingInProgress = False
        bufferReceiveQueue = Queue.Synchronized(New Queue)
        bufferSendQueue = Queue.Synchronized(New Queue)
    End Sub

    Public Function GetLocalIP() As Byte()
        Try
            If IsConnected() Then
                Return CType(client.LocalEndPoint, IPEndPoint).Address.GetAddressBytes
            End If
            Return New Byte() {}
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return New Byte() {}
        End Try
    End Function
    Public Function GetRemoteIP() As Byte()
        Try
            If IsConnected() Then
                Return CType(client.RemoteEndPoint, IPEndPoint).Address.GetAddressBytes
            End If
            Return New Byte() {}
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return New Byte() {}
        End Try

    End Function

    Public Function IsConnected() As Boolean
        Try
            Return client.Connected
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return False
        End Try
    End Function


    Public Function GetReceiveQueue() As Queue
        Return bufferReceiveQueue
    End Function

    Public Function GetSendQueue() As Queue
        Return bufferSendQueue
    End Function

    Public Function Dispose() As Boolean
        Try
            client.Close()
            Return True
        Catch ex As Exception
            Debug.WriteLine("Dispose")
            Return False
        End Try
    End Function
    Public Function [Stop]() As Boolean
        Try
            If client.Connected Then
                client.Shutdown(SocketShutdown.Send)
            End If

            Return True
        Catch ex As Exception
            Debug.WriteLine("[Stop]")
            Return False
        End Try
    End Function
    Public Function Accept(ByVal sock As Socket) As clsSocketTCPClient
        Try
            [Stop]()
            client = sock
            If client.Connected = True Then
                RaiseEvent EventMessage(SocketEvent.ConnectionEstablished, "Client is connected", Me)
                BeginReceive()
            Else
                RaiseEvent EventMessage(SocketEvent.ConnectionFailed, "Client failed to accept connection", Me)
            End If
            Return Me
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return Nothing
        End Try
    End Function
    Public Function Connect(ByVal IP As String, ByVal port As Integer) As Boolean
        Try
            If IP.Length > 0 Then
                [Stop]()
                client = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                client.Blocking = False
                client.BeginConnect(New IPEndPoint(IPAddress.Parse(IP), port), New AsyncCallback(AddressOf CallBackConnect), Me)
                Return True
            End If
            Return False
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return False
        End Try
    End Function

    Public Function Send(ByVal data As String) As Boolean
        Return Send(New System.Text.ASCIIEncoding().GetBytes(data))
    End Function
    Public Function Send(ByVal data As Byte()) As Boolean
        Try
            bufferSendQueue.Enqueue(data)
            If sendingInProgress = False Then
                sendingInProgress = True
                BeginSend()
            End If
            Return True
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return False
        End Try
    End Function

    Private Sub CallBackSend(ByVal ar As IAsyncResult)
        Dim sentSize As Integer
        Dim sockData As clsSocketData
        Try
            If ar Is Nothing = False Then
                sockData = CType(ar.AsyncState, clsSocketData)
                sentSize = client.EndSend(ar)
                If sentSize = sockData.GetBuffer.Length Then
                    BeginSend()
                Else
                    RaiseEvent EventError("CallBackSend", "Sent Inconsistent Data", Me)
                End If
            Else
                RaiseEvent EventError("CallBackSend", "socket already disposed", Me)
            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
            RaiseEvent EventError("CallBackSend", ex.ToString, Me)
        End Try
    End Sub
    Private Sub CallBackConnect(ByVal ar As IAsyncResult)
        Try
            If client.Connected = True Then
                RaiseEvent EventMessage(SocketEvent.ConnectionEstablished, "Client is connected", Me)
                BeginReceive()
            Else
                RaiseEvent EventMessage(SocketEvent.ConnectionFailed, "Client failed to connect", Me)
            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
            RaiseEvent EventError("CallBackConnect", ex.ToString, Me)
        End Try
    End Sub
    Private Sub CallBackDataArrival(ByVal ar As IAsyncResult)
        Dim sockData As clsSocketData
        Dim receiveSize As Integer
        Dim buffer As Byte()
        Try
            If ar Is Nothing = False AndAlso ar.AsyncState Is Nothing = False Then
                sockData = CType(ar.AsyncState, clsSocketData)
                If sockData Is Nothing = False AndAlso sockData.GetSocket() Is Nothing = False Then
                    client = sockData.GetSocket()

                    receiveSize = client.EndReceive(ar)
                    If receiveSize > 0 Then
                        buffer = New Byte(receiveSize - 1) {}
                        Array.Copy(sockData.GetBuffer(), buffer, buffer.Length)
                        SyncLock bufferReceiveQueue.SyncRoot()
                            For Each block As Byte In buffer
                                bufferReceiveQueue.Enqueue(block)
                            Next
                        End SyncLock
                        RaiseEvent EventMessage(SocketEvent.DataArrival, buffer.Clone, Me)
                        BeginReceive()
                    Else
                        RaiseEvent EventMessage(SocketEvent.ConnectionClosed, New Byte() {}, Me)
                    End If
                Else
                    RaiseEvent EventError("CallBackDataArrival", "null refernces", Me)
                End If
            Else
                RaiseEvent EventError("CallBackDataArrival", "null refernces", Me)
            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
            RaiseEvent EventError("CallBackDataArrival", ex.ToString, Me)
        End Try
    End Sub
    Private Sub BeginSend()
        Dim sockData As clsSocketData
        Dim data As Byte()

        Try
            If client.Connected = True AndAlso bufferSendQueue.Count > 0 Then
                data = CType(bufferSendQueue.Dequeue, Byte())
                sockData = New clsSocketData(client, data)
                client.BeginSend(data, 0, data.Length, SocketFlags.None, New AsyncCallback(AddressOf CallBackSend), sockData)
            Else
                sendingInProgress = False
            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
            RaiseEvent EventError("BeginSend", ex.ToString, Me)
        End Try
    End Sub
    Private Sub BeginReceive()
        Dim sockData As clsSocketData
        Try
            sockData = New clsSocketData(client)
            client.BeginReceive(sockData.GetBuffer(), 0, sockData.GetBuffer().Length, SocketFlags.None, New AsyncCallback(AddressOf CallBackDataArrival), sockData)
        Catch ex As Exception
            Debug.WriteLine(ex)
            RaiseEvent EventError("BeginReceive", ex.ToString, Me)
        End Try
    End Sub


End Class

Public Class clsSocketTCPServer
    Inherits clsSocketTCP

    Private server As Socket

    Public Event eventMessage(ByVal socketEvent As SocketEvent, ByVal data As Object, ByVal socket As clsSocketTCP)
    Public Event eventError(ByVal errorFunction As String, ByVal errorString As String, ByVal socket As clsSocketTCP)


    Public Sub New()
        server = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    End Sub

    Public Function GetListeningPort() As Integer
        Try
            If IsListening() Then
                Return CType(server.LocalEndPoint, IPEndPoint).Port
            End If
            Return 0
        Catch ex As Exception
            Return 0
        End Try
    End Function
    Public Function IsListening() As Boolean
        Return server.IsBound
    End Function

    Public Function Dispose() As Boolean
        Try
            server.Close()
            Return True
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return False
        End Try
    End Function

    Public Function [Stop]() As Boolean
        Try
            If server.IsBound Then
                Dispose()
                server = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            End If
            Return True
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return False
        End Try
    End Function

    Public Function Listen(ByVal ip As String, ByVal port As Integer) As Boolean
        Try
            [Stop]()
            server = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            server.Blocking = False
            If ip.Length = 0 Then
                server.Bind(New IPEndPoint(IPAddress.Any, port))
            Else
                server.Bind(New IPEndPoint(IPAddress.Parse(ip), port))
            End If
            server.Listen(5)
            server.BeginAccept(New AsyncCallback(AddressOf CallBackAccept), server)
            Return True
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return False
        End Try
    End Function
    Public Function Listen(ByVal port As Integer) As Boolean
        Return Listen("", port)
    End Function

    Private Sub CallBackAccept(ByVal ar As IAsyncResult)
        Dim client As clsSocketTCPClient
        Try
            If ar Is Nothing = False AndAlso TypeOf (ar.AsyncState) Is Socket Then
                client = New clsSocketTCPClient
                If client.Accept(server.EndAccept(ar)) Is Nothing = False AndAlso client.IsConnected Then
                    RaiseEvent eventMessage(clsSocketTCP.SocketEvent.ConnectionAccepted, client, Me)
                End If
                server.BeginAccept(New AsyncCallback(AddressOf CallBackAccept), server)
            Else
                RaiseEvent eventError("CallBackAccept", "socket already disposed", Me)
            End If
        Catch ex As Exception
            Debug.WriteLine("CallBackAccept")
            RaiseEvent eventError("CallBackAccept", ex.ToString, Me)
        End Try
    End Sub


End Class