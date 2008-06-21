Option Explicit On 
Option Strict On

Imports System.Net
Imports System.IO
Imports System.Net.NetworkInformation
Imports System.Threading
Imports LainBnetCore
Imports LainSocket
Imports LainHelper

#Region "map"
Public Class clsGameHostMap
    Private mapPath As String
    Private mapSize As Byte()
    Private mapInfo As Byte()
    Private mapCRC As Byte()

    Public Sub New()
        Me.mapPath = ""
        Me.mapSize = New Byte() {}
        Me.mapInfo = New Byte() {}
        Me.mapCRC = New Byte() {}
    End Sub
    Public Sub New(ByVal mapPath As String, ByVal mapSize As Byte(), ByVal mapInfo As Byte(), ByVal mapCRC As Byte())
        Me.mapPath = mapPath
        Me.mapSize = mapSize
        Me.mapInfo = mapInfo
        Me.mapCRC = mapCRC
    End Sub

    Public Function GetMapPath() As String
        Return mapPath
    End Function
    Public Function GetMapSize() As Byte()
        Return mapSize
    End Function
    Public Function GetMapInfo() As Byte()
        Return mapInfo
    End Function
    Public Function GetMapCRC() As Byte()
        Return mapCRC
    End Function

End Class
#End Region

Public Class clsGameHost

    Private Const GAME_PRIVATE As Integer = 17  'MrJag|0.8c|gamestate| needed to test game states
    Private Const GAME_PUBLIC As Integer = 16

    Private Const TEAM_SENTINEL As Byte = 0
    Private Const TEAM_SCOURGE As Byte = 1
    Private Const TIME_GAME_EXPIRE_COUNTER As Integer = 60 * 2
    Private sockServer As clsSocketTCPServer

    Private data As clsData

    Private hashClient As Hashtable     'Socket -> Arraylist
    Private queuePacket As Queue
    Private listAction As ArrayList
    Private isGameLoaded As Boolean
    Private countdownCounter As Integer
    Private isCountDownStarted As Boolean
    Private totalFinishLoad As Integer
    Private teamWon As Byte
    Private timeLastLoad As Date

    Private loadAwards As System.Text.StringBuilder
    Private creationTime As Long                'MrJag|0.10|refresh| time game was created
    Private enableRefresh As Boolean            'MrJag|0.10|refresh| toggle for refresh
    Private startLock As Byte

    Private gameState As Byte
    Private numPlayers As Integer
    Private hostName As String
    Private callerName As String
    Private gameName As String
    Private gamePort As Integer
    Private mapPath As String
    Private mapSize As Byte()
    Private mapInfo As Byte()
    Private mapCRC As Byte()
    Private bnet As clsBNET
    Private timeGameExpire As Integer
    Private spoofID As Integer
    Private spoofSafe As ArrayList
    Private listSentinelPlayer As ArrayList
    Private listScourgePlayer As ArrayList
    Private listReferee As ArrayList
    Private reserveList As ArrayList

    Private WithEvents protocol As clsProtocolHost
    Private WithEvents actionTimer As Timers.Timer
    Private WithEvents lobbyTimer As Timers.Timer
    Private WithEvents engineTimer As Timers.Timer
    Private WithEvents endTimer As Timers.Timer
    Private WithEvents refreshTimer As Timers.Timer 'MrJag|0.10|refresh|

    Private WithEvents botLobby As clsBotCommandHostLobby
    Private WithEvents botGame As clsBotCommandHostGame

    Public Event EventHostUncreate()
    Public Event EventHostDisposed(ByVal host As clsGameHost, ByVal reason As String)
    Public Event EventGameWon(ByVal callerName As String, ByVal gameName As String, ByVal sentinelPlayer() As String, ByVal scourgePlayer() As String, ByVal referee() As String, ByVal winner As String)

    Public Sub New()
        Me.data = New clsData
        Me.gameState = 0
        Me.numPlayers = 0
        Me.hostName = ""
        Me.callerName = ""
        Me.gameName = ""
        Me.gamePort = 0
        Me.mapPath = ""
        Me.mapSize = New Byte() {}
        Me.mapInfo = New Byte() {}
        Me.mapCRC = New Byte() {}
        Me.bnet = New clsBNET
        Me.timeGameExpire = 0
        Me.isGameLoaded = False
        Me.countdownCounter = -1
        Me.isCountDownStarted = False
        Me.totalFinishLoad = 0
        Me.teamWon = 0
        Me.timeLastLoad = Now

        Me.reserveList = New ArrayList

        Me.loadAwards = New System.Text.StringBuilder
        Me.creationTime = Environment.TickCount     'MrJag|0.10|refresh| initalize creation time
        Me.enableRefresh = True                     'MrJag|0.10|refresh| toggle for refresh
        Me.startLock = 255                          '255 = not locked, anything else = PID of player who locked it

        sockServer = New clsSocketTCPServer
        hashClient = New Hashtable
        queuePacket = New Queue
        listAction = New ArrayList
        protocol = New clsProtocolHost(hostName, gameName, numPlayers, callerName)
        botLobby = New clsBotCommandHostLobby("", New String() {})
        botGame = New clsBotCommandHostGame("", New String() {})
        spoofID = 0
        spoofSafe = New ArrayList
        listSentinelPlayer = New ArrayList
        listScourgePlayer = New ArrayList
        listReferee = New ArrayList
        engineTimer = New Timers.Timer
        lobbyTimer = New Timers.Timer
        actionTimer = New Timers.Timer
        endTimer = New Timers.Timer
        refreshTimer = New Timers.Timer             'MrJag|0.8c|refresh|
    End Sub
    Public Sub New(ByVal adminName As String(), ByVal gameState As Byte, ByVal numPlayers As Integer, ByVal gameName As String, ByVal hostName As String, ByVal callerName As String, ByVal gamePort As Integer, ByVal mapPath As String, ByVal mapSize As Byte(), ByVal mapInfo As Byte(), ByVal mapCRC As Byte(), ByVal bnet As clsBNET, ByVal data As clsData)

        Me.data = data
        Me.gameState = gameState
        Me.numPlayers = numPlayers
        Me.hostName = hostName
        Me.callerName = callerName
        Me.gameName = gameName
        Me.gamePort = gamePort
        Me.mapPath = mapPath
        Me.mapSize = mapSize
        Me.mapInfo = mapInfo
        Me.mapCRC = mapCRC
        Me.bnet = bnet
        Me.timeGameExpire = 0
        Me.isGameLoaded = False
        Me.countdownCounter = -1
        Me.isCountDownStarted = False
        Me.totalFinishLoad = 0
        Me.teamWon = 255
        Me.timeLastLoad = Now

        Me.reserveList = New ArrayList

        Me.loadAwards = New System.Text.StringBuilder
        Me.creationTime = Environment.TickCount     'MrJag|0.10|refresh| initalize creation time
        Me.enableRefresh = True                     'MrJag|0.10|refresh| toggle for refresh
        Me.startLock = 255                          '255 = not locked, anything else = PID of player who locked it.

        sockServer = New clsSocketTCPServer
        hashClient = Hashtable.Synchronized(New Hashtable)
        queuePacket = Queue.Synchronized(New Queue)
        listAction = ArrayList.Synchronized(New ArrayList)
        protocol = New clsProtocolHost(hostName, gameName, numPlayers, callerName)
        botLobby = New clsBotCommandHostLobby(callerName, adminName)
        botGame = New clsBotCommandHostGame(callerName, adminName)

        spoofID = New Random(Environment.TickCount).Next(100, 1000)
        spoofSafe = ArrayList.Synchronized(New ArrayList)
        spoofSafe.Add(hostName)

        listSentinelPlayer = ArrayList.Synchronized(New ArrayList)
        listScourgePlayer = ArrayList.Synchronized(New ArrayList)
        listReferee = ArrayList.Synchronized(New ArrayList)

        engineTimer = New Timers.Timer
        engineTimer.Interval = 1000

        lobbyTimer = New Timers.Timer
        lobbyTimer.Interval = 500

        actionTimer = New Timers.Timer
        actionTimer.Interval = 100

        endTimer = New Timers.Timer
        endTimer.Interval = 10 * 1000

        refreshTimer = New Timers.Timer                             'MrJag|0.8c|refresh|
        refreshTimer.Interval = 15 * 1000                           'MrJag|0.8c|refresh|every 15 seconds

        AddHandler sockServer.eventMessage, AddressOf sockServer_OnEventMessage
        AddHandler sockServer.eventError, AddressOf sockServer_OnEventError
        AddHandler bnet.EventIncomingChat, AddressOf bnet_EventIncomingChat
        AddHandler bnet.EventBnetSIDSTARTADVEX3Result, AddressOf bnet_EventBnetSIDSTARTADVEX3Result
        AddHandler bnet.EventIncomingFriendList, AddressOf EventMessage_IncomingFriendList
        AddHandler bnet.EventIncomingClanList, AddressOf EventMessage_IncomingClanList
        AddHandler botLobby.EventBotSay, AddressOf EventMessage_SendChat
        'AddHandler botLobby.EventDataSetAccess, AddressOf EventData_SetAccess

    End Sub
    Public Function GetCallerName() As String
        Return callerName
    End Function
    Public Function GetGameName() As String
        Return gameName
    End Function
    Public Function GetHostName() As String
        Return hostName
    End Function
    Public Function GetIsCountDownStarted() As Boolean
        Return isCountDownStarted
    End Function
    Public Function GetIsGameLoaded() As Boolean
        Return isGameLoaded
    End Function
    Public Function GetTotalPlayers() As Integer
        Return protocol.GetPlayerCount(protocol.GetHostPID)
    End Function
    Public Function getUptime() As Long
        Dim retVal As Long
        retVal = CLng(Math.Round((Environment.TickCount - creationTime) / 1000))
        'Debug.WriteLine(String.Format("uptime = {0}, now = {1}, created = {2}", retVal, Environment.TickCount, creationTime))
        Return retVal
    End Function

