Option Explicit On
Option Strict On

Imports System.Threading
Imports LainSocket
Imports LainHelper

#Region "bnet related"
Public Class clsBNETCustomData
    Private exeversion As Byte()
    Private exeversionhash As Byte()
    Private passwordhashtype As String

    Public Sub New()
        Me.exeversion = New Byte() {}
        Me.exeversionhash = New Byte() {}
        Me.passwordhashtype = ""
    End Sub

    Public Function SetPasswordHashType(ByVal passwordhashtype As String) As Boolean
        Me.passwordhashtype = passwordhashtype
    End Function
    Public Function SetExeVersion(ByVal exeversion As Byte()) As Boolean
        If exeversion.Length = 4 Then
            Me.exeversion = exeversion
            Return True
        End If
        Return False
    End Function
    Public Function SetExeVersionHash(ByVal exeversionhash As Byte()) As Boolean
        If exeversionhash.Length = 4 Then
            Me.exeversionhash = exeversionhash
            Return True
        End If
        Return False
    End Function

    Public Function GetPasswordHashType() As String
        Return passwordhashtype
    End Function
    Public Function GetExeVersion() As Byte()
        Return exeversion
    End Function
    Public Function GetExeVersionHash() As Byte()
        Return exeversionhash
    End Function
End Class
Public Class clsBNETChatMessage
    Private msg As String
    Private owner As String
    Private isPersistent As Boolean

    Public Sub New(ByVal msg As String, ByVal owner As String, ByVal isPersistent As Boolean)
        Me.msg = msg
        Me.owner = owner
        Me.isPersistent = isPersistent
    End Sub
    Public Function GetMsg() As String
        Return msg
    End Function
    Public Function GetOwner() As String
        Return owner
    End Function
    Public Function GetIsPersistent() As Boolean
        Return isPersistent
    End Function
End Class
#End Region

Public Class clsBNET

    Public Enum LogOnFailPoints
        AllPass
        NamePasswordInvalid
        ROCKeyInvalid
        TFTKeyInvalid
        GameVersionTooOld
        GameVersionInvalid
        HashKeyFail
        KeyNotAccepted
    End Enum

    Private Const ProjectLainVersion As String = "LainBNETCore"
    Private Const ChatQueueLimit As Integer = 20

    Private bnet As clsProtocolBNET
    Private sockBNET As clsSocketTCPClient
    Private listBNET As ArrayList
    Private queuePacket As Queue
    Private queueChat As Queue
    Private WithEvents chatTimer As Timers.Timer
    Private cdKeyROC As String
    Private cdKeyTFT As String
    Private userName As String
    Private userPassword As String
    Private firstChannel As String
    Private warcraft3Path As String
    Private mbncsutil As clsMBNCSUtiliInterface
    Private isLoggedIn As Boolean
    Private isInChat As Boolean
    Private hostPort As Integer
    Private customData As clsBNETCustomData

    Public Event EventMessage(ByVal server As clsCommandPacket.PacketType, ByVal msg As String)
    Public Event EventError(ByVal errorFunction As String, ByVal errorString As String)
    Public Event EventIncomingChat(ByVal eventChat As clsIncomingChatChannel)
    Public Event EventIncomingGameHost(ByVal eventGameHost As clsIncomingGameHost)
    Public Event EventIncomingFriendList(ByVal eventFriendList() As clsIncomingFriendList)
    Public Event EventIncomingClanList(ByVal eventClanList() As clsIncomingClanList)
    Public Event EventEngingState()
    Public Event EventLogOnStatus(ByVal failpoint As LogOnFailPoints, ByVal info As String)
    Public Event EventBnetSIDSTARTADVEX3Result(ByVal isOK As Boolean)
    Public Event EventBnetSIDPING()



    Public Sub New()
        isLoggedIn = False
        isInChat = False

        userName = ""
        userPassword = ""
        warcraft3Path = ""
        cdKeyROC = "FFFFFFFFFFFFFFFFFFFFFFFFFF"
        cdKeyTFT = "FFFFFFFFFFFFFFFFFFFFFFFFFF"
        firstChannel = "The Void"
        bnet = New clsProtocolBNET
        mbncsutil = New clsMBNCSUtiliInterface("username", "password")
        hostPort = 0
        customData = New clsBNETCustomData

        sockBNET = New clsSocketTCPClient
        listBNET = ArrayList.Synchronized(New ArrayList)

        queuePacket = Queue.Synchronized(New Queue)
        queueChat = Queue.Synchronized(New Queue)
        chatTimer = New Timers.Timer
        chatTimer.Interval = 500
        warcraft3Path = ""
    End Sub


