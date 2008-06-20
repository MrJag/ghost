Option Explicit On 
Option Strict On

Imports LainHelper
Imports LainSocket

#Region "accompany classes"
Public Class clsCommandPacket
    Public Enum PacketType
        BNET        '0xFF
        CustomGame  '0xF7
        LENP        '0x01
    End Enum

    Private packetCommandType As PacketType
    Private packetID As Integer
    Private packetData As Byte()
    Private packetSocket As clsSocketTCPClient

    Public Sub New(ByVal packetCommandType As PacketType, ByVal packetID As Integer, ByVal packetData As Byte(), ByVal packetSocket As clsSocketTCPClient)
        Me.packetCommandType = packetCommandType
        Me.packetID = packetID
        Me.packetData = packetData
        Me.packetSocket = packetSocket
    End Sub

    Public Function GetPacketCommandType() As PacketType
        Return packetCommandType
    End Function
    Public Function GetPacketID() As Integer
        Return packetID
    End Function
    Public Function GetPacketData() As Byte()
        Return packetData
    End Function
    Public Function GetPacketSocket() As clsSocketTCPClient
        Return packetSocket
    End Function

End Class
Public Class clsIncomingChatChannel
    Private chatevent As clsProtocolBNET.IncomingChatEvent
    Private ping As Integer
    Private user As String
    Private message As String

    Public Function GetChatEvent() As clsProtocolBNET.IncomingChatEvent
        Return chatevent
    End Function
    Public Function GetPing() As Integer
        Return ping
    End Function
    Public Function GetUser() As String
        Return user
    End Function
    Public Function GetMessage() As String
        Return message
    End Function

    Public Sub New(ByVal incomingEvent As clsProtocolBNET.IncomingChatEvent, ByVal incomingping As Integer, ByVal incominguser As String, ByVal incomingmsg As String)
        chatevent = incomingEvent
        ping = incomingping
        user = incominguser
        message = incomingmsg
    End Sub

    Public Sub New()
        chatevent = clsProtocolBNET.IncomingChatEvent.UNKNOWN
        ping = 0
        user = "UNKNOWN"
        message = "UNKNOWN"
    End Sub
End Class
Public Class clsIncomingGameHost
    Private IP As String
    Private port As Integer
    Private gameName As String
    Private counterTotalHosted As Byte()

    Public Sub New(ByVal hostIP As String, ByVal hostPort As Integer, ByVal hostGameName As String, ByVal hostCounter As Byte())
        IP = hostIP
        port = hostPort
        gameName = hostGameName
        counterTotalHosted = hostCounter
    End Sub
    Public Sub New()
        IP = "0.0.0.0"
        port = 0
        gameName = ""
        counterTotalHosted = New Byte() {}
    End Sub
    Public Function GetIP() As String
        Return IP
    End Function
    Public Function GetPort() As Integer
        Return port
    End Function
    Public Function GetGameName() As String
        Return gameName
    End Function
    Public Function GetHostCounter() As Byte()
        Return counterTotalHosted
    End Function

End Class
Public Class clsIncomingClanList
    Private name As String
    Private rank As Byte
    Private status As Byte

    Public Sub New()
        name = ""
        rank = 0
        status = 0
    End Sub
    Public Sub New(ByVal clanUserName As String, ByVal clanUserRank As Byte, ByVal clanUserStatus As Byte)
        name = clanUserName
        rank = clanUserRank
        status = clanUserStatus
    End Sub
    Public Function GetName() As String
        Return name
    End Function
    Public Function GetRank() As String
        Try
            Select Case rank
                Case Is = 0 : Return "N00b Recruit"
                Case Is = 1 : Return "Peon"
                Case Is = 2 : Return "Grunt"
                Case Is = 3 : Return "Shaman"
                Case Is = 4 : Return "Chieftan"
                Case Else : Return "Rank Unknown"
            End Select
        Catch ex As Exception
            Return "Rank Unknown"
        End Try
    End Function
    Public Function GetStatus() As String
        If status = 0 Then
            Return "offline"
        Else
            Return "online"
        End If
    End Function

    Public Function GetDescription() As String
        Dim builder As System.Text.StringBuilder
        Try
            builder = New System.Text.StringBuilder
            builder.Append(name)
            builder.Append(Environment.NewLine)
            builder.Append(GetStatus)
            builder.Append(Environment.NewLine)
            builder.Append(GetRank())
            builder.Append(Environment.NewLine)
            builder.Append(Environment.NewLine)
            Return builder.ToString
        Catch ex As Exception
            Return ""
        End Try
    End Function