#Region "sock event"
    Private Sub sockServer_OnEventMessage(ByVal socketEvent As clsSocketTCP.SocketEvent, ByVal data As Object, ByVal socket As clsSocketTCP)
        Dim client As clsSocketTCPClient
        Try
            Select Case socketEvent
                Case clsSocketTCP.SocketEvent.ConnectionAccepted
                    client = CType(data, clsSocketTCPClient)
                    AddHandler client.EventMessage, AddressOf client_OnEventMessage
                    AddHandler client.EventError, AddressOf client_OnEventError

                    hashClient.Add(client, ArrayList.Synchronized(New ArrayList))
                    client_OnEventMessage(clsSocketTCP.SocketEvent.DataArrival, New Byte() {}, client)  'force a data arrival event
            End Select
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try

    End Sub
    Private Sub sockServer_OnEventError(ByVal errorFunction As String, ByVal errorString As String, ByVal socket As clsSocketTCP)
        HostStop()
    End Sub
    Private Sub client_OnEventMessage(ByVal socketEvent As clsSocketTCP.SocketEvent, ByVal data As Object, ByVal socket As clsSocketTCP)
        Dim dataQ As Queue
        Dim client As clsSocketTCPClient
        Dim mutexPackageGamePacket As Mutex
        Dim mutexProcessAllPacket As Mutex
        Try
            client = CType(socket, clsSocketTCPClient)
            Select Case socketEvent
                Case clsSocketTCP.SocketEvent.ConnectionClosed
                    ClientStop(client)
                Case clsSocketTCP.SocketEvent.ConnectionFailed
                    ClientStop(client)
                Case clsSocketTCP.SocketEvent.ConnectionEstablished
                Case clsSocketTCP.SocketEvent.DataArrival
                    If hashClient.Contains(client) Then

                        mutexPackageGamePacket = New Mutex(False, String.Format("mutex-PackageGamePacket-{0}", client.GetHashCode)) 'lock when same socket instance comes in
                        mutexPackageGamePacket.WaitOne()
                        dataQ = client.GetReceiveQueue
                        Do Until dataQ.Count = 0
                            CType(hashClient.Item(client), ArrayList).Add(dataQ.Dequeue())
                        Loop
                        PackageGamePacket(client)
                        mutexPackageGamePacket.ReleaseMutex()

                        mutexProcessAllPacket = New Mutex(False, String.Format("mutex-ProcessAllPacket-{0}", Me.GetHashCode)) 'lock when same class instance comes in
                        mutexProcessAllPacket.WaitOne()
                        ProcessAllPacket()
                        mutexProcessAllPacket.ReleaseMutex()

                    End If
            End Select
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try

    End Sub
    Private Sub client_OnEventError(ByVal errorFunction As String, ByVal errorString As String, ByVal socket As clsSocketTCP)
        SendChat(String.Format("TCP Socket ERROR detected for {0}.", protocol.GetPlayerFromSocket(CType(socket, clsSocketTCPClient)).GetName))
        ClientStop(CType(socket, clsSocketTCPClient))
    End Sub
#End Region

#Region "bot game event"
    Private Sub botGame_EventBotResponse(ByVal msg As String) Handles botGame.EventBotResponse
        SendChat(msg)
    End Sub
    Private Sub botGame_EventBotLatency(ByVal ms As Integer) Handles botGame.EventBotLatency
        If ms > 0 Then
            actionTimer.Interval = ms
        End If
        'SendChat(String.Format("Latency Time = {0}, Quality <- - Latency + -> Performance", actionTimer.Interval))
        SendChat(String.Format("Latency Time = {0},  Use a higher value for less spikes or a lower value for less delay.", actionTimer.Interval))
    End Sub

    Private Sub botGame_EventBotKick(ByVal name As String, ByVal kicker As String) Handles botGame.EventBotKick
        Dim player As clsHostPlayer

        Try
            For Each player In protocol.GetPlayerList(protocol.GetHostPID)
                If player.GetName.ToLower = name.ToLower Then
                    ClientStop(player.GetSock)
                    SendChat(String.Format("{0} was kicked out of game by {1}", name, kicker))
                End If
            Next
        Catch ex As Exception
        End Try
    End Sub
    Private Sub botGame_EventBotGameCancel() Handles botGame.EventBotGameCancel
        If isGameLoaded = True AndAlso teamWon = 255 Then 'winner not yet set
            teamWon = 10
            AnnouceWinner(callerName, gameName, CType(listSentinelPlayer.ToArray(GetType(String)), String()), CType(listScourgePlayer.ToArray(GetType(String)), String()), CType(listReferee.ToArray(GetType(String)), String()), "canceled")
            SendChat("Game is canceled")
        Else
            SendChat("Game can not be canceled or is too late to be canceled")
        End If
    End Sub

    Private Sub botGame_EventBotAbort() Handles botGame.EventBotAbort
        If endTimer.Enabled Then
            endTimer.Enabled = False
            SendChat("Game Auto-Shutdown Is Aborted")
        End If
    End Sub

#End Region