#Region "sock"


    Private Sub sockBNET_OnEventError(ByVal errorFunction As String, ByVal errorString As String, ByVal client As clsSocketTCP)
        RaiseEvent EventError(errorFunction, errorString)
        RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "CONNECTION to Battle.Net ERROR : " & errorString)
        [Stop]()
    End Sub
    Private Sub sockBNET_OnEventMessage(ByVal socketEvent As clsSocketTCP.SocketEvent, ByVal data As Object, ByVal socket As clsSocketTCP)
        Dim dataQ As Queue
        Dim mutex As Mutex
        Try
            Select Case socketEvent
                Case clsSocketTCP.SocketEvent.ConnectionClosed
                    RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "CONNECTION to Battle.Net is CLOSED")
                    [Stop]()
                Case clsSocketTCP.SocketEvent.ConnectionFailed
                    RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "FAILED to CONNECT to Battle.Net")
                    [Stop]()
                Case clsSocketTCP.SocketEvent.ConnectionEstablished
                    RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "SUCCESFULLY CONNECTED to Battle.Net")
                    TrySend(sockBNET, bnet.SEND_PROTOCOL_INITIALIZE_SELECTOR, clsCommandPacket.PacketType.BNET, "PROTOCOL_INITIALIZE_SELECTOR")
                    TrySend(sockBNET, bnet.SEND_SID_AUTH_INFO, clsCommandPacket.PacketType.BNET, "SID_AUTH_INFO")
                Case clsSocketTCP.SocketEvent.DataArrival
                    mutex = New Mutex(False, String.Format("mutex-DataArrival-{0}", Me.GetHashCode)) 'differnt class instance process will run parrallel, same class instance will wait for mutex
                    mutex.WaitOne()
                    dataQ = sockBNET.GetReceiveQueue
                    Do Until dataQ.Count = 0
                        listBNET.Add(dataQ.Dequeue())
                    Loop
                    PackageBNETPacket()
                    ProcessAllPacket()
                    mutex.ReleaseMutex()
            End Select
        Catch ex As Exception
            RaiseEvent EventError("sockBNET_OnEventMessage", ex.ToString)
        End Try
    End Sub