End Class
Public Class clsIncomingFriendList
    Private account As String
    Private status As Byte
    Private area As Byte
    Private location As String

    Public Sub New()
        account = ""
        status = 0
        area = 0
        location = ""
    End Sub
    Private Function ExtractLocation(ByVal friendLocation As String) As String
        Dim builder As String
        builder = friendLocation
        If builder.StartsWith("PX3W") Then
            builder = builder.Substring(4)
        End If
        If builder.Length = 0 Then
            builder = "."
        End If
        Return builder
    End Function
    Private Function ExtractStatus(ByVal friendStatus As Byte) As String
        Dim builder As String
        Try
            builder = ""
            If (1 = (friendStatus And 1) >> 0) Then
                builder = builder & "<Mutual>"
            End If
            If (1 = (friendStatus And 2) >> 1) Then
                builder = builder & "<DND>"
            End If
            If (1 = (friendStatus And 4) >> 2) Then
                builder = builder & "<Away>"
            End If
            If builder.Length = 0 Then
                builder = "<None>"
            End If
            Return builder
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Private Function ExtractArea(ByVal friendArea As Byte) As String
        Try
            Select Case friendArea
                Case 0 : Return "<Offline>"
                Case 1 : Return "<No Channel>"
                Case 2 : Return "<In Channel>"
                Case 3 : Return "<Public Game>"
                Case 4 : Return "<Private Game>"
                Case 5 : Return "<Private Game>"
                Case Else : Return "<Unknown>"
            End Select
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Public Sub New(ByVal friendAccount As String, ByVal friendStatus As Byte, ByVal friendArea As Byte, ByVal friendLocation As String)
        account = friendAccount
        status = friendStatus
        area = friendArea
        location = friendLocation
    End Sub
    Public Function GetAccount() As String
        Return account
    End Function
    Public Function GetStatus() As String
        Return ExtractStatus(status)
    End Function
    Public Function GetArea() As String
        Return ExtractArea(area)
    End Function
    Public Function GetLocation() As String
        Return ExtractLocation(location)
    End Function
    Public Function GetDescription() As String
        Dim builder As System.Text.StringBuilder
        Try
            builder = New System.Text.StringBuilder
            builder.Append(account)
            builder.Append(Environment.NewLine)
            builder.Append(GetStatus)
            builder.Append(Environment.NewLine)
            builder.Append(GetArea)
            builder.Append(Environment.NewLine)
            builder.Append(GetLocation)
            builder.Append(Environment.NewLine)
            builder.Append(Environment.NewLine)
            Return builder.ToString
        Catch ex As Exception
            Return ""
        End Try
    End Function
End Class

#End Region

Public Class clsProtocolBNET

    Public Enum Protocol As Byte
        SID_NULL = 0                        '0x0
        SID_STOPADV = 2                     '0x2
        SID_GETADVLISTEX = 9                '0x9
        SID_ENTERCHAT = 10                  '0xA
        SID_JOINCHANNEL = 12                '0xC
        SID_CHATCOMMAND = 14                '0xE
        SID_CHATEVENT = 15                  '0xF
        SID_CHECKAD = 21                    '0x15
        SID_STARTADVEX3 = 28                '0x1c
        SID_DISPLAYAD = 33                  '0x21
        SID_NOTIFYJOIN = 34                 '0x22
        SID_PING = 37                       '0x25
        SID_LOGONRESPONSE = 41              '0x29
        SID_NETGAMEPORT = 69                '0x45
        SID_AUTH_INFO = 80                  '0x50
        SID_AUTH_CHECK = 81                 '0x51
        SID_AUTH_ACCOUNTLOGON = 83          '0x53
        SID_AUTH_ACCOUNTLOGONPROOF = 84     '0x54
        SID_FRIENDSLIST = 101               '0x65
        SID_FRIENDSUPDATE = 102             '0x66
        SID_CLANMEMBERLIST = 125            '0x7D
        SID_CLANMEMBERSTATUSCHANGE = 127    '0x7F
    End Enum
    Public Enum KeyResult As Integer
        GOOD = 0
        OLD_GAME_VERSION = 256
        INVALID_VERSION = 257
        ROC_KEY_IN_USE = 513
        TFT_KEY_IN_USE = 529
    End Enum
    Public Enum IncomingChatEvent As Integer
        EID_SHOWUSER = 1    'Received when you join a channel (users in channel and their information)
        EID_JOIN = 2        'Received when someone joins the channel you're currently in
        EID_LEAVE = 3       'Received when someone leaves the channel you're currently in
        EID_WHISPER = 4     'Receive whisper message
        EID_TALK = 5        'Received when someone talks in the channel you're currently in
        EID_BROADCAST = 6   'Server broadcast
        EID_CHANNEL = 7     'Received when you join a channel (the channel's name, flags)
        EID_USERFLAGS = 9   'User flags updates
        EID_WHISPERSENT = 10            'Sent whisper message
        EID_CHANNELFULL = 13            'Channel is full
        EID_CHANNELDOESNOTEXIST = 14    'Channel does not exist
        EID_CHANNELRESTRICTED = 15      'Channel is restricted
        EID_INFO = 18       'Broadcast/information message
        EID_ERROR = 19      'Error message
        EID_EMOTE = 23      'Emote
        UNKNOWN = -1
        LOCAL_LOOP_BACK = -2
    End Enum

    Private logontype As Byte()
    Private servertoken As Byte()
    Private clienttoken As Byte()
    Private MPQfiletime As Byte()
    Private IX86verfilename As Byte()
    Private valuestringformula As Byte()
    Private keystate As Byte()
    Private keystatedescription As Byte()
    Private serverpublickey As Byte()
    Private salt As Byte()
    Private uniquename As Byte()

    Public Sub New()
        clienttoken = New Byte() {220, 1, 203, 7}
        servertoken = New Byte() {}
        MPQfiletime = New Byte() {}
        IX86verfilename = New Byte() {}
        valuestringformula = New Byte() {}
        keystate = New Byte() {}
        keystatedescription = New Byte() {}
        serverpublickey = New Byte() {}
        salt = New Byte() {}
        logontype = New Byte() {}
        uniquename = New Byte() {}
    End Sub