#Region "bot lobby event"

    Private Sub EventData_SetAccess(ByVal name As String, ByVal accessLevel As adminFlags)
        Dim username As String = ""

        username = data.userList.fixName(name)

        If username.Length > 0 Then
            data.adminList.getUser(name).setAccess(accessLevel)
            SendChat(String.Format("Updating {0}'s access level to {1}", username, accessLevel))
        Else
            SendChat(String.Format("Error: {0} is not a valid user", name))
        End If
    End Sub
    Private Sub EventMessage_SendChat(ByVal msg As String)
        'MsgBox("hit the event chat")
        SendChat(msg)
    End Sub
    Private Sub EventMessage_IncomingClanList(ByVal eventClanList() As clsIncomingClanList)
        'MrJag|0.8c|reserve|
        For Each clanPlayer As clsIncomingClanList In eventClanList
            If (reserveList.Contains(clanPlayer.GetName.ToLower)) Then
                'do nothing
            Else
                reserveList.Add(clanPlayer.GetName.ToLower)
            End If
        Next
    End Sub
    Private Sub EventMessage_IncomingFriendList(ByVal eventFriendList() As clsIncomingFriendList)
        'MrJag|0.8c|reserve|
        For Each friendPlayer As clsIncomingFriendList In eventFriendList
            'MsgBox(String.Format("{0} in my friend!", friendPlayer.GetAccount))
            If (reserveList.Contains(friendPlayer.GetAccount.ToLower)) Then
                'do nothing
            Else
                reserveList.Add(friendPlayer.GetAccount.ToLower)
            End If
        Next
    End Sub
    Private Sub bnet_EventBnetSIDSTARTADVEX3Result(ByVal isOK As Boolean)
        If data.botSettings.enable_refreshDisplay = True Then
            If isOK Then
                'SendChat("Game Listing Refresh Successful...")
                SendChat("Game Refreshed...")
            Else
                'SendChat("Game Listing Refresh Failed...")
                SendChat("Error refreshing the game...")
            End If
        End If
    End Sub
    Private Sub bnet_EventIncomingChat(ByVal eventChat As LainBnetCore.clsIncomingChatChannel)
        Dim playerName As String = ""       ' The player name parsed from the /whois
        Dim PID As Byte = New Byte
        'If eventChat.GetChatEvent = clsProtocolBNET.IncomingChatEvent.EID_WHISPER AndAlso eventChat.GetMessage.Trim = Convert.ToString(spoofID) Then
        If eventChat.GetChatEvent = clsProtocolBNET.IncomingChatEvent.EID_WHISPER Then
            If spoofSafe.Contains(eventChat.GetUser) = False Then
                'SendChatLobby(String.Format("{0} is not safelisted yet.", eventChat.GetUser))
                'MrJag|0.8c|antispoof|Adds the user to the safelist if they are in the current game.
                If protocol.GetPlayerFromName(eventChat.GetUser).GetName = eventChat.GetUser Then 'check if we have a player in the game with that name
                    spoofSafe.Add(eventChat.GetUser)
                    'SendChat(String.Format("Identification for {0} is accepted", eventChat.GetUser))
                End If
            Else
                'MrJag|0.8c|commands|Runs commands if neccessary
                If isCountDownStarted = False Then
                    botLobby.ProcessCommand(eventChat.GetUser, eventChat.GetMessage)
                ElseIf isGameLoaded Then
                    botGame.ProcessCommand(eventChat.GetUser, eventChat.GetMessage)
                End If
            End If
        ElseIf eventChat.GetChatEvent = clsProtocolBNET.IncomingChatEvent.EID_INFO And gameState = GAME_PUBLIC Then
            'MrJag|0.8c|antispoof|handles a battle.net info response to see if the player is in the game.
            playerName = Split(eventChat.GetMessage)(0)     'grab the first word in the message as the player name.
            If protocol.GetPlayerFromName(playerName).GetPID <> 255 Then
                If eventChat.GetMessage.Contains("is away") Then
                    'do nothing -- the user setup an away message
                ElseIf eventChat.GetMessage.Contains("is unavailable") Then
                    'do nothing -- the user setup a ??? message
                ElseIf eventChat.GetMessage.Contains("is refusing messages") Then
                    'do nothing -- the user setup a Do Not Disturb message
                ElseIf (eventChat.GetMessage.Contains("is using Warcraft III The Frozen Throne in the channel") Or eventChat.GetMessage.Contains("is using Warcraft III The Frozen Throne in channel")) Then
                    SendChat(String.Format("Name spoof detected -- The real {0} is not in a game.", playerName))     'chat alert that the player spoofchecked
                ElseIf eventChat.GetMessage.Contains("is using Warcraft III The Frozen Throne in a private channel") Then
                    SendChat(String.Format("Name spoof detected -- The real {0} is in a private channel.", playerName))     'chat alert that the player spoofchecked
                ElseIf (eventChat.GetMessage.Contains(String.Format("is using Warcraft III The Frozen Throne in game", gameName)) Or eventChat.GetMessage.Contains(String.Format("is using Warcraft III Frozen Throne and is currently in  game", gameName))) Then
                    If eventChat.GetMessage.Contains(gameName) Then
                        If protocol.GetPlayerFromName(playerName).GetName = playerName Then
                            spoofSafe.Add(playerName)                           'add the player to the spoofcheck list
                            'SendChatLobby(String.Format("{0} has passed the anti-spoof check.", playerName))     'chat alert that the player spoofchecked
                            'MsgBox(String.Format("Adding {0} to the safe list.", playerName))
                        End If
                    Else
                        SendChat(String.Format("Name spoof detected -- The real {0} is in another game.", playerName))     'chat alert that the player spoofchecked
                    End If
                ElseIf eventChat.GetMessage.Length = 0 Then
                    'do nothing -- the clan has a Message Of The Day setup for their channel.
                Else
                    'protocol.GetPlayerFromName(playerName).SpoofCheck()
                    'AddHandler protocol.GetPlayerFromName(playerName).EventSpoofCheck, AddressOf OnEventMessage_SpoofCheck
                    'SendChat(String.Format("DEBUG: {0}:[{1}]", playerName, eventChat.GetMessage))   'debug code - replace with auto-kick later
                    'bnet.SendChatToQueue(New clsBNETChatMessage(String.Format("/w {0} I was unable to confirm your identity, '/r anything' to manually confirm.", playerName), hostName, False))
                    'bnet.SendChatToQueue(New clsBNETChatMessage(String.Format("/w MrJag antispoof debug: [{0}] [{1}]", playerName, eventChat.GetMessage), hostName, False))
                End If
            End If
        End If
    End Sub

    Private Sub botLobby_EventBotKick(ByVal name As String) Handles botLobby.EventBotKick
        Dim player As clsHostPlayer

        Try
            For Each player In protocol.GetPlayerList(protocol.GetHostPID)
                If player.GetName.ToLower = name.ToLower Then
                    ClientStop(player.GetSock)
                End If
            Next
        Catch ex As Exception
        End Try
    End Sub
    'MrJag|0.10|refresh|
    Private Sub botLobby_EventBotToggleRefresh(ByVal enabled As Boolean) Handles botLobby.EventBotToggleRefresh
        Try
            If enabled Then
                data.botSettings.enable_refreshDisplay = True
                SendChat("Enabling the display of auto-refresh.")
            Else
                data.botSettings.enable_refreshDisplay = False
                SendChat("Disabling the display of auto-refresh.")
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub botLobby_EventBotLock(ByVal username As String) Handles botLobby.EventBotLock
        startLock = protocol.GetPlayerFromName(username).GetPID()
        SendChat("Game locked.")
    End Sub
    Private Sub botLobby_EventBotUnlock(ByVal username As String) Handles botLobby.EventBotUnlock
        startLock = 255
        SendChat("Game unlocked.")
    End Sub

    'MrJag|0.9b|hold|
    Private Sub botLobby_EventBotHold(ByVal name As String) Handles botLobby.EventBotHold
        Try
            If (reserveList.Contains(name.ToLower)) Then
                SendChat(String.Format("A reservation already exists for {0}", name))
            Else
                reserveList.Add(name.ToLower)
                SendChat(String.Format("Adding a reservation for {0}", name))
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub botLobby_EventBotSwap(ByVal slot1 As Byte, ByVal slot2 As Byte) Handles botLobby.EventBotSwap
        If protocol.SwapSlot(slot1, slot2) Then
            SendSlotInfo()
        End If
    End Sub
    Private Sub botLobby_EventBotShufflePlayers() Handles botLobby.EventBotShufflePlayers
        Dim randomNumber As New System.Random(System.DateTime.Now.Millisecond)
        Dim slot1 As Integer = 0
        Dim slot2 As Integer = randomNumber.Next(1, 9)  'seed initial random slot
        For index = 1 To 40
            slot1 = slot2
            Do
                slot2 = randomNumber.Next(1, 10)  'seed initial random slot (first slot, last slot + 1)
                If slot1 <> slot2 Then
                    Exit Do
                End If
            Loop
            protocol.SwapSlot(CByte(slot1), CByte(slot2))
        Next
        SendSlotInfo()
    End Sub
    Private Sub botLobby_EventBotResponse(ByVal msg As String) Handles botLobby.EventBotResponse
        SendChat(msg)
    End Sub
    Private Sub botLobby_EventBotComputer(ByVal SID As Byte, ByVal skill As Byte) Handles botLobby.EventBotComputer
        Dim player As clsHostPlayer = protocol.GetPlayerFromSID(SID)
        If player.GetPID <> 255 Then
            ClientStop(player.GetSock, True)
            protocol.SlotComputer(SID, skill)
        Else
            protocol.SlotComputer(SID, skill)
        End If
        SendSlotInfo()
    End Sub

    Private Sub botLobby_EventBotSlot(ByVal open As Boolean, ByVal SID As Byte) Handles botLobby.EventBotSlot
        Dim player As clsHostPlayer = protocol.GetPlayerFromSID(SID)
        If player.GetPID <> 255 Then
            ClientStop(player.GetSock, open)
        Else
            protocol.SlotOpenClose(SID, open)
        End If
        SendSlotInfo()
    End Sub
    Private Sub botLobby_EventBotStart(ByVal isForced As Boolean) Handles botLobby.EventBotStart
        Dim unsafes As System.Text.StringBuilder

        countdownCounter = 5

        If isForced = False Then    'is not forced then must pass spoof check and balance team
            If startLock <> 255 Then
                SendChat(String.Format("The game has been locked by {0}, an admin must clear the lock or use -START FORCE to continue.", protocol.GetPlayerFromPID(startLock).GetName))
                countdownCounter = -1
                Return
            End If
            If protocol.GetPlayerCountTeam(TEAM_SENTINEL) <> protocol.GetPlayerCountTeam(TEAM_SCOURGE) Then
                SendChat("The teams are uneven, use -START FORCE to continue.")
                countdownCounter = -1
                Return
            End If

            unsafes = New System.Text.StringBuilder
            For Each player In protocol.GetPlayerList(protocol.GetHostPID)
                If spoofSafe.Contains(player.GetName) = False Then
                    unsafes.Append(String.Format("{0}, ", player.GetName))
                End If
            Next

            If unsafes.Length > 0 Then
                SendChat(String.Format("Players Require Identification: {0}", unsafes.ToString))
                SendChat(String.Format("Please Identify (Spoof Check) By Whispering [ /w {0} ]", hostName))
                countdownCounter = -1
                Return
            End If
        End If

        'GameStart(isForced)
    End Sub
    Private Sub botLobby_EventBotEnd() Handles botLobby.EventBotEnd
        Dispose("Lobby Canceled")
    End Sub
    'MrJag|0.8c|ping|function for ping through firewall
    Private Sub botLobby_EventBotPing(ByVal maxPing As Integer) Handles botLobby.EventBotPing
        Dim pingText As System.Text.StringBuilder = New System.Text.StringBuilder

        Dim player As clsHostPlayer
        Dim players As Array = protocol.GetPlayerList 'MrJag|0.8c|observer| protocol.GetPlayerList(protocol.GetHostPID)
        Dim playerPingComparer As clsPlayerPingComparer = New clsPlayerPingComparer()
        Dim playerPing As Long
        Dim kickList As ArrayList = New ArrayList
        Dim kickText As System.Text.StringBuilder = New System.Text.StringBuilder
        kickText.Append(String.Format("Players kicked for pinging over {0}ms: ", maxPing))

        Array.Sort(players, playerPingComparer)
        Array.Reverse(players)
        For Each player In players
            If player.GetPID = protocol.GetHostPID Then
                'skip
            Else
                playerPing = player.GetPing
                If playerPing >= 0 Then
                    If playerPing > maxPing Then
                        For Each slot In protocol.GetSlotList
                            If slot.GetPID = player.GetPID Then
                                kickList.Add(slot.GetSID)
                                kickText.Append(String.Format("{0}, ", player.GetName))
                                'SendChat(String.Format("Kicking {0} for having a ping over {1}.", player.GetName, maxPing))
                                'botLobby_EventBotSlot(True, slot.GetSID)  'kick the player
                            End If
                        Next
                    End If
                    pingText.Append(String.Format("{0}: {1}ms, ", player.GetName, playerPing))
                ElseIf player.GetPing = -1 Then
                    pingText.Append(String.Format("{0}: NA, ", player.GetName))
                Else
                    pingText.Append(String.Format("{0}: DEBUG({1}), ", player.GetName, playerPing))
                End If
            End If
        Next
        SendChat(String.Format("{0}", pingText))
        If kickList.Count > 0 Then
            SendChat(String.Format("{0}", kickText))
            For index = 0 To kickList.Count
                botLobby_EventBotSlot(True, CByte(kickList.Item(index)))  'kick the player
            Next
        End If
    End Sub
    Private Sub OnEventPingCompleted(ByVal sender As Object, ByVal e As System.Net.NetworkInformation.PingCompletedEventArgs)
        Dim result As PingReply
        Dim player As clsHostPlayer
        Try
            result = e.Reply
            player = CType(e.UserState, clsHostPlayer)

            If result.Status = IPStatus.Success Then
                player.SetPing(result.RoundtripTime)
            Else
                player.SetPing(-1)
            End If
            RemoveHandler CType(sender, Ping).PingCompleted, AddressOf OnEventPingCompleted
            CType(sender, IDisposable).Dispose()
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try

    End Sub

    Private Sub OnEventMessage_SpoofCheck(ByVal name As String)
        Try
            'Debug.WriteLine(String.Format("Reached OnEventMessage_SpoofCheck({0})", name))
            If gameState = GAME_PUBLIC Then
                bnet.SendChatToQueue(New clsBNETChatMessage(String.Format("/whois {0}", name), hostName, False))
            ElseIf gameState = GAME_PRIVATE Then
                bnet.SendChatToQueue(New clsBNETChatMessage(String.Format("/w {0} Spoofcheck by replying to this message [ /r spoofcheck ]", name), hostName, False))
            End If
            RemoveHandler protocol.GetPlayerFromName(name).EventSpoofCheck, AddressOf OnEventMessage_SpoofCheck
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try

    End Sub

    Private Sub botLobby_EventBotCountry() Handles botLobby.EventBotCountry
        Dim url As String
        Dim IP As String
        Dim player As clsHostPlayer
        Dim request As HttpWebRequest
        Dim response As HttpWebResponse
        Dim reader As StreamReader
        Dim html As String
        Dim nameQ As Queue
        Dim country As String
        Dim countryText As System.Text.StringBuilder
        Try
            'SendChat("Credit to wc3banlist.de, Checking IP Country...")
            url = "http://www.wc3banlist.de/iptc.php?addr="
            nameQ = New Queue
            For Each player In protocol.GetPlayerList(protocol.GetHostPID)
                IP = String.Format("{0}.{1}.{2}.{3}", player.GetExternalIP(0), player.GetExternalIP(1), player.GetExternalIP(2), player.GetExternalIP(3))
                url = url & IP & ";"
                nameQ.Enqueue(player.GetName)
            Next

            request = CType(WebRequest.Create(url), HttpWebRequest)
            request.UserAgent = "Mozilla/3.0 (compatible; Indy Library)"
            response = CType(request.GetResponse(), HttpWebResponse)

            reader = New StreamReader(response.GetResponseStream())
            html = reader.ReadToEnd()
            reader.Close()
            response.Close()

            countryText = New System.Text.StringBuilder
            For Each country In html.Split(Convert.ToChar(";"))
                If country.LastIndexOf(Convert.ToChar("(")) >= 0 AndAlso country.EndsWith(")") AndAlso nameQ.Count > 0 Then
                    countryText.Append(String.Format("{0}: ({1}), ", CType(nameQ.Dequeue, String), country.Substring(country.LastIndexOf(Convert.ToChar("(")) + 1, 2)))
                End If
            Next

            SendChat(String.Format("{0}", countryText))
        Catch ex As Exception
            Debug.Write(ex)
        End Try
    End Sub