#End Region

    Private Sub PackageBNETPacket()
        Dim length As Integer
        Dim listPacket As ArrayList
        Dim i As Integer
        Dim data As Byte()
        Try
            While listBNET.Count >= 4
                If CByte(listBNET.Item(0)) = 255 Then
                    length = CType(clsHelper.ByteArrayToLong(New Byte() {CByte(listBNET.Item(2)), CByte(listBNET.Item(3))}), Integer)
                    If listBNET.Count >= length Then
                        listPacket = New ArrayList
                        For i = 1 To length
                            listPacket.Add(listBNET.Item(0))
                            listBNET.RemoveAt(0)
                        Next
                        data = CType(listPacket.ToArray(GetType(Byte)), Byte())
                        queuePacket.Enqueue(New clsCommandPacket(clsCommandPacket.PacketType.BNET, data(1), data, sockBNET))
                    Else
                        Exit While
                    End If
                Else
                    RaiseEvent EventError("PackageBNETPacket", "Packet ID is not 255")
                    Exit While
                End If
            End While
        Catch ex As Exception
            RaiseEvent EventError("PackageBNETPacket", ex.ToString)
        End Try

    End Sub
    Private Sub ProcessAllPacket()
        Dim command As clsCommandPacket
        Dim success As Boolean

        Try
            While queuePacket.Count > 0
                command = CType(queuePacket.Dequeue, clsCommandPacket)
                If command.GetPacketCommandType = clsCommandPacket.PacketType.BNET Then
                    Select Case command.GetPacketID
                        Case clsProtocolBNET.Protocol.SID_NULL      'every 2 min
                            If bnet.RECEIVE_SID_NULL(command.GetPacketData) Then
                                TrySend(sockBNET, bnet.SEND_SID_NULL)
                            End If
                        Case clsProtocolBNET.Protocol.SID_PING  'every 30 seconds
                            TrySend(sockBNET, bnet.SEND_SID_PING(bnet.RECEIVE_SID_PING(command.GetPacketData)))
                            RaiseEvent EventBnetSIDPING()
                        Case clsProtocolBNET.Protocol.SID_ENTERCHAT
                            If TryReceive(bnet.RECEIVE_SID_ENTERCHAT(command.GetPacketData), clsCommandPacket.PacketType.BNET, "SID_ENTERCHAT") = True Then
                                TrySend(sockBNET, bnet.SEND_SID_JOINCHANNEL(firstChannel), clsCommandPacket.PacketType.BNET, "SID_JOINCHANNEL")

                                Threading.Thread.Sleep(1000)    'make sure we are in channel
                                isInChat = True
                                RaiseEvent EventLogOnStatus(LogOnFailPoints.AllPass, String.Format(" -=[{0}]=-", userName))
                            End If
                        Case clsProtocolBNET.Protocol.SID_AUTH_ACCOUNTLOGONPROOF
                            If TryReceive(bnet.RECEIVE_SID_AUTH_ACCOUNTLOGONPROOF(command.GetPacketData), clsCommandPacket.PacketType.BNET, "SID_AUTH_ACCOUNTLOGONPROOF") = True Then
                                RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "Account Log On Successful : " & userName)
                                isLoggedIn = True
                                TrySend(sockBNET, bnet.SEND_SID_NETGAMEPORT(hostPort), clsCommandPacket.PacketType.BNET, "SID_NETGAMEPORT")
                                TrySend(sockBNET, bnet.SEND_SID_ENTERCHAT(), clsCommandPacket.PacketType.BNET, "SID_ENTERCHAT")
                            Else
                                RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "Account Log On Failed : Invalid Password")
                                RaiseEvent EventLogOnStatus(LogOnFailPoints.NamePasswordInvalid, "Account Log On Failed : Invalid Password")
                            End If
                        Case clsProtocolBNET.Protocol.SID_AUTH_ACCOUNTLOGON
                            If TryReceive(bnet.RECEIVE_SID_AUTH_ACCOUNTLOGON(command.GetPacketData), clsCommandPacket.PacketType.BNET, "SID_AUTH_ACCOUNTLOGON") = True Then
                                If customData.GetPasswordHashType = "pvpgn" Then
                                    mbncsutil.HELP_PVPGNPasswordHash(userPassword)
                                    TrySend(sockBNET, bnet.SEND_SID_AUTH_ACCOUNTLOGONPROOF(mbncsutil.GetPvPGNPasswordHash()), clsCommandPacket.PacketType.BNET, "SID_AUTH_ACCOUNTLOGONPROOF")
                                Else
                                    mbncsutil.HELP_SID_AUTH_ACCOUNTLOGONPROOF(bnet.GetSalt, bnet.GetServerPublicKey)
                                    TrySend(sockBNET, bnet.SEND_SID_AUTH_ACCOUNTLOGONPROOF(mbncsutil.GetM1()), clsCommandPacket.PacketType.BNET, "SID_AUTH_ACCOUNTLOGONPROOF")
                                End If
                            Else
                                RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "Account Log On Failed : Invalid User Name")
                                RaiseEvent EventLogOnStatus(LogOnFailPoints.NamePasswordInvalid, "Account Log On Failed : Invalid User Name")
                            End If
                        Case clsProtocolBNET.Protocol.SID_AUTH_CHECK
                            If TryReceive(bnet.RECEIVE_SID_AUTH_CHECK(command.GetPacketData), clsCommandPacket.PacketType.BNET, "SID_AUTH_CHECK") = True Then
                                RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "CD KEYS ACCPTED")

                                mbncsutil.HELP_SID_AUTH_ACCOUNTLOGON()
                                TrySend(sockBNET, bnet.SEND_SID_AUTH_ACCOUNTLOGON(mbncsutil.GetClientKey, userName), clsCommandPacket.PacketType.BNET, "SID_AUTH_ACCOUNTLOGON")
                            Else
                                Select Case clsHelper.ByteArrayToLong(bnet.GetKeyState)
                                    Case clsProtocolBNET.KeyResult.ROC_KEY_IN_USE
                                        RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "CD KEYS ROC In Use By : " & clsHelper.ByteArrayToStringASCII(bnet.GetKeyStateDescription))
                                        RaiseEvent EventLogOnStatus(LogOnFailPoints.ROCKeyInvalid, "CD KEYS ROC In Use By : " & clsHelper.ByteArrayToStringASCII(bnet.GetKeyStateDescription))
                                    Case clsProtocolBNET.KeyResult.TFT_KEY_IN_USE
                                        RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "CD KEYS TFT In Use By : " & clsHelper.ByteArrayToStringASCII(bnet.GetKeyStateDescription))
                                        RaiseEvent EventLogOnStatus(LogOnFailPoints.TFTKeyInvalid, "CD KEYS TFT In Use By : " & clsHelper.ByteArrayToStringASCII(bnet.GetKeyStateDescription))
                                    Case clsProtocolBNET.KeyResult.OLD_GAME_VERSION
                                        RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "Game Version Is too Old : " & clsHelper.ByteArrayToStringASCII(bnet.GetKeyStateDescription))
                                        RaiseEvent EventLogOnStatus(LogOnFailPoints.GameVersionTooOld, "Game Version Is too Old : " & clsHelper.ByteArrayToStringASCII(bnet.GetKeyStateDescription))
                                    Case clsProtocolBNET.KeyResult.INVALID_VERSION
                                        RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "Game Version Is Invalid")
                                        RaiseEvent EventLogOnStatus(LogOnFailPoints.GameVersionInvalid, "Game Version Is Invalid")
                                    Case Else
                                        RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "CD KEYS Not Accepted")
                                        RaiseEvent EventLogOnStatus(LogOnFailPoints.KeyNotAccepted, "CD KEYS Not Accepted")
                                End Select
                            End If
                        Case clsProtocolBNET.Protocol.SID_AUTH_INFO
                            If TryReceive(bnet.RECEIVE_SID_AUTH_INFO(command.GetPacketData), clsCommandPacket.PacketType.BNET, "SID_AUTH_INFO") = True Then
                                If mbncsutil.HELP_SID_AUTH_CHECK(warcraft3Path, cdKeyROC, cdKeyTFT, bnet.GetValueStringFormulaString, bnet.GetIX86verFileNameString, bnet.GetClientToken, bnet.GetServerToken) = True Then

                                    If customData.GetExeVersion.Length = 4 Then
                                        mbncsutil.SetExeVersion(customData.GetExeVersion)
                                    End If
                                    If customData.GetExeVersionHash.Length = 4 Then
                                        mbncsutil.SetExeVersionHash(customData.GetExeVersionHash)
                                    End If

                                    TrySend(sockBNET, bnet.SEND_SID_AUTH_CHECK(bnet.GetClientToken(), mbncsutil.GetExeVersion(), mbncsutil.GetExeVersionHash(), mbncsutil.GetKeyInfoROC(), mbncsutil.GetKeyInfoTFT(), mbncsutil.GetExeInfo(), ProjectLainVersion), clsCommandPacket.PacketType.BNET, "SID_AUTH_CHECK")
                                Else
                                    RaiseEvent EventLogOnStatus(LogOnFailPoints.HashKeyFail, "MBNCSUtil Key Hash Failed, Check your Warcraft3 Folder Path and KEYs")
                                    RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "MBNCSUtil ERROR - " & "Help_SID_AUTH_CHECK")
                                    [Stop]()
                                End If
                            End If
                        Case clsProtocolBNET.Protocol.SID_CHATEVENT
                            RaiseEvent EventIncomingChat(bnet.RECEIVE_SID_CHATEVENT(command.GetPacketData))
                        Case clsProtocolBNET.Protocol.SID_STARTADVEX3
                            success = bnet.RECEIVE_SID_STARTADVEX3(command.GetPacketData)
                            If success Then
                                'out of chat
                            Else
                                isInChat = True
                            End If
                            RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "receive -- " & "SID_STARTADVEX3")
                            RaiseEvent EventBnetSIDSTARTADVEX3Result(success)
                        Case clsProtocolBNET.Protocol.SID_GETADVLISTEX
                            RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "receive -- " & "SID_GETADVLISTEX")
                            RaiseEvent EventIncomingGameHost(bnet.RECEIVE_SID_GETADVLISTEX(command.GetPacketData))
                        Case clsProtocolBNET.Protocol.SID_FRIENDSUPDATE
                            RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "receive -- " & "SID_FRIENDSUPDATE")
                        Case clsProtocolBNET.Protocol.SID_FRIENDSLIST
                            RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "receive -- " & "SID_FRIENDSLIST")
                            RaiseEvent EventIncomingFriendList(bnet.RECEIVE_SID_FRIENDSLIST(command.GetPacketData))
                        Case clsProtocolBNET.Protocol.SID_CLANMEMBERLIST
                            RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "receive -- " & "SID_CLANMEMBERLIST")
                            RaiseEvent EventIncomingClanList(bnet.RECEIVE_SID_CLANMEMBERLIST(command.GetPacketData))
                        Case clsProtocolBNET.Protocol.SID_CLANMEMBERSTATUSCHANGE
                            RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "receive -- " & "SID_CLANMEMBERSTATUSCHANGE")
                        Case clsProtocolBNET.Protocol.SID_CHECKAD
                            TryReceive(bnet.RECEIVE_SID_CHECKAD(command.GetPacketData), clsCommandPacket.PacketType.BNET, "SID_CHECKAD")
                        Case Else
                            Debug.WriteLine("unprocessed BNET packet ID = " & command.GetPacketID)
                    End Select
                End If
            End While
        Catch ex As Exception
            RaiseEvent EventError("ProcessAllPacket", ex.ToString)
        End Try

    End Sub


    Private Function TrySend(ByVal sock As clsSocketTCPClient, ByVal buffer As Byte()) As Boolean
        If buffer.Length > 0 AndAlso sock.IsConnected = True Then
            Return sock.Send(buffer)
        Else
            Return False
        End If
    End Function
    Private Function TrySend(ByVal sock As clsSocketTCPClient, ByVal buffer As Byte(), ByVal server As clsCommandPacket.PacketType, ByVal protocol As String) As Boolean
        If buffer.Length > 0 AndAlso sock.IsConnected = True Then
            RaiseEvent EventMessage(server, "sending OK " & protocol)
            Return sock.Send(buffer)
        Else
            RaiseEvent EventMessage(server, "sending ERROR " & protocol)
            [Stop]()
            Return False
        End If
    End Function
    Private Function TryReceive(ByVal result As Boolean, ByVal server As clsCommandPacket.PacketType, ByVal protocol As String) As Boolean
        If result = True Then
            RaiseEvent EventMessage(server, "receive OK " & protocol)
            Return True
        Else
            RaiseEvent EventMessage(server, "receive ERROR " & protocol)
            [Stop]()
            Return False
        End If
    End Function


    Private Sub chatTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles chatTimer.Elapsed
        Dim chat As clsBNETChatMessage
        Dim delay As Integer
        Static isRunning As Boolean = False

        Try
            If isRunning = False Then
                isRunning = True

                If queueChat.Count > 0 Then
                    chat = CType(queueChat.Dequeue, clsBNETChatMessage)

                    If isInChat OrElse (isLoggedIn AndAlso chat.GetMsg.ToLower.StartsWith("/w")) Then
                        ChatMessage(chat.GetMsg)
                        delay = chat.GetMsg.Length * 60
                        delay = Math.Max(1500, delay)

                        Threading.Thread.Sleep(delay)
                    ElseIf chat.GetIsPersistent Then
                        queueChat.Enqueue(chat)
                    End If
                End If

                isRunning = False
            End If
        Catch ex As Exception
            RaiseEvent EventError("chatTimer_Elapsed", ex.ToString)
        End Try
    End Sub



    Private Function ChatMessage(ByVal msg As String) As Boolean
        Try
            If isLoggedIn Then
                If msg.Length > 220 Then
                    msg = msg.Substring(0, 220)
                End If
                If TrySend(sockBNET, bnet.SEND_SID_CHATCOMMAND(msg)) = True Then
                    RaiseEvent EventIncomingChat(New clsIncomingChatChannel(clsProtocolBNET.IncomingChatEvent.LOCAL_LOOP_BACK, 0, clsHelper.ByteArrayToStringASCII(bnet.GetUniqueName), msg))
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
            RaiseEvent EventError("Chat", ex.ToString)
        End Try
    End Function

    Public Function EnterChat() As Boolean
        Try
            Return TrySend(sockBNET, bnet.SEND_SID_ENTERCHAT(), clsCommandPacket.PacketType.BNET, "SID_ENTERCHAT")
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function GetHostPort() As Integer
        Return hostPort
    End Function

    Public Function IsSendChatAble() As Boolean
        Return isInChat And (queueChat.Count < ChatQueueLimit)
    End Function
    Public Function SendChatToQueue(ByVal chat As clsBNETChatMessage) As Boolean
        Try
            If queueChat.Count < ChatQueueLimit AndAlso chat.GetMsg.Trim.Length > 0 Then
                queueChat.Enqueue(chat)
                Return True
            End If
            Return False
        Catch ex As Exception
            RaiseEvent EventError("SendChatToQueue", ex.ToString)
            Return False
        End Try
    End Function

    Public Function GetChatQueue() As Queue
        Return queueChat
    End Function


    Public Function GetUniqueUserName() As String
        Return clsHelper.ByteArrayToStringASCII(bnet.GetUniqueName)
    End Function
    Public Function GetClanList() As Boolean
        Try
            If isInChat = True Then
                Return TrySend(sockBNET, bnet.SEND_SID_CLANMEMBERLIST(), clsCommandPacket.PacketType.BNET, "SID_CLANMEMBERLIST")
            End If
            Return False
        Catch ex As Exception
            Return False
            RaiseEvent EventError("GetFriendList", ex.ToString)
        End Try
    End Function
    Public Function GetFriendList() As Boolean
        Try
            If isInChat = True Then
                Return TrySend(sockBNET, bnet.SEND_SID_FRIENDSLIST(), clsCommandPacket.PacketType.BNET, "SID_FRIENDSLIST")
            End If
            Return False
        Catch ex As Exception
            Return False
            RaiseEvent EventError("GetFriendList", ex.ToString)
        End Try
    End Function

    Public Function GameUnCreate() As Boolean
        Try
            Return TrySend(sockBNET, bnet.SEND_SID_STOPADV, clsCommandPacket.PacketType.BNET, "SID_STOPADV")

        Catch ex As Exception
            Return False
            RaiseEvent EventError("GameUnCreate", ex.ToString)
        End Try
    End Function

    Public Function GameCreate(ByVal currentChannel As String, ByVal state As Byte, ByVal numPlayers As Integer, ByVal gameName As String, ByVal hostName As String, ByVal upTime As Integer, ByVal mapPath As String, ByVal mapCRC As Byte()) As Boolean
        Try
            isInChat = False
            If currentChannel.Length > 0 Then
                firstChannel = currentChannel
            End If

            Return TrySend(sockBNET, bnet.SEND_SID_STARTADVEX3(state, numPlayers, gameName, hostName, upTime, mapPath, mapCRC), clsCommandPacket.PacketType.BNET, "SID_STARTADVEX3")
        Catch ex As Exception
            Return False
            RaiseEvent EventError("GameCreate", ex.ToString)
        End Try
    End Function
    'MrJag|0.10|refresh|
    Public Function GameRefresh(ByVal state As Byte, ByVal numPlayers As Integer, ByVal gameName As String, ByVal hostName As String, ByVal numSlot As Integer, ByVal upTime As Long, ByVal mapPath As String, ByVal mapCRC As Byte()) As Boolean
        Try
            'Return TrySend(sockBNET, bnet.SEND_SID_STARTADVEX3(state, gameName, hostName, numSlot, upTime, mapPath, mapCRC), clsCommandPacket.PacketType.BNET, "SID_STARTADVEX3")
            Return TrySend(sockBNET, bnet.SEND_SID_STARTADVEX3(state, numPlayers, gameName, hostName, upTime, mapPath, mapCRC), clsCommandPacket.PacketType.BNET, "SID_STARTADVEX3")
        Catch ex As Exception
            Return False
            RaiseEvent EventError("GameRefresh", ex.ToString)
        End Try
    End Function

    Public Function GameJoin(ByVal gameName As String) As Boolean
        Try
            Return TrySend(sockBNET, bnet.SEND_SID_GETADVLISTEX(gameName), clsCommandPacket.PacketType.BNET, "SID_GETADVLISTEX")
        Catch ex As Exception
            Return False
            RaiseEvent EventError("GameJoin", ex.ToString)
        End Try
    End Function
    Public Function GoChannel(ByVal channelName As String) As Boolean
        Try
            If isInChat = True Then
                Return TrySend(sockBNET, bnet.SEND_SID_JOINCHANNEL(channelName), clsCommandPacket.PacketType.BNET, "SID_JOINCHANNEL")
            End If
            Return False
        Catch ex As Exception
            Return False
            RaiseEvent EventError("Channel", ex.ToString)
        End Try
    End Function

    Public Sub [Stop]()
        Try
            chatTimer.Stop()

            sockBNET.Stop()
            RemoveHandler sockBNET.eventMessage, AddressOf sockBNET_OnEventMessage
            RemoveHandler sockBNET.eventError, AddressOf sockBNET_OnEventError

            queuePacket.Clear()
            listBNET.Clear()
            isInChat = False
            isLoggedIn = False
            queueChat.Clear()

            RaiseEvent EventEngingState()
        Catch ex As Exception
            RaiseEvent EventError("[Stop]", ex.ToString)
        End Try
    End Sub
    Public Sub Start(ByVal bnetServer As String, ByVal war3path As String, ByVal keyROC As String, ByVal keyTFT As String, ByVal accountName As String, ByVal accountPassword As String, ByVal defaultChannel As String, ByVal gameHostPort As Integer, ByVal userCustomData As clsBNETCustomData)
        Try
            isInChat = False
            isLoggedIn = False

            firstChannel = defaultChannel
            cdKeyROC = keyROC.ToUpper
            cdKeyTFT = keyTFT.ToUpper

            bnet = New clsProtocolBNET
            mbncsutil = New clsMBNCSUtiliInterface(accountName, accountPassword)
            userName = accountName
            userPassword = accountPassword
            hostPort = gameHostPort
            customData = userCustomData
            warcraft3Path = war3path


            sockBNET = New clsSocketTCPClient
            AddHandler sockBNET.eventMessage, AddressOf sockBNET_OnEventMessage
            AddHandler sockBNET.eventError, AddressOf sockBNET_OnEventError

            chatTimer.Start()

            If sockBNET.Connect(clsSocketTCP.GetFirstIP(bnetServer), 6112) = False Then
                [Stop]()
            End If

            RaiseEvent EventEngingState()
            RaiseEvent EventMessage(clsCommandPacket.PacketType.BNET, "======================== Starting The Session ========================")
        Catch ex As Exception
            RaiseEvent EventError("Start", ex.ToString)
        End Try
    End Sub

    Public Function IsEngineRunning() As Boolean
        Return chatTimer.Enabled
    End Function



End Class

































'