#Region "game stat string"
    Public Function CreateStatString(ByVal numPlayers As Integer, ByVal hostName As String, ByVal mapPath As String, ByVal mapCRC As Byte()) As Byte()
        Dim data As ArrayList
        Try
            If mapPath.Length > 0 AndAlso hostName.Length > 0 AndAlso mapCRC.Length = 4 Then
                data = New ArrayList
                'MrJag|0.8c|observer| changing the 4th byte, 64, to 0 removes the custom game view from the map listing

                Debug.WriteLine(String.Format("numPlayers = {0}", numPlayers))
                If numPlayers = 10 Then
                    clsHelper.AddByteArray(data, New Byte() {2, 72, 6, 0, 0, 116, 0, 116, 0})
                Else
                    clsHelper.AddByteArray(data, New Byte() {2, 72, 6, 64, 0, 116, 0, 116, 0})
                End If

                clsHelper.AddByteArray(data, mapCRC)
                clsHelper.AddByteArray(data, mapPath) 'Maps\Download\DotA Allstars v6.48b.w3x
                clsHelper.AddByteArray(data, hostName)
                clsHelper.AddByteArray(data, New Byte() {0})
                Return EncodeStatString(CType(data.ToArray(GetType(Byte)), Byte()))
            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function EncodeStatString(ByVal data As Byte()) As Byte()
        Dim i As Integer
        Dim mask As Byte
        Dim list As ArrayList

        Try
            mask = 1
            list = New ArrayList
            For i = 0 To data.Length - 1
                If 0 = data(i) Mod 2 Then
                    list.Add(CType(data(i) + 1, Byte)) 'even
                Else
                    list.Add(CType(data(i), Byte))  'odd
                    mask = CType(mask Or (1 << (i Mod 7) + 1), Byte)
                End If

                If 6 = i Mod 7 OrElse i = data.Length - 1 Then
                    list.Insert(list.Count - 1 - (i Mod 7), mask)
                    mask = 1
                End If
            Next
            Return CType(list.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function DecodeStatString(ByVal code As Byte()) As Byte()
        Dim i As Integer
        Dim mask As Byte
        Dim list As ArrayList
        Try
            i = 0
            list = New ArrayList
            If code Is Nothing OrElse code.Length = 0 Then
                Return New Byte() {}
            End If

            While i < code.Length AndAlso code(i) <> 0
                If i Mod 8 = 0 Then
                    mask = code(i)
                Else
                    If 0 = (mask And (1 << (i Mod 8))) Then
                        list.Add(CType(code(i) - 1, Byte))
                    Else
                        list.Add(CType(code(i), Byte))
                    End If
                End If
                i = i + 1
            End While
            Return CType(list.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try

    End Function
#End Region



    Public Function GetUniqueName() As Byte()
        Return uniquename
    End Function
    Public Function GetLogOnType() As Byte()
        Return logontype
    End Function
    Public Function GetServerPublicKey() As Byte()
        Return serverpublickey
    End Function
    Public Function GetSalt() As Byte()
        Return salt
    End Function
    Public Function GetKeyState() As Byte()
        Return keystate
    End Function
    Public Function GetKeyStateDescription() As Byte()
        Return keystatedescription
    End Function
    Public Function GetMPQFileTime() As Byte()
        Return MPQfiletime
    End Function
    Public Function GetIX86verFileName() As Byte()
        Return IX86verfilename
    End Function
    Public Function GetIX86verFileNameString() As String
        Dim temp As Byte()
        Try
            temp = New Byte(IX86verfilename.Length - 1 - 1) {}
            Array.Copy(IX86verfilename, temp, temp.Length)
            Return New System.Text.ASCIIEncoding().GetString(temp)
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Public Function GetValueStringFormula() As Byte()
        Return valuestringformula
    End Function
    Public Function GetValueStringFormulaString() As String
        Dim temp As Byte()
        Try
            temp = New Byte(valuestringformula.Length - 1 - 1) {}
            Array.Copy(valuestringformula, temp, temp.Length)
            Return New System.Text.ASCIIEncoding().GetString(temp)
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Public Function GetServerToken() As Byte()
        Return servertoken
    End Function
    Public Function GetClientToken() As Byte()
        Return clienttoken
    End Function

    Public Function AssignLength(ByVal content As Byte()) As Boolean
        Dim length As Integer
        Dim lengthByte As Byte()
        Try
            If content Is Nothing = False AndAlso content.Length >= 4 AndAlso content.Length <= 65535 Then
                lengthByte = clsHelper.LongToDWORD(content.Length, False)
                length = CType(clsHelper.ByteArrayToLong(New Byte() {lengthByte(0), lengthByte(1)}), Integer)
                If length = content.Length Then
                    content(2) = lengthByte(0)
                    content(3) = lengthByte(1)
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
            If content Is Nothing = False AndAlso content.Length >= 4 AndAlso content.Length <= 65535 Then
                length = CType(clsHelper.ByteArrayToLong(New Byte() {content(2), content(3)}), Integer)
                If length = content.Length Then
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function



    Public Function RECEIVE_SID_PING(ByVal data As Byte()) As Byte()
        Dim pingvalue As Byte()
        Try
            If ValidateLength(data) = True Then
                pingvalue = New Byte(4 - 1) {}
                Array.Copy(data, 4, pingvalue, 0, 4)   'ping value
                Return pingvalue
            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try

    End Function
    Public Function RECEIVE_SID_NULL(ByVal data As Byte()) As Boolean
        Try
            Return ValidateLength(data)
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_SID_CHECKAD(ByVal data As Byte()) As Boolean
        Try
            Return ValidateLength(data)
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_SID_AUTH_INFO(ByVal data As Byte()) As Boolean
        Dim i As Integer
        Dim list As ArrayList
        Dim currentbyte As Byte
        Try
            If ValidateLength(data) = True Then
                logontype = New Byte(4 - 1) {}
                servertoken = New Byte(4 - 1) {}
                MPQfiletime = New Byte(8 - 1) {}
                Array.Copy(data, 4, logontype, 0, 4)    'Log On Type
                Array.Copy(data, 8, servertoken, 0, 4)  'Server Token
                Array.Copy(data, 16, MPQfiletime, 0, 8) 'MPQ FILETIME 

                i = 24
                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        IX86verfilename = CType(list.ToArray(GetType(Byte)), Byte())
                        Exit Do
                    End If
                Loop
                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        valuestringformula = CType(list.ToArray(GetType(Byte)), Byte())
                        Exit Do
                    End If
                Loop

                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_SID_AUTH_CHECK(ByVal data As Byte()) As Boolean
        Dim i As Integer
        Dim list As ArrayList
        Dim currentbyte As Byte
        Try
            If ValidateLength(data) = True Then
                i = 8
                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        keystatedescription = CType(list.ToArray(GetType(Byte)), Byte())
                        Exit Do
                    End If
                Loop

                keystate = New Byte(4 - 1) {}
                Array.Copy(data, 4, keystate, 0, 4)

                If clsHelper.ByteArrayToLong(keystate) = 0 Then
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function RECEIVE_SID_LOGONRESPONSE(ByVal data As Byte()) As Boolean
        Dim status As Byte()
        Try
            If ValidateLength(data) = True Then
                status = New Byte(4 - 1) {}
                Array.Copy(data, 4, status, 0, 4)
                If clsHelper.ByteArrayToLong(status) = 1 Then
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function RECEIVE_SID_AUTH_ACCOUNTLOGON(ByVal data As Byte()) As Boolean
        Dim status As Byte()
        Try
            If ValidateLength(data) = True Then
                status = New Byte(4 - 1) {}
                Array.Copy(data, 4, status, 0, 4)
                If clsHelper.ByteArrayToLong(status) = 0 Then
                    salt = New Byte(32 - 1) {}
                    serverpublickey = New Byte(32 - 1) {}

                    Array.Copy(data, 8, salt, 0, 32)
                    Array.Copy(data, 40, serverpublickey, 0, 32)
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try

    End Function
    Public Function RECEIVE_SID_AUTH_ACCOUNTLOGONPROOF(ByVal data As Byte()) As Boolean
        Dim status As Byte()
        Try
            If ValidateLength(data) = True Then
                status = New Byte(4 - 1) {}
                Array.Copy(data, 4, status, 0, 4)
                If clsHelper.ByteArrayToLong(status) = 0 Then
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function RECEIVE_SID_STARTADVEX3(ByVal data As Byte()) As Boolean
        Dim status As Byte()
        Try
            If ValidateLength(data) = True Then
                status = New Byte(4 - 1) {}
                Array.Copy(data, 4, status, 0, 4)
                If clsHelper.ByteArrayToLong(status) = 0 Then
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try

    End Function
    Public Function RECEIVE_SID_ENTERCHAT(ByVal data As Byte()) As Boolean
        Dim i As Integer
        Dim list As ArrayList
        Dim currentbyte As Byte
        Try
            If ValidateLength(data) = True Then
                i = 4
                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        uniquename = CType(list.ToArray(GetType(Byte)), Byte())
                        Exit Do
                    End If
                Loop

                If uniquename.Length > 0 Then
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_SID_CHATEVENT(ByVal data As Byte()) As clsIncomingChatChannel
        Dim i As Integer
        Dim list As ArrayList
        Dim currentbyte As Byte

        Dim eventid As Byte()
        Dim ping As Byte()
        Dim user As Byte()
        Dim msg As Byte()
        Try
            If ValidateLength(data) = True Then
                eventid = New Byte(4 - 1) {}
                Array.Copy(data, 4, eventid, 0, 4)

                ping = New Byte(4 - 1) {}
                Array.Copy(data, 12, ping, 0, 4)

                i = 28
                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        user = CType(list.ToArray(GetType(Byte)), Byte())
                        Exit Do
                    End If
                Loop
                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        msg = CType(list.ToArray(GetType(Byte)), Byte())
                        Exit Do
                    End If
                Loop

                Select Case CType(clsHelper.ByteArrayToLong(eventid), Integer)
                    Case clsProtocolBNET.IncomingChatEvent.EID_BROADCAST, _
                        clsProtocolBNET.IncomingChatEvent.EID_CHANNEL, _
                        clsProtocolBNET.IncomingChatEvent.EID_CHANNELDOESNOTEXIST, _
                        clsProtocolBNET.IncomingChatEvent.EID_CHANNELFULL, _
                        clsProtocolBNET.IncomingChatEvent.EID_CHANNELRESTRICTED, _
                        clsProtocolBNET.IncomingChatEvent.EID_EMOTE, _
                        clsProtocolBNET.IncomingChatEvent.EID_ERROR, _
                        clsProtocolBNET.IncomingChatEvent.EID_INFO, _
                        clsProtocolBNET.IncomingChatEvent.EID_JOIN, _
                        clsProtocolBNET.IncomingChatEvent.EID_LEAVE, _
                        clsProtocolBNET.IncomingChatEvent.EID_SHOWUSER, _
                        clsProtocolBNET.IncomingChatEvent.EID_TALK, _
                        clsProtocolBNET.IncomingChatEvent.EID_USERFLAGS, _
                        clsProtocolBNET.IncomingChatEvent.EID_WHISPER, _
                        clsProtocolBNET.IncomingChatEvent.EID_WHISPERSENT
                        Return New clsIncomingChatChannel(CType(CType(clsHelper.ByteArrayToLong(eventid), Integer), clsProtocolBNET.IncomingChatEvent), CType(clsHelper.ByteArrayToLong(ping), Integer), clsHelper.ByteArrayToStringASCII(user), clsHelper.ByteArrayToStringASCII(msg))
                    Case Else
                        Return New clsIncomingChatChannel
                End Select
            End If
            Return New clsIncomingChatChannel
        Catch ex As Exception
            Return New clsIncomingChatChannel
        End Try
    End Function


    Public Function RECEIVE_SID_FRIENDSLIST(ByVal data As Byte()) As clsIncomingFriendList()
        Dim total As Integer
        Dim account As Byte()
        Dim status As Byte
        Dim area As Byte
        Dim location As Byte()
        Dim i As Integer
        Dim list As ArrayList
        Dim currentbyte As Byte
        Dim friends As ArrayList
        Try
            If ValidateLength(data) = True Then
                total = data(4)
                If total > 0 Then
                    friends = New ArrayList
                    i = 5
                    Do Until total = 0
                        total = total - 1
                        list = New ArrayList
                        Do
                            currentbyte = data(i)
                            list.Add(currentbyte)
                            i = i + 1
                            If currentbyte = 0 Then
                                account = CType(list.ToArray(GetType(Byte)), Byte())
                                Exit Do
                            End If
                        Loop
                        status = data(i)
                        area = data(i + 1)
                        i = i + 2 + 4
                        list = New ArrayList
                        Do
                            currentbyte = data(i)
                            list.Add(currentbyte)
                            i = i + 1
                            If currentbyte = 0 Then
                                location = CType(list.ToArray(GetType(Byte)), Byte())
                                Exit Do
                            End If
                        Loop
                        friends.Add(New clsIncomingFriendList(clsHelper.ByteArrayToStringASCII(account), status, area, clsHelper.ByteArrayToStringASCII(location)))
                    Loop
                    Return CType(friends.ToArray(GetType(clsIncomingFriendList)), clsIncomingFriendList())
                End If
            End If
            Return New clsIncomingFriendList() {}
        Catch ex As Exception
            Return New clsIncomingFriendList() {}
        End Try
    End Function
    Public Function RECEIVE_SID_GETADVLISTEX(ByVal data As Byte()) As clsIncomingGameHost
        Dim gamesFound As Byte()
        Dim ip As Byte()
        Dim port As Byte()
        Dim gamename As Byte()
        Dim i As Integer
        Dim list As ArrayList
        Dim currentbyte As Byte
        Dim hostcounter As Byte()
        Dim hex As String

        Try
            If ValidateLength(data) = True Then
                gamesFound = New Byte(4 - 1) {}
                Array.Copy(data, 4, gamesFound, 0, 4)
                If clsHelper.ByteArrayToLong(gamesFound) > 0 Then
                    port = New Byte(2 - 1) {}
                    Array.Copy(data, 18, port, 0, 2)
                    ip = New Byte(4 - 1) {}
                    Array.Copy(data, 20, ip, 0, 4)

                    i = 40
                    list = New ArrayList
                    Do
                        currentbyte = data(i)
                        list.Add(currentbyte)
                        i = i + 1
                        If currentbyte = 0 Then
                            gamename = CType(list.ToArray(GetType(Byte)), Byte())
                            Exit Do
                        End If
                    Loop

                    i = i + 2
                    list = New ArrayList

                    hex = Convert.ToChar(data(i + 1)).ToString & Convert.ToChar(data(i + 0)).ToString
                    list.Add(Byte.Parse(hex, System.Globalization.NumberStyles.HexNumber))
                    hex = Convert.ToChar(data(i + 3)).ToString & Convert.ToChar(data(i + 2)).ToString
                    list.Add(Byte.Parse(hex, System.Globalization.NumberStyles.HexNumber))
                    hex = Convert.ToChar(data(i + 5)).ToString & Convert.ToChar(data(i + 4)).ToString
                    list.Add(Byte.Parse(hex, System.Globalization.NumberStyles.HexNumber))
                    hex = Convert.ToChar(data(i + 7)).ToString & Convert.ToChar(data(i + 6)).ToString
                    list.Add(Byte.Parse(hex, System.Globalization.NumberStyles.HexNumber))
                    hostcounter = CType(list.ToArray(GetType(Byte)), Byte())

                    Return New clsIncomingGameHost(String.Format("{0}.{1}.{2}.{3}", ip(0), ip(1), ip(2), ip(3)), CType(clsHelper.ByteArrayToLong(port, False), Integer), clsHelper.ByteArrayToStringASCII(gamename), hostcounter)
                End If
            End If
            Return New clsIncomingGameHost
        Catch ex As Exception
            Return New clsIncomingGameHost
        End Try
    End Function
    Public Function RECEIVE_SID_CLANMEMBERLIST(ByVal data As Byte()) As clsIncomingClanList()
        Dim total As Integer
        Dim clanlist As ArrayList
        Dim i As Integer
        Dim list As ArrayList
        Dim currentbyte As Byte
        Dim name As Byte()
        Dim rank As Byte
        Dim status As Byte
        Dim location As Byte()

        Try
            If ValidateLength(data) = True Then
                total = data(8)
                If total > 0 Then
                    clanlist = New ArrayList
                    i = 9
                    Do Until total = 0
                        total = total - 1
                        list = New ArrayList
                        Do
                            currentbyte = data(i)
                            list.Add(currentbyte)
                            i = i + 1
                            If currentbyte = 0 Then
                                name = CType(list.ToArray(GetType(Byte)), Byte())
                                Exit Do
                            End If
                        Loop
                        rank = data(i)
                        status = data(i + 1)
                        i = i + 2
                        list = New ArrayList
                        Do
                            currentbyte = data(i)
                            list.Add(currentbyte)
                            i = i + 1
                            If currentbyte = 0 Then
                                location = CType(list.ToArray(GetType(Byte)), Byte())
                                Exit Do
                            End If
                        Loop

                        clanlist.Add(New clsIncomingClanList(clsHelper.ByteArrayToStringASCII(name), rank, status))
                    Loop
                    Return CType(clanlist.ToArray(GetType(clsIncomingClanList)), clsIncomingClanList())
                End If
            End If
            Return New clsIncomingClanList() {}
        Catch ex As Exception
            Return New clsIncomingClanList() {}
        End Try
    End Function
    Public Function RECEIVE_SID_CLANMEMBERSTATUSCHANGE(ByVal data As Byte()) As clsIncomingClanList
        Dim i As Integer
        Dim list As ArrayList
        Dim currentbyte As Byte
        Dim name As Byte()
        Dim rank As Byte
        Dim status As Byte
        Dim location As Byte()
        Try
            If ValidateLength(data) = True Then
                i = 4
                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        name = CType(list.ToArray(GetType(Byte)), Byte())
                        Exit Do
                    End If
                Loop
                rank = data(i)
                status = data(i + 1)
                i = i + 2
                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        location = CType(list.ToArray(GetType(Byte)), Byte())
                        Exit Do
                    End If
                Loop
                Return New clsIncomingClanList(clsHelper.ByteArrayToStringASCII(name), rank, status)
            End If
            Return Nothing
        Catch ex As Exception
            Return Nothing
        End Try
    End Function


    Public Function SEND_PROTOCOL_INITIALIZE_SELECTOR() As Byte()
        Return New Byte() {1}
    End Function

    Public Function SEND_SID_STOPADV() As Byte()
        Dim packet As ArrayList
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {255})      'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {2})        'SID_STOPADV
            clsHelper.AddByteArray(packet, New Byte() {4, 0})     'packet length
            Return CType(packet.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function



    Public Function SEND_SID_PING(ByVal pingvalue As Byte()) As Byte()
        Dim packet As ArrayList
        Try
            If pingvalue Is Nothing = False AndAlso pingvalue.Length = 4 Then
                packet = New ArrayList
                clsHelper.AddByteArray(packet, New Byte() {255})      'BNET header constant
                clsHelper.AddByteArray(packet, New Byte() {37})       'SID_PING
                clsHelper.AddByteArray(packet, New Byte() {8, 0})     'packet length
                clsHelper.AddByteArray(packet, pingvalue)
                Return CType(packet.ToArray(GetType(Byte)), Byte())
            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try

    End Function
    Public Function SEND_SID_NULL() As Byte()
        Dim packet As ArrayList
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {255})      'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {0})        'SID_NULL
            clsHelper.AddByteArray(packet, New Byte() {4, 0})     'packet length
            Return CType(packet.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_SID_AUTH_INFO() As Byte()
        Dim packet As ArrayList
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {255})                  'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {80})                   'SID_AUTH_INFO
            clsHelper.AddByteArray(packet, New Byte() {54, 0})                'packet length
            clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})           'Protocol ID
            clsHelper.AddByteArray(packet, New Byte() {54, 56, 88, 73})       'Platform ID for IX86
            clsHelper.AddByteArray(packet, New Byte() {80, 88, 51, 87})       'Product ID for W3XP
            clsHelper.AddByteArray(packet, New Byte() {21, 0, 0, 0})          'Version currently 1.21, might change in future
            clsHelper.AddByteArray(packet, New Byte() {83, 85, 110, 101})     'Product language for enUS
            clsHelper.AddByteArray(packet, New Byte() {127, 0, 0, 1})         'Local IP for NAT compatibility
            clsHelper.AddByteArray(packet, New Byte() {108, 253, 255, 255})   'Time zone bias
            clsHelper.AddByteArray(packet, New Byte() {9, 12, 0, 0})          'Locale ID
            clsHelper.AddByteArray(packet, New Byte() {9, 4, 0, 0})           'Language ID
            clsHelper.AddByteArray(packet, "AUS")                             'Country abreviation for aus
            clsHelper.AddByteArray(packet, "Australia")                       'Country for Australia
            Return CType(packet.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_SID_AUTH_CHECK(ByVal clienttoken As Byte(), ByVal exeversion As Byte(), ByVal exeversionhash As Byte(), ByVal keyinfoROC As Byte(), ByVal keyinfoTFT As Byte(), ByVal exeinfo As String, ByVal keyOwnerName As String) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            If clienttoken.Length = 4 And keyinfoROC.Length = 36 And keyinfoTFT.Length = 36 And exeversion.Length = 4 And exeversionhash.Length = 4 And exeinfo.Length > 0 Then
                packet = New ArrayList
                clsHelper.AddByteArray(packet, New Byte() {255})            'BNET header constant
                clsHelper.AddByteArray(packet, New Byte() {81})             'SID_AUTH_CHECK
                clsHelper.AddByteArray(packet, New Byte() {0, 0})           'undfined packet length
                clsHelper.AddByteArray(packet, clienttoken)                 'Client Token
                clsHelper.AddByteArray(packet, exeversion)                  'EXE Version
                clsHelper.AddByteArray(packet, exeversionhash)              'EXE Hash
                clsHelper.AddByteArray(packet, New Byte() {2, 0, 0, 0})     'Number of keys in this packet
                clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})     'boolean Using Spawn (32-bit)

                clsHelper.AddByteArray(packet, keyinfoROC)                  'key info
                clsHelper.AddByteArray(packet, keyinfoTFT)                  'key info

                clsHelper.AddByteArray(packet, exeinfo)                     'Exe Information
                clsHelper.AddByteArray(packet, keyOwnerName)                'CD Key owner name 

                buffer = CType(packet.ToArray(GetType(Byte)), Byte())
                AssignLength(buffer)
                Return buffer
            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function

    Public Function SEND_SID_LOGONRESPONSE(ByVal clientToken As Byte(), ByVal serverToken As Byte(), ByVal passwordHash As Byte(), ByVal accountname As String) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            If clientToken.Length = 4 AndAlso serverToken.Length = 4 AndAlso passwordHash.Length = 20 AndAlso accountname.Length > 0 Then
                packet = New ArrayList
                clsHelper.AddByteArray(packet, New Byte() {255})        'BNET header constant
                clsHelper.AddByteArray(packet, New Byte() {41})         'SID_LOGONRESPONSE
                clsHelper.AddByteArray(packet, New Byte() {0, 0})       'undefined packet length
                clsHelper.AddByteArray(packet, clientToken)             'Client Token 
                clsHelper.AddByteArray(packet, serverToken)             'Server Token 
                clsHelper.AddByteArray(packet, passwordHash)            'Password Hash
                clsHelper.AddByteArray(packet, accountname)             'Username
                buffer = CType(packet.ToArray(GetType(Byte)), Byte())
                AssignLength(buffer)
                Return buffer
            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try

    End Function

    Public Function SEND_SID_AUTH_ACCOUNTLOGON(ByVal clientpublickey As Byte(), ByVal accountname As String) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            If clientpublickey Is Nothing = False AndAlso clientpublickey.Length = 32 Then
                packet = New ArrayList
                clsHelper.AddByteArray(packet, New Byte() {255})      'BNET header constant
                clsHelper.AddByteArray(packet, New Byte() {83})       'SID_AUTH_ACCOUNTLOGON
                clsHelper.AddByteArray(packet, New Byte() {0, 0})     'undefined packet length
                clsHelper.AddByteArray(packet, clientpublickey)       'Client Key ('A')
                clsHelper.AddByteArray(packet, accountname)           'Username
                buffer = CType(packet.ToArray(GetType(Byte)), Byte())
                AssignLength(buffer)
                Return buffer
            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try

    End Function
    Public Function SEND_SID_ENTERCHAT() As Byte()
        Dim packet As ArrayList

        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {255})      'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {10})       'SID_ENTERCHAT
            clsHelper.AddByteArray(packet, New Byte() {6, 0})     'packet length
            clsHelper.AddByteArray(packet, New Byte() {0})        'Username Null on Warcraft III/The Frozen Throne 
            clsHelper.AddByteArray(packet, New Byte() {0})        'Statstring Null on CdKey'd products
            Return CType(packet.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try

    End Function
    Public Function SEND_SID_AUTH_ACCOUNTLOGONPROOF(ByVal clientpasswordproof As Byte()) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            If clientpasswordproof Is Nothing = False AndAlso clientpasswordproof.Length = 20 Then
                packet = New ArrayList
                clsHelper.AddByteArray(packet, New Byte() {255})      'BNET header constant
                clsHelper.AddByteArray(packet, New Byte() {84})       'SID_AUTH_ACCOUNTLOGONPROOF
                clsHelper.AddByteArray(packet, New Byte() {0, 0})     'undefined packet length
                clsHelper.AddByteArray(packet, clientpasswordproof)   'clientpasswordproof 
                buffer = CType(packet.ToArray(GetType(Byte)), Byte())
                AssignLength(buffer)
                Return buffer
            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try

    End Function
    Public Function SEND_SID_NETGAMEPORT(ByVal serverPort As Integer) As Byte()
        Dim packet As ArrayList

        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {255})      'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {69})       'SID_NETGAMEPORT
            clsHelper.AddByteArray(packet, New Byte() {6, 0})     'packet length
            clsHelper.AddByteArray(packet, clsHelper.IntegerToWORD(serverPort, False))   'local game server Port 
            Return CType(packet.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try

    End Function
    Public Function SEND_SID_CHECKAD() As Byte()
        Dim packet As ArrayList
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {255})            'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {21})             'SID_CHECKAD
            clsHelper.AddByteArray(packet, New Byte() {20, 0})          'packet length
            clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})
            clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})
            clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})
            clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})
            Return CType(packet.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_SID_CLANMEMBERLIST() As Byte()
        Dim packet As ArrayList
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {255})          'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {125})          'SID_CLANMEMBERLIST
            clsHelper.AddByteArray(packet, New Byte() {8, 0})         'packet length
            clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})   'cookie
            Return CType(packet.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_SID_CHATCOMMAND(ByVal message As String) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {255})      'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {14})       'SID_CHATCOMMAND
            clsHelper.AddByteArray(packet, New Byte() {0, 0})     'undefined packet length
            clsHelper.AddByteArray(packet, message)  'chat string

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer
        Catch ex As Exception
            Return New Byte() {}
        End Try

    End Function
    Public Function SEND_SID_NOTIFYJOIN(ByVal gameName As String) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {255})              'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {34})               'SID_NOTIFYJOIN
            clsHelper.AddByteArray(packet, New Byte() {0, 0})             'undefined packet length
            clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})       'product Id
            clsHelper.AddByteArray(packet, New Byte() {14, 0, 0, 0})      'product version byte war3 = 14
            clsHelper.AddByteArray(packet, gameName)                      'Game name
            clsHelper.AddByteArray(packet, New Byte() {0})                'Game password
            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_SID_GETADVLISTEX(ByVal gameName As String) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {255})          'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {9})            'SID_GETADVLISTEX
            clsHelper.AddByteArray(packet, New Byte() {0, 0})         'undefined packet length
            clsHelper.AddByteArray(packet, New Byte() {255, 3, 0, 0}) 'map filter
            clsHelper.AddByteArray(packet, New Byte() {255, 3, 0, 0}) 'map filter
            clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})   'map filter
            clsHelper.AddByteArray(packet, New Byte() {1, 0, 0, 0})   'Maximum number of games to list
            clsHelper.AddByteArray(packet, gameName)                  'Game name
            clsHelper.AddByteArray(packet, New Byte() {0})            'Game password
            clsHelper.AddByteArray(packet, New Byte() {0})            'Game stats

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_SID_JOINCHANNEL(ByVal channel As String) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            If channel Is Nothing = False Then
                packet = New ArrayList
                clsHelper.AddByteArray(packet, New Byte() {255})          'BNET header constant
                clsHelper.AddByteArray(packet, New Byte() {12})           'SID_JOINCHANNEL
                clsHelper.AddByteArray(packet, New Byte() {0, 0})         'undefined packet length

                If channel.Length > 0 Then
                    clsHelper.AddByteArray(packet, New Byte() {2, 0, 0, 0})   'Flags for Nocreate Join:
                Else
                    clsHelper.AddByteArray(packet, New Byte() {1, 0, 0, 0})   'Flags for First Join:
                End If
                clsHelper.AddByteArray(packet, channel)                   'channel

                buffer = CType(packet.ToArray(GetType(Byte)), Byte())
                AssignLength(buffer)
                Return buffer
            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_SID_STARTADVEX3(ByVal state As Byte, ByVal numPlayers As Integer, ByVal gameName As String, ByVal hostName As String, ByVal upTime As Long, ByVal mapPath As String, ByVal mapCRC As Byte()) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            packet = New ArrayList

            clsHelper.AddByteArray(packet, New Byte() {255})      'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {28})       'SID_STARTADVEX3
            clsHelper.AddByteArray(packet, New Byte() {0, 0})     'packet length

            'state 16=public 17=private 18=close 
            clsHelper.AddByteArray(packet, New Byte() {state, 0, 0, 0})             'game state
            clsHelper.AddByteArray(packet, clsHelper.LongToDWORD(upTime, False))       'Time since creation

            clsHelper.AddByteArray(packet, New Byte() {1, 32, 73, 0})       'Game Type, Parameter 
            'clsHelper.AddByteArray(packet, New Byte() {1, 32, 25, 0})       'Game Type, Parameter

            clsHelper.AddByteArray(packet, New Byte() {255, 3, 0, 0})       'Unknown 
            clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})         'custom game
            clsHelper.AddByteArray(packet, gameName)                        'game name
            clsHelper.AddByteArray(packet, New Byte() {0})                  'game password

            'this is hardcoded for now, eventually replace with new numPlayers variable
            'keep this hard coded to allow a maximum number of hidden players.
            'clsHelper.AddByteArray(packet, New Byte() {57})                 'asc(57)=9, 9(hex) = 9(dec) = 9 slots free 
            clsHelper.AddByteArray(packet, New Byte() {98})                 '98=b=11 slots free
            'clsHelper.AddByteArray(packet, New Byte() {102})                 '98=b=15 slots free
            'clsHelper.AddByteArray(packet, New Byte() {99})                 '98=b=15 slots free

            clsHelper.AddByteArray(packet, New Byte() {49, 48, 48, 48, 48, 48, 48, 48})     '1,0,0,0,0,0,0,0=host counter

            Debug.WriteLine(String.Format("Sending numPlayers = {0}", numPlayers))
            clsHelper.AddByteArray(packet, CreateStatString(numPlayers, hostName, mapPath, mapCRC))     'game stat
            clsHelper.AddByteArray(packet, New Byte() {0})                                  'game stat end

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_SID_FRIENDSLIST() As Byte()
        Dim packet As ArrayList
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {255})      'BNET header constant
            clsHelper.AddByteArray(packet, New Byte() {101})      'SID_FRIENDSLIST
            clsHelper.AddByteArray(packet, New Byte() {4, 0})     'packet length
            Return CType(packet.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function




End Class







































'