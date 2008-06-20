Option Explicit On
Option Strict On

Imports LainHelper
Imports LainSocket

Public Class clsProtocolLENPServer
    Private Const LENP As Byte = 1

    Public Enum Protocol As Byte
        LENP_PING = 1           '0x01
        LENP_VERSION = 2        '0x02
        LENP_MSG_USER = 10      '0x0A
        LENP_MSG_GAME = 11      '0x0B
    End Enum

    Public Event LENP_PING(ByVal sock As clsSocketTCPClient, ByVal protocolID As Byte, ByVal commandID As Byte, ByVal cookie As Byte, ByVal pingValue As Byte())
    Public Event LENP_VERSION(ByVal sock As clsSocketTCPClient, ByVal protocolID As Byte, ByVal commandID As Byte, ByVal cookie As Byte)
    Public Event LENP_MSG_USER(ByVal sock As clsSocketTCPClient, ByVal protocolID As Byte, ByVal commandID As Byte, ByVal cookie As Byte, ByVal userName As String, ByVal msg As String)
    Public Event LENP_MSG_GAME(ByVal sock As clsSocketTCPClient, ByVal protocolID As Byte, ByVal commandID As Byte, ByVal cookie As Byte, ByVal gameName As String, ByVal msg As String)


    Public Function AssignLength(ByVal content As Byte()) As Boolean
        Dim length As Integer
        Dim lengthByte As Byte()
        Try
            If content Is Nothing = False AndAlso content.Length >= 5 AndAlso content.Length <= 65535 Then
                lengthByte = clsHelper.LongToDWORD(content.Length, False)
                length = CType(clsHelper.ByteArrayToLong(New Byte() {lengthByte(0), lengthByte(1)}), Integer)
                If length = content.Length Then
                    content(3) = lengthByte(0)
                    content(4) = lengthByte(1)
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function ValidateLength(ByVal content As Byte()) As Boolean
        Dim length As Integer
        Try
            If content Is Nothing = False AndAlso content.Length >= 5 AndAlso content.Length <= 65535 Then
                length = CType(clsHelper.ByteArrayToLong(New Byte() {content(3), content(4)}), Integer)
                If length = content.Length Then
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function RECEIVE_LENP_PING(ByVal sock As clsSocketTCPClient, ByVal data As Byte()) As Boolean
        Dim cookie As Byte
        Dim pingValue As Byte()
        Try
            If ValidateLength(data) = True Then
                cookie = data(2)
                pingValue = New Byte(4 - 1) {}
                Array.Copy(data, 5, pingValue, 0, pingValue.Length)

                RaiseEvent LENP_PING(sock, LENP, Protocol.LENP_PING, cookie, pingValue)
                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_LENP_VERSION(ByVal sock As clsSocketTCPClient, ByVal data As Byte()) As Boolean
        Dim cookie As Byte
        Try
            If ValidateLength(data) = True Then
                cookie = data(2)
                RaiseEvent LENP_VERSION(sock, LENP, Protocol.LENP_VERSION, cookie)
                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_LENP_MSG_USER(ByVal sock As clsSocketTCPClient, ByVal data As Byte()) As Boolean
        Dim cookie As Byte
        Dim i As Integer
        Dim list As ArrayList
        Dim currentbyte As Byte
        Dim userName As String
        Dim msg As String

        Try
            If ValidateLength(data) = True Then
                cookie = data(2)
                i = 5

                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        userName = clsHelper.ByteArrayToStringASCII(CType(list.ToArray(GetType(Byte)), Byte()))
                        Exit Do
                    End If
                Loop

                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        msg = clsHelper.ByteArrayToStringASCII(CType(list.ToArray(GetType(Byte)), Byte()))
                        Exit Do
                    End If
                Loop

                RaiseEvent LENP_MSG_USER(sock, LENP, Protocol.LENP_MSG_USER, cookie, userName, msg)
                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_LENP_MSG_GAME(ByVal sock As clsSocketTCPClient, ByVal data As Byte()) As Boolean
        Dim cookie As Byte
        Dim i As Integer
        Dim list As ArrayList
        Dim currentbyte As Byte
        Dim gameName As String
        Dim msg As String
        Try
            If ValidateLength(data) = True Then
                cookie = data(2)
                i = 5

                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        gameName = clsHelper.ByteArrayToStringASCII(CType(list.ToArray(GetType(Byte)), Byte()))
                        Exit Do
                    End If
                Loop

                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        msg = clsHelper.ByteArrayToStringASCII(CType(list.ToArray(GetType(Byte)), Byte()))
                        Exit Do
                    End If
                Loop

                RaiseEvent LENP_MSG_GAME(sock, LENP, Protocol.LENP_MSG_GAME, cookie, gameName, msg)
                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function SEND_LENP_PING(ByVal cookie As Byte, ByVal pingValue As Byte()) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try

            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {LENP})                   'LENP ID
            clsHelper.AddByteArray(packet, New Byte() {Protocol.LENP_PING})     'LENP_PING
            clsHelper.AddByteArray(packet, New Byte() {cookie})                 'cookie
            clsHelper.AddByteArray(packet, New Byte() {0, 0})                   'undefined packet length
            clsHelper.AddByteArray(packet, pingValue)                           'ping value

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer

        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_LENP_VERSION(ByVal cookie As Byte, ByVal version As String) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try

            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {LENP})                       'LENP ID
            clsHelper.AddByteArray(packet, New Byte() {Protocol.LENP_VERSION})      'LENP_VERSION
            clsHelper.AddByteArray(packet, New Byte() {cookie})                     'cookie
            clsHelper.AddByteArray(packet, New Byte() {0, 0})                       'undefined packet length
            clsHelper.AddByteArray(packet, version)                                 'version

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer

        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_LENP_MSG_USER(ByVal cookie As Byte, ByVal totalReceiver As Byte) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try

            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {LENP})                       'LENP ID
            clsHelper.AddByteArray(packet, New Byte() {Protocol.LENP_MSG_USER})     'LENP_MSG_USER
            clsHelper.AddByteArray(packet, New Byte() {cookie})                     'cookie
            clsHelper.AddByteArray(packet, New Byte() {0, 0})                       'undefined packet length
            clsHelper.AddByteArray(packet, New Byte() {totalReceiver})              'number of receivers

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer

        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_LENP_MSG_GAME(ByVal cookie As Byte, ByVal totalReceiver As Byte) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try

            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {LENP})                       'LENP ID
            clsHelper.AddByteArray(packet, New Byte() {Protocol.LENP_MSG_GAME})     'LENP_MSG_GAME
            clsHelper.AddByteArray(packet, New Byte() {cookie})                     'cookie
            clsHelper.AddByteArray(packet, New Byte() {0, 0})                       'undefined packet length
            clsHelper.AddByteArray(packet, New Byte() {totalReceiver})              'number of receivers

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer

        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function




End Class