#End Region

#Region "protocol event"
    Private Sub protocol_EventHostChat(ByVal fromPID As Byte, ByVal toPIDs As Byte(), ByVal flag As Byte, ByVal flagextra As Byte(), ByVal msg As String) Handles protocol.EventHostChat
        Dim PID As Byte
        Dim name As String

        If isCountDownStarted = False OrElse isGameLoaded = True Then
            For Each PID In toPIDs
                protocol.GetPlayerFromPID(PID).GetSock.Send(protocol.SEND_W3GS_CHAT_FROM_HOST(fromPID, toPIDs, flag, flagextra, msg))
            Next
        End If

        name = protocol.GetPlayerFromPID(fromPID).GetName

        If spoofSafe.Contains(name) Then
            If isCountDownStarted = False Then
                botLobby.ProcessCommand(name, msg)
            ElseIf isGameLoaded Then
                botGame.ProcessCommand(name, msg)
            End If
            'Else
            'If msg.StartsWith("-") Then
            'SendChat(String.Format("Please Identify (Spoof Check) By Whispering [ /w {0} {1} ]", hostName, spoofID), fromPID)
            'End If
        End If
    End Sub
    Private Sub protocol_EventHostTeam(ByVal fromPID As Byte, ByVal toPIDs As Byte(), ByVal flag As Byte, ByVal team As Byte) Handles protocol.EventHostTeam
        If isCountDownStarted = False Then
            If toPIDs.Length = 1 AndAlso toPIDs(0) = 255 AndAlso (protocol.PlayerTeamChange(fromPID, team)) Then
                SendSlotInfo()
            End If
        End If
    End Sub
    Private Sub protocol_EventAction(ByVal fromPID As Byte, ByVal actionCRC As Byte(), ByVal actionData As Byte()) Handles protocol.EventAction
        Dim packet As ArrayList
        Dim buffer As Byte()
        Dim team As Byte
        Dim i As Integer
        Dim currentByte As Byte
        Dim list As ArrayList
        Dim text As String
        Dim playerkills(11) As Integer
        Dim playerdeaths(11) As Integer
        Dim playercreeps(11) As Integer
        Dim playerdenies(11) As Integer

        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {fromPID})
            clsHelper.AddByteArray(packet, clsHelper.IntegerToWORD(actionData.Length, False))  'length
            clsHelper.AddByteArray(packet, actionData)
            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            listAction.Add(buffer)


            If buffer.Length >= 3 AndAlso buffer(3) = 107 Then
                'Debug.WriteLine(clsHelper.PrintArray(buffer))
                'Debug.WriteLine(clsHelper.PrintArray(buffer, clsHelper.PrintType.ASCII))

                '---------[Get Playerstats]----------------
                playerkills(0) = buffer(13)
                playerdeaths(0) = buffer(27)
                playercreeps(0) = buffer(41)
                playerdenies(0) = buffer(55)

                playerkills(5) = buffer(84)
                playerdeaths(5) = buffer(98)
                playercreeps(5) = buffer(112)
                playerdenies(5) = buffer(126)

                playerkills(1) = buffer(155)
                playerdeaths(1) = buffer(169)
                playercreeps(1) = buffer(183)
                playerdenies(1) = buffer(197)

                playerkills(6) = buffer(226)
                playerdeaths(6) = buffer(240)
                playercreeps(6) = buffer(254)
                playerdenies(6) = buffer(268)

                playerkills(2) = buffer(297)
                playerdeaths(2) = buffer(311)
                playercreeps(2) = buffer(325)
                playerdenies(2) = buffer(339)

                playerkills(7) = buffer(368)
                playerdeaths(7) = buffer(382)
                playercreeps(7) = buffer(396)
                playerdenies(7) = buffer(410)

                playerkills(3) = buffer(439)
                playerdeaths(3) = buffer(453)
                playercreeps(3) = buffer(467)
                playerdenies(3) = buffer(481)

                playerkills(8) = buffer(511)
                playerdeaths(8) = buffer(526)
                playercreeps(8) = buffer(541)
                playerdenies(8) = buffer(556)

                playerkills(4) = buffer(586)
                playerdeaths(4) = buffer(600)
                playercreeps(4) = buffer(614)
                playerdenies(4) = buffer(628)

                playerkills(9) = buffer(658)
                playerdeaths(9) = buffer(673)
                playercreeps(9) = buffer(688)
                playerdenies(9) = buffer(703)
                '---------[Get Playerstats]----------------

                list = New ArrayList
                i = buffer.Length - 1 - 5
                Do
                    currentByte = buffer(i)
                    list.Add(currentByte)
                    i = i - 1
                    If currentByte = 0 Then
                        list.Reverse()
                        text = clsHelper.ByteArrayToStringASCII(CType(list.ToArray(GetType(Byte)), Byte()))
                        Exit Do
                    End If
                Loop
                team = buffer(buffer.Length - 1 - 3)
                If isGameLoaded = True AndAlso teamWon = 255 Then 'winner not yet set
                    teamWon = CType(team - 1, Byte)
                    SendChat(String.Format("{0} Wins!", GetTeamName(teamWon)))
                    SendChat(String.Format("Player 1:  Kills({0}), Deaths({1}), Creeps({2}), Denies({3}).", playerkills(0), playerdeaths(0), playercreeps(0), playerdenies(0)))
                    'SendChat(String.Format("{0} Has Won The Game ! Tree/Throne Down [{1}]", GetTeamName(teamWon), protocol.GetPlayerFromPID(fromPID).GetName))
                    AnnouceWinner(callerName, gameName, CType(listSentinelPlayer.ToArray(GetType(String)), String()), CType(listScourgePlayer.ToArray(GetType(String)), String()), CType(listReferee.ToArray(GetType(String)), String()), GetTeamName(teamWon))
                End If

            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub

#End Region

#Region "timers"
    Private Sub engineTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles engineTimer.Elapsed
        Static isRunning As Boolean = False

        Try
            If isRunning = False Then
                isRunning = True

                timeGameExpire = timeGameExpire + 1
                If timeGameExpire Mod TIME_GAME_EXPIRE_COUNTER = 0 Then

                    If isGameLoaded = True AndAlso teamWon = 255 Then 'winner not yet set
                        If protocol.GetPlayerCountTeam(TEAM_SENTINEL) = 0 AndAlso protocol.GetPlayerCountTeam(TEAM_SCOURGE) > 0 Then
                            teamWon = TEAM_SCOURGE
                            SendChat(String.Format("{0} has won the game !", GetTeamName(teamWon)))
                            AnnouceWinner(callerName, gameName, CType(listSentinelPlayer.ToArray(GetType(String)), String()), CType(listScourgePlayer.ToArray(GetType(String)), String()), CType(listReferee.ToArray(GetType(String)), String()), GetTeamName(teamWon))
                        ElseIf protocol.GetPlayerCountTeam(TEAM_SCOURGE) = 0 AndAlso protocol.GetPlayerCountTeam(TEAM_SENTINEL) > 0 Then
                            teamWon = TEAM_SENTINEL
                            SendChat(String.Format("{0} has won the game !", GetTeamName(teamWon)))
                            AnnouceWinner(callerName, gameName, CType(listSentinelPlayer.ToArray(GetType(String)), String()), CType(listScourgePlayer.ToArray(GetType(String)), String()), CType(listReferee.ToArray(GetType(String)), String()), GetTeamName(teamWon))
                        ElseIf protocol.GetPlayerCountTeam(TEAM_SCOURGE) = 0 AndAlso protocol.GetPlayerCountTeam(TEAM_SENTINEL) = 0 Then
                            teamWon = 10
                            SendChat("No one remain in game")
                            AnnouceWinner(callerName, gameName, CType(listSentinelPlayer.ToArray(GetType(String)), String()), CType(listScourgePlayer.ToArray(GetType(String)), String()), CType(listReferee.ToArray(GetType(String)), String()), "none")
                        End If
                    End If

                    'If protocol.GetPlayerCount(protocol.GetHostPID) = 0 Then
                    If protocol.GetPlayerCount = 0 Then
                        Dispose("No players remain")
                    End If
                End If

                isRunning = False
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub actionTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles actionTimer.Elapsed
        Static timeInterval As Integer = CType(Environment.TickCount - actionTimer.Interval, Integer)
        Static isRunning As Boolean = False
        Dim packet As ArrayList
        Dim action As Byte()
        Dim buffer As Byte()
        Try
            If isRunning = False Then
                isRunning = True

                packet = New ArrayList

                Do Until listAction.Count = 0
                    action = CType(listAction.Item(0), Byte())
                    listAction.RemoveAt(0)
                    packet.Add(action)
                Loop


                buffer = protocol.SEND_W3GS_INCOMING_ACTION(CType(packet.ToArray(GetType(Byte())), Byte()()), CType(Environment.TickCount - timeInterval, Integer))
                'Debug.WriteLine(String.Format("{0} - {1}", Environment.TickCount - timeInterval, clsHelper.PrintArray(buffer)))

                timeInterval = Environment.TickCount
                SendAllClient(buffer)

                isRunning = False
            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub
    Private Sub lobbyTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles lobbyTimer.Elapsed
        Static isRunning As Boolean = False
        Try
            If isRunning = False Then
                isRunning = True

                Dim kickList As ArrayList = New ArrayList
                Dim kickText As System.Text.StringBuilder = New System.Text.StringBuilder
                Dim pingComplete As Boolean = True
                kickText.Append(String.Format("Kicked for excessive pings:  "))

                'search for players with high pings and add them to the kick list
                For Each player As clsHostPlayer In protocol.GetPlayerList
                    If player.GetPID = protocol.GetHostPID Then
                        'skip
                    Else
                        If player.GetPing >= 0 AndAlso player.GetPing > frmLainEthLite.data.botSettings.maxPing Then
                            For Each slot In protocol.GetSlotList
                                If (slot.GetPID = player.GetPID) And (reserveList.Contains(player.GetName.ToLower) = False) Then
                                    kickList.Add(slot.GetSID)
                                    kickText.Append(String.Format("{0}, ", player.GetName))
                                End If
                            Next
                        ElseIf player.GetPing = -1 Then
                            pingComplete = False
                        End If
                    End If
                Next

                'kick the players on the kick list
                If kickList.Count > 0 Then
                    SendChat(String.Format("{0}", kickText))
                    While kickList.Count > 0
                        botLobby_EventBotSlot(True, CByte(kickList.Item(0)))  'kick the player
                        kickList.RemoveAt(0)
                    End While
                End If

                SendAllClient(protocol.SEND_W3GS_PING_FROM_HOST())  'ping all players


                If countdownCounter > 0 Then
                    SendChat(String.Format("{0}. . .", countdownCounter))
                    countdownCounter = countdownCounter - 1
                ElseIf countdownCounter = 0 Then
                    SendChat(String.Format("Game Started. . .", countdownCounter))
                    GameStart(False)
                ElseIf countdownCounter = -1 Or countdownCounter = -2 Then
                    If protocol.GetPlayerCount = 10 AndAlso pingComplete Then
                        countdownCounter = -3
                        SendChat("The game is ready to start...")
                    ElseIf protocol.GetPlayerCount = 10 AndAlso countdownCounter <> -2 Then
                        countdownCounter = -2
                        SendChat("Waiting on ping check to complete.")
                    End If
                ElseIf countdownCounter = -3 Then
                    If protocol.GetPlayerCount < 10 Or pingComplete = False Then
                        countdownCounter = -1
                    End If
                End If
                isRunning = False
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub endTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles endTimer.Elapsed
        Static isRunning As Boolean = False
        Static timeRemain As Integer = 10
        Try
            If isRunning = False Then
                isRunning = True

                timeRemain = timeRemain - 1
                If timeRemain < 4 Then
                    SendChat(String.Format("Game Shut Down in {0} seconds", timeRemain * 10))
                End If

                If timeRemain = 0 Then
                    Dispose("Shutdown Counter Reached 0")
                End If

                isRunning = False
            End If
        Catch ex As Exception
        End Try
    End Sub
    'MrJag|0.10|refresh|
    Private Sub refreshTimer_Elapsed() Handles refreshTimer.Elapsed
        Static isRunning As Boolean = False
        Try
            If isRunning = False Then
                isRunning = True

                'bnet.GameRefresh(gameState, numPlayers, gameName, hostName, (10 - protocol.GetPlayerCount), getUptime, mapPath, mapCRC)
                If GetTotalPlayers() < numPlayers Then
                    bnet.GameRefresh(gameState, numPlayers, gameName, hostName, (numPlayers - protocol.GetPlayerCount), getUptime, mapPath, mapCRC)
                End If

                'If enableRefresh = True Then
                'SendChat(String.Format("Refreshed [{0}].", gameName))
                'End If
                isRunning = False
            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub
#End Region


    Private Sub ProcessAllPacket()
        Dim command As clsCommandPacket
        Dim PID As Byte
        Dim SID As Byte 'MrJag|0.8c|reserve|used to save the slot ID of the player to kick
        Dim player As clsHostPlayer

        Try
            While queuePacket.Count > 0
                command = CType(queuePacket.Dequeue, clsCommandPacket)
                If command.GetPacketCommandType = clsCommandPacket.PacketType.CustomGame Then
                    Select Case command.GetPacketID
                        Case clsProtocolHost.Protocol.W3GS_GAMELOADED_SELF
                            player = protocol.GetPlayerFromSocket(command.GetPacketSocket())
                            If player.GetPID <> 255 Then
                                player.mapLoaded() 'store that the player is done loading
                                SendAllClient(protocol.SEND_W3GS_GAMELOADED_OTHERS(player.GetPID))
                                totalFinishLoad = totalFinishLoad + 1

                                'Debug.WriteLine(totalFinishLoad & " : " & protocol.GetPlayerCount(protocol.GetHostPID))
                                Debug.WriteLine(String.Format("[{0}/{1}] {2} finished loading in {3} seconds", totalFinishLoad, protocol.GetPlayerCount, player.GetName, Math.Round(Now.Subtract(timeLastLoad).TotalSeconds, 2)))
                                If totalFinishLoad = 1 Then
                                    loadAwards.Append(String.Format("Fastest Load By Player: {0} - {1} Seconds", player.GetName, Math.Round(Now.Subtract(timeLastLoad).TotalSeconds, 2)))
                                End If

                                If totalFinishLoad = protocol.GetPlayerCount Then
                                    Debug.WriteLine(String.Format("All {0} players finished loading.", protocol.GetPlayerCount))
                                    isGameLoaded = True
                                    lobbyTimer.Stop()
                                    actionTimer.Start()
                                    refreshTimer.Stop()

                                    SendChat(String.Format("{0}", loadAwards))
                                    SendChat(String.Format("Longest Load By Player: {0} - {1} Seconds", player.GetName, Math.Round(Now.Subtract(timeLastLoad).TotalSeconds, 2)))

                                End If
                            End If
                        Case clsProtocolHost.Protocol.W3GS_CHAT_TO_HOST
                            If protocol.RECEIVE_W3GS_CHAT_TO_HOST(command.GetPacketData) = False Then
                                Debug.WriteLine("FAILED RECEIVE_W3GS_CHAT_TO_HOST")
                            End If
                        Case clsProtocolHost.Protocol.W3GS_PONG_TO_HOST
                            If isCountDownStarted = False OrElse isGameLoaded = False Then
                                Dim tempNumber As Long = 0
                                tempNumber = protocol.RECEIVE_W3GS_PONG_TO_HOST(command.GetPacketData)
                                'SendChatLobby(String.Format("Received a ping response of {0}ms from {1}.", tempNumber, protocol.GetPlayerFromSocket(command.GetPacketSocket()).GetName))
                                protocol.GetPlayerFromSocket(command.GetPacketSocket()).SetPing(protocol.RECEIVE_W3GS_PONG_TO_HOST(command.GetPacketData))
                            Else
                                protocol.RECEIVE_W3GS_PONG_TO_HOST(command.GetPacketData) ' do nothing?
                            End If
                            'Case clsProtocolHost.Protocol.W3GS_PONG_TO_HOST
                            '    protocol.RECEIVE_W3GS_PONG_TO_HOST(command.GetPacketData)
                        Case clsProtocolHost.Protocol.W3GS_LEAVEGAME
                            If protocol.RECEIVE_W3GS_LEAVEGAME(command.GetPacketData) Then
                                Debug.WriteLine(protocol.GetPlayerFromSocket(command.GetPacketSocket()).GetName & " left voluntarily")
                                If protocol.GetPlayerFromSocket(command.GetPacketSocket()).isLoaded Then
                                    totalFinishLoad = totalFinishLoad - 1
                                End If

                                If isGameLoaded Then
                                    SendChat(String.Format("{0} has Left the game", protocol.GetPlayerFromSocket(command.GetPacketSocket()).GetName))
                                End If
                                ClientStop(command.GetPacketSocket())
                            End If
                        Case clsProtocolHost.Protocol.W3GS_REQJOIN
                            Dim playerCount As Integer = protocol.GetPlayerCount
                            Dim user As clsUser

                            player = protocol.RECEIVE_W3GS_REQJOIN(command.GetPacketData, command.GetPacketSocket())
                            If player.GetName().Length > 0 Then

                                'check to see if the user exists in the database
                                user = data.userList.getUser(player.GetName)
                                If user.name = "" Then
                                    SendChat(String.Format("Adding {0} as a new player in the local database.", player.GetName))
                                    data.userList.addUser(player.GetName, player.GetSock.GetLocalIP, player.GetSock.GetRemoteIP)
                                Else
                                    If user.ban.Length > 0 Then
                                        'player.cancelSpoofCheck()
                                        ClientStop(player.GetSock)
                                        SendChat(String.Format("{0} is blacklisted and cannot join this game.", player.GetName))
                                        Exit Select
                                    ElseIf user.vip = True Then
                                        'new database VIP list check
                                        SendChat(String.Format("{0} has VIP status and last played {1} day(s) ago.", player.GetName, Now.Subtract(user.recentGame).Days))
                                        SID = protocol.GetReserveSlot(reserveList)
                                        If SID <> 255 Then
                                            botLobby_EventBotSlot(True, SID)  'kick the non reserved player so the reserved player can join.
                                        End If
                                    ElseIf (reserveList.Contains(player.GetName.ToLower)) Then  'Check to see if the player is on the reserve list
                                        SendChat(String.Format("{0} has VIP status and is trying to join this game.", player.GetName))
                                        SID = protocol.GetReserveSlot(reserveList)
                                        If SID <> 255 Then
                                            botLobby_EventBotSlot(True, SID)  'kick the non reserved player so the reserved player can join.
                                        End If
                                    Else
                                        SendChat(String.Format("{0} last played {1} day(s) ago.", player.GetName, Now.Subtract(user.recentGame).Days))
                                    End If
                                End If

                                Debug.WriteLine(String.Format("{0} is trying to join the game.", player.GetName))
                                If player.GetName().ToLower = callerName.ToLower AndAlso protocol.GetEmptySlot = 255 Then
                                    Debug.WriteLine(String.Format("Host-player({0}) is trying to join the game and all slots are full.", player.GetName))
                                    botLobby_EventBotSlot(True, 0)  'the player trying to join is the host-player and all slots are full, empty a slot.
                                    Debug.WriteLine(String.Format("Kicking a player.", player.GetName))
                                End If

                                PID = protocol.PlayerAdd(player.GetName(), player.GetSock, player.GetExternalIP, player.GetInternalIP)
                                If PID <> 255 Then
                                    'Debug.WriteLine(String.Format("Adding spoofcheck handler for {0}", player.GetName))
                                    AddHandler protocol.GetPlayerFromPID(PID).EventSpoofCheck, AddressOf OnEventMessage_SpoofCheck
                                    protocol.GetPlayerFromPID(PID).SpoofCheck()

                                    Debug.WriteLine(String.Format("Player:{0} PID:{1} SID:{5} Internal:{2} External:{3} PlayerNumExclusive:{4} ", player.GetName, PID, clsHelper.PrintArray(player.GetInternalIP), clsHelper.PrintArray(player.GetExternalIP), playerCount, protocol.GetPlayerSlot(PID).GetSID))
                                    'Debug.WriteLine(player.GetName() & " : " & PID & " " & clsHelper.PrintArray(player.GetInternalIP) & " " & clsHelper.PrintArray(player.GetExternalIP))
                                    command.GetPacketSocket().Send(protocol.SEND_W3GS_SLOTINFOJOIN(PID, protocol.GetSlotInfo))

                                    For Each player In protocol.GetPlayerList(PID)
                                        player.GetSock.Send(protocol.SEND_W3GS_PLAYERINFO(PID))                         'send all other player about this new player
                                        command.GetPacketSocket().Send(protocol.SEND_W3GS_PLAYERINFO(player.GetPID))    'send this new player about other players
                                    Next

                                    command.GetPacketSocket().Send(protocol.SEND_W3GS_MAPCHECK(mapPath, mapSize, mapInfo, mapCRC))
                                    SendSlotInfo()

                                    'SendChatLobby(String.Format("Hi Welcome to Dota {0} Hosting Service", frmLainEthLite.ProjectLainName), PID)
                                    SendChat(" ", PID)
                                    SendChat(" ", PID)
                                    SendChat(" ", PID)
                                    SendChat(" ", PID)
                                    SendChat(" ", PID)
                                    SendChat("Battle.net Game Host                                                http://ghost.pwner.org", PID)
                                    SendChat("-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-", PID)
                                    SendChat(String.Format("          Game Name:    {0}", gameName), PID)
                                    Exit Select




                                End If
                            End If
                ClientStop(command.GetPacketSocket())
                        Case clsProtocolHost.Protocol.W3GS_MAPSIZE
                            If protocol.RECEIVE_W3GS_MAPSIZE(command.GetPacketData, mapSize) Then
                                SendSlotInfo()
                            Else
                                'SendChat(String.Format("{0} need to download the map to play in this game", protocol.GetPlayerFromSocket(command.GetPacketSocket()).GetName))
                                ClientStop(command.GetPacketSocket())
                            End If
                        Case clsProtocolHost.Protocol.W3GS_OUTGOING_KEEPALIVE
                            If protocol.RECEIVE_W3GS_OUTGOING_KEEPALIVE(command.GetPacketData) = False Then
                                Debug.WriteLine("FAILED W3GS_OUTGOING_KEEPALIVE")
                            End If
                        Case clsProtocolHost.Protocol.W3GS_OUTGOING_ACTION
                            If protocol.RECEIVE_W3GS_OUTGOING_ACTION(command.GetPacketData, protocol.GetPlayerFromSocket(command.GetPacketSocket()).GetPID) = False Then
                                Debug.WriteLine("FAILED W3GS_OUTGOING_ACTION")
                            End If
                        Case Else
                            Debug.WriteLine("W3GS : " & command.GetPacketID)
                            Debug.WriteLine(clsHelper.PrintArray(command.GetPacketData, clsHelper.PrintType.HEX))
                            Debug.WriteLine(clsHelper.PrintArray(command.GetPacketData, clsHelper.PrintType.ASCII))
                    End Select
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

                While listDataBuffer.Count >= 4
                    If CByte(listDataBuffer.Item(0)) = 247 Then
                        length = CType(clsHelper.ByteArrayToLong(New Byte() {CByte(listDataBuffer.Item(2)), CByte(listDataBuffer.Item(3))}), Integer)
                        If listDataBuffer.Count >= length Then
                            listPacket = New ArrayList
                            For i = 1 To length
                                listPacket.Add(listDataBuffer.Item(0))
                                listDataBuffer.RemoveAt(0)
                            Next
                            data = CType(listPacket.ToArray(GetType(Byte)), Byte())
                            queuePacket.Enqueue(New clsCommandPacket(clsCommandPacket.PacketType.CustomGame, data(1), data, client))
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


    Private Function AnnouceWinner(ByVal callerName As String, ByVal gameName As String, ByVal sentinelPlayer() As String, ByVal scourgePlayer() As String, ByVal referee() As String, ByVal winner As String) As Boolean
        RaiseEvent EventGameWon(callerName, gameName, sentinelPlayer, scourgePlayer, referee, winner)
        'SendChat("Game has officially ended, a Game Shut Down will be initialised, Please make your leave or use -ABORT to abort")
        SendChat("To report a bug or for more information on Dota Host Bot, visit http://ghost.pwner.org")
        SendChat("Credits: Leax, Netrunner, MrJag, Saidin, pooks0r, and everyone else that gave support.")
        endTimer.Start()
    End Function

    Private Function SendAllClient(ByVal data As Byte()) As Boolean
        Dim player As clsHostPlayer
        Try
            'For Each player In protocol.GetPlayerList(protocol.GetHostPID)
            For Each player In protocol.GetPlayerList()
                player.GetSock.Send(data)

                If player.GetSock.GetReceiveQueue.Count > 1 Then
                    Debug.WriteLine(String.Format("lag:[{0}->host][{1}]", player.GetName, player.GetSock.GetReceiveQueue.Count))
                End If
                If player.GetSock.GetSendQueue.Count > 1 Then
                    Debug.WriteLine(String.Format("lag:[host->{0}][{1}]", player.GetName, player.GetSock.GetSendQueue.Count))
                End If
            Next
        Catch ex As Exception
            Return False
        End Try
    End Function
    Private Function SendSlotInfo() As Boolean
        Try
            If isCountDownStarted = False Then
                Return SendAllClient(protocol.SEND_W3GS_SLOTINFO) 'send everyone new slot info
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function


    Private Function SendChat(ByVal msg As String, ByVal PID As Byte) As Boolean
        If isCountDownStarted = False Then
            If msg.Length > 220 Then
                msg = msg.Substring(0, 220)
            End If
            Return protocol.GetPlayerFromPID(PID).GetSock.Send(protocol.SEND_W3GS_CHAT_FROM_HOST(protocol.GetHostPID(), New Byte() {PID}, 16, New Byte() {}, msg))
        ElseIf isGameLoaded Then
            If msg.Length > 120 Then
                msg = msg.Substring(0, 120)
            End If
            Return SendAllClient(protocol.SEND_W3GS_CHAT_FROM_HOST(protocol.GetHostPID(), New Byte() {PID}, 32, New Byte() {0, 0, 0, 0}, msg))
        Else
            Return False
        End If
    End Function

    Private Function SendChat(ByVal msg As String) As Boolean
        If isCountDownStarted = False Then
            If msg.Length > 220 Then
                msg = msg.Substring(0, 220)
            End If
            'MsgBox("using countdown not started")
            'Return SendAllClient(protocol.SEND_W3GS_CHAT_FROM_HOST(protocol.GetHostPID(callerName), protocol.GetPIDList(protocol.GetHostPID), 16, New Byte() {}, msg))
            Return SendAllClient(protocol.SEND_W3GS_CHAT_FROM_HOST(protocol.GetHostPID(), protocol.GetPIDList(), 16, New Byte() {}, msg))
        ElseIf isGameLoaded Then
            If msg.Length > 120 Then
                msg = msg.Substring(0, 120)
            End If
            'Return SendAllClient(protocol.SEND_W3GS_CHAT_FROM_HOST(protocol.GetHostPID(callerName), protocol.GetPIDList(protocol.GetHostPID), 32, New Byte() {0, 0, 0, 0}, msg))
            'MsgBox("using game loaded")
            Return SendAllClient(protocol.SEND_W3GS_CHAT_FROM_HOST(protocol.GetHostPID(), protocol.GetPIDList(), 32, New Byte() {0, 0, 0, 0}, msg))
        Else
            Return False
        End If
    End Function
    Private Sub SpoofChatLobby(ByVal name As String, ByVal msg As String) Handles botLobby.EventBotSpoof
        If isCountDownStarted = False Then
            If msg.Length > 220 Then
                msg = msg.Substring(0, 220)
            End If
            SendAllClient(protocol.SEND_W3GS_CHAT_FROM_HOST(protocol.GetPlayerFromName(name).GetPID, protocol.GetPIDList(protocol.GetHostPID), 16, New Byte() {}, msg))
        ElseIf isGameLoaded Then
            If msg.Length > 120 Then
                msg = msg.Substring(0, 120)
            End If
            SendAllClient(protocol.SEND_W3GS_CHAT_FROM_HOST(protocol.GetPlayerFromName(name).GetPID, protocol.GetPIDList(protocol.GetHostPID), 32, New Byte() {0, 0, 0, 0}, msg))
        End If
    End Sub
    Private Sub SpoofChatGame(ByVal name As String, ByVal msg As String) Handles botGame.EventBotSpoof
        If isCountDownStarted = False Then
            If msg.Length > 220 Then
                msg = msg.Substring(0, 220)
            End If
            SendAllClient(protocol.SEND_W3GS_CHAT_FROM_HOST(protocol.GetPlayerFromName(name).GetPID, protocol.GetPIDList(protocol.GetHostPID), 16, New Byte() {}, msg))
        ElseIf isGameLoaded Then
            If msg.Length > 120 Then
                msg = msg.Substring(0, 120)
            End If
            SendAllClient(protocol.SEND_W3GS_CHAT_FROM_HOST(protocol.GetPlayerFromName(name).GetPID, protocol.GetPIDList(protocol.GetHostPID), 32, New Byte() {0, 0, 0, 0}, msg))
        End If
    End Sub


    Private Function GetTeamName(ByVal team As Byte) As String
        Select Case team
            Case Is = 0 : Return "Sentinel"
            Case Is = 1 : Return "Scourge"
            Case Else : Return "Team:" & CType(team, String)
        End Select
    End Function
    Private Function GameStart(ByVal isForced As Boolean) As Boolean
        Static runOnce As Boolean = False
        Dim player As clsHostPlayer
        Dim slot As clsHostSlot
        Try
            If runOnce = False Then
                runOnce = True
                For Each slot In protocol.GetSlotList
                    If slot.GetPID <> 0 AndAlso slot.GetPID <> 255 Then
                        player = protocol.GetPlayerFromPID(slot.GetPID)
                        If player.GetName.Length > 0 AndAlso spoofSafe.Contains(player.GetName) Then

                            If slot.GetTeam = 0 Then
                                listSentinelPlayer.Add(player.GetName)
                            ElseIf slot.GetTeam = 1 Then
                                listScourgePlayer.Add(player.GetName)
                            Else
                                listReferee.Add(player.GetName)
                            End If
                        End If
                    End If
                Next

                UnCreate()
                HostStop()

                isCountDownStarted = True
                SendAllClient(protocol.SEND_W3GS_COUNTDOWN_START)
                'insert delay here for countdown.
                protocol.RemoveHost()
                'Thread.Sleep(2000)
                SendAllClient(protocol.SEND_W3GS_COUNTDOWN_END)

                'SendAllClient(protocol.SEND_W3GS_GAMELOADED_OTHERS(protocol.GetHostPID))
                timeLastLoad = Now

                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Private Function ClientStop(ByVal clients As clsSocketTCPClient()) As Boolean
        Dim name As String
        Dim client As clsSocketTCPClient
        Dim PID As Byte
        Dim list As ArrayList

        Try
            list = New ArrayList

            For Each client In clients
                name = protocol.GetPlayerFromSocket(client).GetName
                If spoofSafe.Contains(name) AndAlso name <> hostName Then
                    spoofSafe.Remove(name)
                End If

                PID = protocol.PlayerRemove(client, True)
                If PID <> 255 Then
                    list.Add(PID)
                End If

                hashClient.Remove(client)
                client.Stop()
                RemoveHandler client.eventMessage, AddressOf client_OnEventMessage
                RemoveHandler client.eventError, AddressOf client_OnEventError
                RemoveHandler protocol.GetPlayerFromSocket(client).EventSpoofCheck, AddressOf OnEventMessage_SpoofCheck
            Next

            For Each PID In CType(list.ToArray(GetType(Byte)), Byte())
                SendAllClient(protocol.SEND_W3GS_PLAYERLEAVE_OTHERS(PID))
            Next
            SendSlotInfo()

            If isGameLoaded = False AndAlso countdownCounter >= 0 Then
                countdownCounter = -1
                SendChat("Countdown aborted!")
            End If
            If isCountDownStarted = False Then
                timeGameExpire = TIME_GAME_EXPIRE_COUNTER - 60 'check 1 minutes afer
            ElseIf isGameLoaded Then
                timeGameExpire = TIME_GAME_EXPIRE_COUNTER - 1      'check 1 sec after
            End If

            Return True
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return False
        End Try

    End Function
    Private Function ClientStop(ByVal client As clsSocketTCPClient) As Boolean
        Return ClientStop(client, True)
    End Function
    Private Function ClientStop(ByVal client As clsSocketTCPClient, ByVal slotOpen As Boolean) As Boolean
        Dim PID As Byte
        Dim player As clsHostPlayer
        Dim name As String
        Try
            name = protocol.GetPlayerFromSocket(client).GetName
            If spoofSafe.Contains(name) AndAlso name <> hostName Then
                spoofSafe.Remove(name)
            End If

            PID = protocol.PlayerRemove(client, slotOpen)
            If PID <> 255 Then
                For Each player In protocol.GetPlayerList(PID)
                    player.GetSock.Send(protocol.SEND_W3GS_PLAYERLEAVE_OTHERS(PID))
                Next
                SendSlotInfo()
            End If

            hashClient.Remove(client)
            client.Stop()
            RemoveHandler client.EventMessage, AddressOf client_OnEventMessage
            RemoveHandler client.EventError, AddressOf client_OnEventError
            RemoveHandler protocol.GetPlayerFromSocket(client).EventSpoofCheck, AddressOf OnEventMessage_SpoofCheck

            If isGameLoaded = False AndAlso countdownCounter >= 0 Then
                countdownCounter = -1
                SendChat("Countdown aborted!")
            End If
            If isCountDownStarted = False Then
                timeGameExpire = TIME_GAME_EXPIRE_COUNTER - 60 'check 1 minutes afer
            ElseIf isGameLoaded Then
                timeGameExpire = TIME_GAME_EXPIRE_COUNTER - 1      'check 1 sec after
            End If

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
    Public Function HostStart() As Boolean
        Try
            If sockServer.Listen(gamePort) Then
                engineTimer.Start()
                lobbyTimer.Start()
                If enableRefresh And gameState = GAME_PUBLIC Then
                    refreshTimer.Start() 'MrJag|0.8c|refresh|start the timer
                End If
                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function UnCreate() As Boolean
        refreshTimer.Stop() 'MrJag|0.10|refresh| stop the timer
        RemoveHandler bnet.EventBnetSIDSTARTADVEX3Result, AddressOf bnet_EventBnetSIDSTARTADVEX3Result
        RaiseEvent EventHostUncreate()
    End Function
    Public Function Dispose(ByVal reason As String) As Boolean
        Dim client As clsSocketTCPClient
        Dim list As ArrayList
        Try
            Debug.WriteLine("DISPOSED : " & reason)

            HostStop()

            If isCountDownStarted = False Then
                UnCreate()
            End If

            list = New ArrayList
            SyncLock hashClient.SyncRoot
                For Each client In hashClient.Keys
                    list.Add(client)
                Next
            End SyncLock

            ClientStop(CType(list.ToArray(GetType(clsSocketTCPClient)), clsSocketTCPClient()))

            engineTimer.Stop()
            lobbyTimer.Stop()
            actionTimer.Stop()
            endTimer.Stop()
            refreshTimer.Stop() 'MrJag|0.10|refresh| stop the timer

            RemoveHandler sockServer.eventMessage, AddressOf sockServer_OnEventMessage
            RemoveHandler sockServer.eventError, AddressOf sockServer_OnEventError
            RemoveHandler bnet.EventIncomingChat, AddressOf bnet_EventIncomingChat
            RemoveHandler bnet.EventBnetSIDSTARTADVEX3Result, AddressOf bnet_EventBnetSIDSTARTADVEX3Result
            RemoveHandler bnet.EventIncomingFriendList, AddressOf EventMessage_IncomingFriendList
            RemoveHandler bnet.EventIncomingClanList, AddressOf EventMessage_IncomingClanList
            RemoveHandler botLobby.EventBotSay, AddressOf EventMessage_SendChat
            RemoveHandler botGame.EventBotSay, AddressOf EventMessage_SendChat


            RaiseEvent EventHostDisposed(Me, reason)
            Return True
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return False
        End Try

    End Function

    Public Function SendMessage(ByVal name As String, ByVal msg As String) As Integer
        Dim player As clsHostPlayer
        Dim total As Integer

        Try
            total = 0
            For Each player In protocol.GetPlayerList(protocol.GetHostPID)
                If player.GetName.ToLower = name.ToLower Then
                    SendChat(msg, player.GetPID)
                    total = total + 1
                End If
            Next
            Return total
        Catch ex As Exception
            Return 0
        End Try
    End Function
    Public Function SendMessage(ByVal msg As String) As Integer
        Dim player As clsHostPlayer
        Dim total As Integer

        Try
            total = 0
            For Each player In protocol.GetPlayerList(protocol.GetHostPID)
                SendChat(msg, player.GetPID)
                total = total + 1
            Next
            Return total
        Catch ex As Exception
            Return 0
        End Try
    End Function

End Class


























'