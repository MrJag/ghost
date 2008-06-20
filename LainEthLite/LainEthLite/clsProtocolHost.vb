Option Explicit On
Option Strict On

Imports LainBnetCore
Imports LainSocket
Imports LainHelper

#Region "related classes"
Public Class clsHostSlot
    Private SID As Byte
    Private PID As Byte
    Private downloadstatus As Byte  '0% - 100%
    Private slotstatus As Byte      '0 open, 1 closed, 2 controlled
    Private iscomputer As Byte      '0 no, 1 yes
    Private team As Byte
    Private color As Byte
    Private race As Byte            '1 HU, 2 ORC, 4 NE, 8 UD, 32 random, 64 fixed
    Private computerType As Byte    '0 - easy comp, 2 - hard comp, 1 - human/normal comp
    Private handicap As Byte

    Public Sub New()
        SID = 255
        PID = 255
        downloadstatus = 0
        slotstatus = 0
        iscomputer = 0
        team = 0
        color = 0
        race = 0
        computerType = 0
        handicap = 0
    End Sub
    Public Sub New(ByVal ID As Byte, ByVal slot As Byte())
        SID = ID
        If slot.Length = 9 Then
            PID = slot(0)
            downloadstatus = slot(1)
            slotstatus = slot(2)
            iscomputer = slot(3)
            team = slot(4)
            color = slot(5)
            race = slot(6)
            computerType = slot(7)
            handicap = slot(8)
        Else
            PID = 255
            downloadstatus = 0
            slotstatus = 0
            iscomputer = 0
            team = 0
            color = 0
            race = 0
            computerType = 0
            handicap = 0
        End If
    End Sub
    Public Sub New(ByVal SID As Byte, ByVal PID As Byte, ByVal downloadstatus As Byte, ByVal slotstatus As Byte, ByVal iscomputer As Byte, ByVal team As Byte, ByVal color As Byte, ByVal race As Byte)
        Me.SID = SID
        Me.PID = PID
        Me.downloadstatus = downloadstatus
        Me.slotstatus = slotstatus
        Me.iscomputer = iscomputer
        Me.team = team
        Me.color = color
        Me.race = race
        Me.computerType = 1
        Me.handicap = 100
    End Sub

    Public Function GetSID() As Byte
        Return SID
    End Function
    Public Function GetIsComputer() As Byte
        Return iscomputer
    End Function
    Public Function GetSlotStatus() As Byte
        Return slotstatus
    End Function
    Public Function GetPID() As Byte
        Return PID
    End Function
    Public Function GetTeam() As Byte
        Return team
    End Function
    Public Function GetRace() As Byte
        Return race
    End Function
    Public Function GetColor() As Byte
        Return color
    End Function
    Public Function GetComputerType() As Byte
        Return computerType
    End Function
    Public Function GetByteArray() As Byte()
        Return New Byte() {PID, downloadstatus, slotstatus, iscomputer, team, color, race, computerType, handicap}
    End Function

End Class

'MrJag|0.8c|ping|class to sort players by ping
Public Class clsPlayerPingComparer
    Implements IComparer

    Public Function Compare(ByVal one As Object, ByVal two As Object) As Integer Implements IComparer.Compare
        Dim onePlayer As clsHostPlayer = DirectCast(one, clsHostPlayer)
        Dim twoPlayer As clsHostPlayer = DirectCast(two, clsHostPlayer)
        Return CInt(onePlayer.GetPing() - twoPlayer.GetPing())
    End Function
End Class

Public Class clsHostPlayer
    Private PID As Byte
    Private name As String
    Private sock As clsSocketTCPClient
    Private internalIP As Byte()
    Private externalIP As Byte()
    Private pingValues As ArrayList 'MrJag|0.8c|ping|array to store historical ping data in
    Private finishedLoading As Boolean

    Private WithEvents whoisTimer As Timers.Timer
    Public Event EventSpoofCheck(ByVal name As String)

    Public Sub New()
        Me.PID = 255
        Me.name = ""
        Me.sock = New clsSocketTCPClient
        Me.externalIP = New Byte() {}
        Me.internalIP = New Byte() {}
        Me.pingValues = New ArrayList   'MrJag|0.8c|ping|array to store historical ping data in
        Me.finishedLoading = False

        whoisTimer = New Timers.Timer
    End Sub
    Public Sub New(ByVal pID As Byte, ByVal name As String, ByVal sock As clsSocketTCPClient, ByVal externalIP As Byte(), ByVal internalIP As Byte())
        Me.PID = pID
        Me.name = name
        Me.sock = sock
        Me.externalIP = externalIP
        Me.internalIP = internalIP
        Me.pingValues = New ArrayList   'MrJag|0.8c|ping|array to store historical ping data in
        Me.finishedLoading = False

        whoisTimer = New Timers.Timer
        whoisTimer.Interval = 1000 'ms
        whoisTimer.Start()
    End Sub

    Public Function isLoaded() As Boolean
        Return finishedLoading
    End Function
    Public Sub mapLoaded()
        finishedLoading = True
    End Sub
    Public Function GetPID() As Byte
        Return PID
    End Function

    Public Function GetName() As String
        Return name
    End Function

    Public Function GetSock() As clsSocketTCPClient
        Return sock
    End Function

    Public Function GetInternalIP() As Byte()
        Return internalIP
    End Function

    Public Function GetExternalIP() As Byte()
        Return externalIP
    End Function

    Public Function GetPing() As Long
        Dim retVal As Long = 0
        Dim pingArray As ArrayList = pingValues 'make a copy of the data
        Dim mean As Long = 0                    'average of entire array
        Dim sum As Long = 0                     'sigma of (value - mean)^2
        Dim std As Long = 0                     'standard deviation, sqrt(sum/n-1)
        Dim counter As Integer = 0

        If pingArray.Count = 0 Then
            Return -1                       'no ping data
        ElseIf pingArray.Count < 10 Then
            Return -1
        End If

        'Calculate the average ping
        For index = 0 To pingArray.Count - 1
            mean = mean + CLng(pingArray.Item(index))
        Next
        mean = CLng(mean / (pingArray.Count + 1))

        'Calculate the standard deviation
        For index = 0 To pingArray.Count - 1
            sum = sum + CLng((CLng(pingArray.Item(index)) - mean) ^ 2)
        Next
        std = CLng(Math.Sqrt(CLng(sum / (pingArray.Count - 1))))

        For index = 0 To pingArray.Count - 1
            If CLng(pingArray.Item(index)) >= (mean - std) AndAlso CLng(pingArray.Item(index)) <= (mean + std) Then
                counter += 1
                retVal = retVal + CLng(pingArray.Item(index))
            End If
        Next
        retVal = CLng(retVal / counter)

        If frmLainEthLite.data.botSettings.enable_LCPings = True Then
            retVal = CLng(retVal / 2) ' (divide by 2) to match LC style pings
        End If

        Return retVal
    End Function
    'MrJag|0.8c|ping|function to add to the ping data
    Public Function SetPing(ByVal ping As Long) As Boolean
        If pingValues.Count > 20 Then
            pingValues.RemoveAt(0)
        End If
        Me.pingValues.Add(ping)
        Return True
    End Function

    Public Sub SpoofCheck()
        whoisTimer.Start()
    End Sub
    Private Sub whoisTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles whoisTimer.Elapsed
        Static isRunning As Boolean = False
        Try
            If isRunning = False Then
                isRunning = True
                whoisTimer.Stop()
                RaiseEvent EventSpoofCheck(GetName)
                isRunning = False
            End If
        Catch ex As Exception
        End Try
    End Sub



End Class
#End Region


Public Class clsProtocolHost
    Private Const GAME_PRIVATE As Integer = 17  'MrJag|0.8c|gamestate| needed to test game states
    Private Const GAME_PUBLIC As Integer = 16
    Private hashSlot As Hashtable       'SID -> SLOT
    Private hashPlayer As Hashtable     'PID -> PLAYER
    Private hostName As String
    Private gameName As String
    Private numPlayers As Integer
    Private virtualHostName As String
    Private callerName As String

    Public Enum Protocol As Byte
        W3GS_PING_FROM_HOST = 1         '0x01
        W3GS_SLOTINFOJOIN = 4           '0x04
        W3GS_REJECTJOIN = 5             '0x05
        W3GS_PLAYERINFO = 6             '0x06
        W3GS_PLAYERLEAVE_OTHERS = 7     '0x07
        W3GS_GAMELOADED_OTHERS = 8      '0x08
        W3GS_SLOTINFO = 9               '0x09
        W3GS_COUNTDOWN_START = 10       '0x0A
        W3GS_COUNTDOWN_END = 11         '0x0B
        W3GS_INCOMING_ACTION = 12       '0x0C
        W3GS_CHAT_FROM_HOST = 15        '0x0F

        W3GS_START_LAG = 16             '0x10
        W3GS_STOP_LAG = 17              '0x11

        W3GS_HOST_KICK_PLAYER = 28      '0x1C
        W3GS_REQJOIN = 30               '0x1E
        W3GS_LEAVEGAME = 33             '0x21
        W3GS_GAMELOADED_SELF = 35       '0x23
        W3GS_OUTGOING_ACTION = 38       '0x26
        W3GS_OUTGOING_KEEPALIVE = 39    '0x27
        W3GS_CHAT_TO_HOST = 40          '0x28
        W3GS_CHAT_OTHERS = 52           '0x34
        W3GS_PING_FROM_OTHERS = 53      '0x35
        W3GS_PONG_TO_OTHERS = 54        '0x36
        W3GS_MAPCHECK = 61              '0x3D
        W3GS_MAPSIZE = 66               '0x42
        W3GS_PONG_TO_HOST = 70          '0x46
    End Enum

    Public Event EventHostChat(ByVal fromPID As Byte, ByVal toPIDs As Byte(), ByVal flag As Byte, ByVal flagextra As Byte(), ByVal msg As String)
    Public Event EventHostTeam(ByVal fromPID As Byte, ByVal toPIDs As Byte(), ByVal flag As Byte, ByVal team As Byte)
    Public Event EventAction(ByVal fromPID As Byte, ByVal actionCRC As Byte(), ByVal actionData As Byte())


    Private Function CreateNewPlayer(ByVal playerName As String, ByVal sock As clsSocketTCPClient, ByVal externalIP As Byte(), ByVal internalIP As Byte()) As clsHostPlayer
        Dim i As Byte
        Try
            SyncLock hashPlayer.SyncRoot
                For i = 1 To 200
                    If hashPlayer.Contains(i) = False Then
                        Return New clsHostPlayer(i, playerName, sock, externalIP, internalIP)
                    End If
                Next
            End SyncLock

            Return New clsHostPlayer
        Catch ex As Exception
            Return New clsHostPlayer
        End Try
    End Function
    Private Function CreateHost() As Boolean
        'Dim slot As clsHostSlot
        Dim player As clsHostPlayer
        Try

            'slot = CType(hashSlot.Item(GetEmptySlot(12)), clsHostSlot)
            'player = CreateNewPlayer(hostName, New clsSocketTCPClient, New Byte() {0, 0, 0, 0}, New Byte() {0, 0, 0, 0})
            'player = CreateNewPlayer("|cFF4080C0Host", New clsSocketTCPClient, New Byte() {0, 0, 0, 0}, New Byte() {0, 0, 0, 0})

            'player = CreateNewPlayer("|cFF000000", New clsSocketTCPClient, New Byte() {0, 0, 0, 0}, New Byte() {0, 0, 0, 0})
            player = CreateNewPlayer(virtualHostName, New clsSocketTCPClient, New Byte() {0, 0, 0, 0}, New Byte() {0, 0, 0, 0})

            'player = New clsHostPlayer(GetHostPID, "|cFF4080C0Host", New clsSocketTCPClient, New Byte() {0, 0, 0, 0}, New Byte() {0, 0, 0, 0})

            'hashSlot.Item(slot.GetSID) = New clsHostSlot(slot.GetSID, New Byte() {player.GetPID, 100, 2, slot.GetIsComputer, slot.GetTeam, slot.GetColor, slot.GetRace, slot.GetComputerType, 100})
            hashPlayer.Add(player.GetPID, player)

            Return True
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return False
        End Try
    End Function
    'MrJag
    Public Function RemoveHost() As Boolean
        Try
            If GetPlayerFromPID(GetHostPID).GetName = virtualHostName Then
                Debug.WriteLine(String.Format("Removing PID:{0} Name:{1}", GetHostPID, GetPlayerFromPID(GetHostPID).GetName))
                For Each player In GetPlayerList()
                    Debug.WriteLine(String.Format("Telling {0}:{2} that {1} left the game", player.GetName, virtualHostName, player.GetPID))
                    player.GetSock.Send(SEND_W3GS_PLAYERLEAVE_OTHERS(GetHostPID))
                Next
                hashPlayer.Remove(GetHostPID)
            End If
            Return True
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return False
        End Try
    End Function
    Private Function CreateDotaSlotStrucure() As Hashtable
        Dim hash As Hashtable

        Try
            hash = Hashtable.Synchronized(New Hashtable)

            'pid, download, slot open|close, is computer, team, color, race, AI level, handicap
            hash.Add(CByte(0), New clsHostSlot(0, New Byte() {0, 100, 0, 0, 0, 1, 4, 1, 100})) 'sentinel slots
            hash.Add(CByte(1), New clsHostSlot(1, New Byte() {0, 100, 0, 0, 0, 2, 4, 1, 100}))
            hash.Add(CByte(2), New clsHostSlot(2, New Byte() {0, 100, 0, 0, 0, 3, 4, 1, 100}))
            hash.Add(CByte(3), New clsHostSlot(3, New Byte() {0, 100, 0, 0, 0, 4, 4, 1, 100}))
            hash.Add(CByte(4), New clsHostSlot(4, New Byte() {0, 100, 0, 0, 0, 5, 4, 1, 100}))
            hash.Add(CByte(5), New clsHostSlot(5, New Byte() {0, 100, 0, 0, 1, 7, 8, 1, 100})) 'scourge slots
            hash.Add(CByte(6), New clsHostSlot(6, New Byte() {0, 100, 0, 0, 1, 8, 8, 1, 100}))
            hash.Add(CByte(7), New clsHostSlot(7, New Byte() {0, 100, 0, 0, 1, 9, 8, 1, 100}))
            hash.Add(CByte(8), New clsHostSlot(8, New Byte() {0, 100, 0, 0, 1, 10, 8, 1, 100}))
            hash.Add(CByte(9), New clsHostSlot(9, New Byte() {0, 100, 0, 0, 1, 11, 8, 1, 100}))

            If numPlayers = 12 Then
                hash.Add(CByte(10), New clsHostSlot(10, New Byte() {0, 100, 1, 0, 12, 12, 96, 1, 100}))    'default to close
                hash.Add(CByte(11), New clsHostSlot(11, New Byte() {0, 100, 1, 0, 12, 12, 96, 1, 100}))    'default to close
            End If
            'hash.Add(CByte(12), New clsHostSlot(12, New Byte() {0, 100, 1, 0, 12, 12, 96, 1, 100}))    'open obs slot

            Return hash

        Catch ex As Exception
            Debug.WriteLine(ex)
            Return New Hashtable
        End Try
    End Function

    Public Sub New(ByVal hostName As String, ByVal gameName As String, ByVal numPlayers As Integer, ByVal callerName As String)
        Me.hostName = hostName
        Me.gameName = gameName
        Me.numPlayers = numPlayers
        Me.virtualHostName = "|cFF4080C0Host"
        Me.callerName = callerName

        hashSlot = Hashtable.Synchronized(New Hashtable)
        hashPlayer = Hashtable.Synchronized(New Hashtable)

        hashSlot = CreateDotaSlotStrucure()

        If numPlayers < 12 Then
            CreateHost()
        End If
    End Sub

    Public Function SwapSlot(ByVal SID1 As Byte, ByVal SID2 As Byte) As Boolean
        Dim slot1original As clsHostSlot
        Dim slot2original As clsHostSlot
        Dim slot1new As clsHostSlot
        Dim slot2new As clsHostSlot

        Try
            slot1original = CType(hashSlot.Item(SID1), clsHostSlot)
            slot2original = CType(hashSlot.Item(SID2), clsHostSlot)

            slot1new = New clsHostSlot(slot1original.GetSID, slot2original.GetPID, 100, slot2original.GetSlotStatus, slot2original.GetIsComputer, slot1original.GetTeam, slot1original.GetColor, slot1original.GetRace)
            slot2new = New clsHostSlot(slot2original.GetSID, slot1original.GetPID, 100, slot1original.GetSlotStatus, slot1original.GetIsComputer, slot2original.GetTeam, slot2original.GetColor, slot2original.GetRace)

            hashSlot.Item(SID1) = slot1new
            hashSlot.Item(SID2) = slot2new

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function GetPlayerSlot(ByVal PID As Byte) As clsHostSlot
        Dim slot As clsHostSlot

        Try
            For Each slot In GetSlotList()
                If slot.GetPID = PID Then
                    Return slot
                End If
            Next
            Return New clsHostSlot
        Catch ex As Exception
            Return New clsHostSlot
        End Try
    End Function

    Public Function GetSlotList() As clsHostSlot()
        Dim list As ArrayList
        Dim SID As Byte
        Dim slot As clsHostSlot
        Try
            list = New ArrayList

            SyncLock hashSlot.SyncRoot
                For Each SID In hashSlot.Keys
                    slot = CType(hashSlot.Item(SID), clsHostSlot)
                    list.Add(slot)
                Next
                list.Reverse()
            End SyncLock

            Return CType(list.ToArray(GetType(clsHostSlot)), clsHostSlot())
        Catch ex As Exception
            Return New clsHostSlot() {}
        End Try
    End Function
    Public Function GetSlotInfo() As Byte()
        Dim list As ArrayList
        Dim slot As clsHostSlot
        Try
            list = New ArrayList
            clsHelper.AddByteArray(list, New Byte() {CType(hashSlot.Keys.Count, Byte)})               'slot count

            For Each slot In GetSlotList()
                clsHelper.AddByteArray(list, slot.GetByteArray)     'slot array
            Next

            clsHelper.AddByteArray(list, clsHelper.LongToDWORD(Environment.TickCount, False))         'GetTickCount
            clsHelper.AddByteArray(list, New Byte() {3})            'not sure
            clsHelper.AddByteArray(list, New Byte() {10})           'not sure
            Return CType(list.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function GetHostPID() As Byte
        SyncLock hashPlayer.SyncRoot
            For Each player In GetPlayerList()
                If player.GetName = virtualHostName Then
                    Return player.GetPID
                End If
            Next
            For Each player In GetPlayerList()
                If player.GetName = callerName Then
                    Return player.GetPID
                End If
            Next
            For Each player In GetPlayerList()
                Return player.GetPID
            Next
        End SyncLock
        Return 1
    End Function
    Public Function GetHostSID() As Byte
        Return GetPlayerSlot(GetHostPID()).GetSID
    End Function

    Public Function GetPlayerFromSocket(ByVal client As clsSocketTCPClient) As clsHostPlayer
        Dim PID As Byte
        Dim player As clsHostPlayer
        Try
            SyncLock hashPlayer.SyncRoot
                For Each PID In hashPlayer.Keys
                    player = CType(hashPlayer.Item(PID), clsHostPlayer)
                    If player.GetSock Is client Then
                        Return player
                    End If
                Next
            End SyncLock
            Return New clsHostPlayer
        Catch ex As Exception
            Return New clsHostPlayer
        End Try
    End Function
    Public Function GetPlayerList(ByVal excludePID As Byte) As clsHostPlayer()
        Dim PID As Byte
        Dim list As ArrayList
        Try
            list = New ArrayList
            SyncLock hashPlayer.SyncRoot
                For Each PID In hashPlayer.Keys
                    If PID <> excludePID Then
                        list.Add(hashPlayer.Item(PID))
                    End If
                Next
            End SyncLock
            Return CType(list.ToArray(GetType(clsHostPlayer)), clsHostPlayer())
        Catch ex As Exception
            Return New clsHostPlayer() {}
        End Try
    End Function

    Public Function GetPlayerList() As clsHostPlayer()
        Return GetPlayerList(255)
    End Function
    'MrJag|0.8c|function|returns class player from player name
    Public Function GetPlayerFromName(ByVal playerName As String) As clsHostPlayer
        Dim player As clsHostPlayer

        Try
            SyncLock hashPlayer.SyncRoot
                For Each PID In hashPlayer.Keys
                    player = CType(hashPlayer.Item(PID), clsHostPlayer)
                    If player.GetName.ToLower = playerName.ToLower Then
                        'If player.GetName Is player Then
                        Return player
                    End If
                Next
            End SyncLock
            Return New clsHostPlayer
        Catch ex As Exception
            Return New clsHostPlayer
        End Try
    End Function
    Public Function GetPlayerFromPID(ByVal PID As Byte) As clsHostPlayer
        Try
            If hashPlayer.Contains(PID) Then
                Return CType(hashPlayer.Item(PID), clsHostPlayer)
            End If
            Return New clsHostPlayer
        Catch ex As Exception
            Return New clsHostPlayer
        End Try
    End Function
    Public Function GetPlayerFromSID(ByVal SID As Byte) As clsHostPlayer
        Dim slot As clsHostSlot
        Try
            If hashSlot.Contains(SID) Then
                slot = CType(hashSlot.Item(SID), clsHostSlot)
                Return GetPlayerFromPID(slot.GetPID())
            End If
            Return New clsHostPlayer
        Catch ex As Exception
            Return New clsHostPlayer
        End Try
    End Function

    Public Function GetPlayerCount() As Integer
        Return GetPlayerCount(255)
    End Function

    Public Function GetPlayerCountTeam(ByVal team As Byte) As Integer
        Dim slot As clsHostSlot
        Dim total As Integer
        Try
            total = 0
            For Each slot In GetSlotList()
                If slot.GetPID <> 255 AndAlso slot.GetPID <> 0 AndAlso slot.GetTeam = team Then
                    total = total + 1
                End If
            Next
            Return total
        Catch ex As Exception
            Return 0
        End Try
    End Function
    Public Function GetPlayerCount(ByVal excludePID As Byte) As Integer
        Dim slot As clsHostSlot
        Dim total As Integer
        Try
            total = 0
            For Each slot In GetSlotList()
                If slot.GetPID <> 255 AndAlso slot.GetPID <> 0 AndAlso slot.GetPID <> excludePID Then
                    total = total + 1
                End If
            Next
            Return total
        Catch ex As Exception
            Return 0
        End Try
    End Function
    Public Function GetPIDList(ByVal excludePID As Byte) As Byte()
        Dim PID As Byte
        Dim list As ArrayList

        Try
            list = New ArrayList
            SyncLock hashPlayer.SyncRoot
                For Each PID In hashPlayer.Keys
                    If PID <> excludePID Then
                        list.Add(PID)
                    End If
                Next
            End SyncLock
            Return CType(list.ToArray(GetType(Byte)), Byte())
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function GetPIDList() As Byte()
        Return GetPIDList(255)
    End Function
    Public Function GetEmptySlot() As Byte
        Dim slot As clsHostSlot

        Try
            For Each slot In GetSlotList()
                If slot.GetPID = 0 AndAlso slot.GetSlotStatus = 0 Then
                    Return slot.GetSID
                End If
            Next

            Return 255
        Catch ex As Exception
            Return 255
        End Try
    End Function
    Public Function GetEmptySlot(ByVal team As Byte) As Byte
        Dim slot As clsHostSlot

        Try
            For Each slot In GetSlotList()
                If slot.GetPID = 0 AndAlso slot.GetSlotStatus = 0 AndAlso slot.GetTeam = team Then
                    Return slot.GetSID
                End If
            Next

            Return 255
        Catch ex As Exception
            Return 255
        End Try
    End Function
    'MrJag|0.8c|function|overloads the GetEmptySlot function to fix the slot change bug.
    'it finds the next empty slot based on the current player position
    Public Function GetEmptySlot(ByVal pid As Byte, ByVal team As Byte) As Byte
        Dim slot As clsHostSlot
        Dim SID As Byte
        Try
            'locate sid for the current player
            For Each slot In GetSlotList()
                If slot.GetPID = pid Then
                    SID = slot.GetSID ' start searching for an open slot from the current players slot position
                    If team <> slot.GetTeam Then
                        If team = 0 Then
                            SID = 0 'if the player is switching to the sentinel team, start seaching with slot 0
                        ElseIf team = 1 Then
                            SID = 5 'if the player is switching to the scorge team, start seaching with slot 1
                        ElseIf team = 12 Then
                            SID = 10 'if the player is switching to observer team, start seaching with slot 10
                        End If
                    ElseIf SID = CByte(4) Then
                        SID = CByte(0) 'if the player is staying on the sentinel but already in the last slot, start with the 1st slot
                    ElseIf SID = CByte(9) Then
                        SID = CByte(5)  'if the player is staying on the scourge but already in the last slot, start with the 6th slot
                    ElseIf SID = CByte(11) Then
                        SID = CByte(10)  'if the player is staying on the observer team but already in the last slot, start with the 11th slot
                    End If
                End If
            Next

            For Each slot In GetSlotList()
                If slot.GetSID >= SID And slot.GetPID = 0 AndAlso slot.GetSlotStatus = 0 AndAlso slot.GetTeam = team Then
                    Return slot.GetSID
                End If
            Next

            Return 255
        Catch ex As Exception
            Return 255
        End Try
    End Function

    Public Function GetReserveSlot(ByVal reserveList As ArrayList) As Byte
        Dim SID As Byte
        Dim slot As clsHostSlot
        Try
            SyncLock hashSlot.SyncRoot
                'first try to find an empty slot
                For Each SID In hashSlot.Keys 'MrJag|0.8c|observer| 11
                    slot = CType(hashSlot.Item(SID), clsHostSlot)
                    If slot.GetSlotStatus = 0 Then
                        Return SID
                    End If
                Next
                'next try to find a closed slot
                For Each SID In hashSlot.Keys 'MrJag|0.8c|observer| 11
                    slot = CType(hashSlot.Item(SID), clsHostSlot)
                    If slot.GetSlotStatus = 1 Then
                        Return SID
                    End If
                Next
            End SyncLock
            'lastly, try to find a non reserved player to kick
            For Each SID In hashSlot.Keys 'MrJag|0.8c|observer| 11
                If reserveList.Contains(GetPlayerFromSID(SID).GetName.ToLower) Then
                    'do nothing
                Else
                    'MsgBox(String.Format("{0} is not on the reservation list.", GetPlayerFromSID(SID).GetName))
                    Return SID
                End If
            Next
            Return 255
        Catch ex As Exception
            Return 255
        End Try
    End Function
    Public Function SlotComputer(ByVal SID As Byte, ByVal skill As Byte) As Boolean
        Dim slot As clsHostSlot
        Try
            If hashSlot.Contains(SID) Then
                slot = CType(hashSlot.Item(SID), clsHostSlot)
                hashSlot(slot.GetSID) = New clsHostSlot(slot.GetSID, New Byte() {0, 100, 0, 1, slot.GetTeam, slot.GetColor, slot.GetRace, skill, 100})
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function SlotOpenClose(ByVal SID As Byte, ByVal open As Boolean) As Boolean
        Dim slot As clsHostSlot
        Try

            If hashSlot.Contains(SID) Then
                slot = CType(hashSlot.Item(SID), clsHostSlot)
                If open Then
                    hashSlot(slot.GetSID) = New clsHostSlot(slot.GetSID, New Byte() {0, 100, 0, slot.GetIsComputer, slot.GetTeam, slot.GetColor, slot.GetRace, slot.GetComputerType, 100})
                Else
                    hashSlot(slot.GetSID) = New clsHostSlot(slot.GetSID, New Byte() {0, 100, 1, slot.GetIsComputer, slot.GetTeam, slot.GetColor, slot.GetRace, slot.GetComputerType, 100})
                End If
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function PlayerTeamChange(ByVal PID As Byte, ByVal team As Byte) As Boolean
        Dim newSID As Byte
        Dim slot As clsHostSlot
        Try
            If hashPlayer.Contains(PID) Then
                'newSID = GetEmptySlot(team)
                newSID = GetEmptySlot(PID, team) 'MrJag|0.8c|function|use new overloaded function to fix the slot change bug
                If newSID <> 255 Then
                    For Each slot In GetSlotList()
                        If slot.GetPID = PID Then
                            hashSlot(slot.GetSID) = New clsHostSlot(slot.GetSID, New Byte() {0, 100, 0, slot.GetIsComputer, slot.GetTeam, slot.GetColor, slot.GetRace, slot.GetComputerType, 100})
                        End If
                    Next

                    slot = CType(hashSlot.Item(newSID), clsHostSlot)
                    hashSlot(slot.GetSID) = New clsHostSlot(slot.GetSID, New Byte() {PID, 100, 2, slot.GetIsComputer, slot.GetTeam, slot.GetColor, slot.GetRace, slot.GetComputerType, 100})
                    Return True
                End If
            End If

            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function PlayerRemove(ByVal PID As Byte, ByVal slotOpen As Boolean) As Byte
        Dim slot As clsHostSlot
        Try
            If hashPlayer.Contains(PID) Then
                hashPlayer.Remove(PID)

                For Each slot In GetSlotList()
                    If slot.GetPID = PID Then
                        SlotOpenClose(slot.GetSID, slotOpen)
                    End If
                Next

                Return PID
            End If
            Return 255
        Catch ex As Exception
            Return 255
        End Try
    End Function
    Public Function PlayerRemove(ByVal sock As clsSocketTCPClient, ByVal slotOpen As Boolean) As Byte
        Dim player As clsHostPlayer
        Dim PID As Byte
        Try
            SyncLock hashPlayer.SyncRoot
                For Each PID In hashPlayer.Keys
                    player = CType(hashPlayer.Item(PID), clsHostPlayer)
                    If player.GetSock Is sock Then
                        Return PlayerRemove(PID, slotOpen)
                    End If
                Next
            End SyncLock

            Return 255
        Catch ex As Exception
            Return 255
        End Try
    End Function
    Public Function PlayerAdd(ByVal playerName As String, ByVal sock As clsSocketTCPClient, ByVal externalIP As Byte(), ByVal internalIP As Byte()) As Byte
        Dim player As clsHostPlayer
        Dim SID As Byte
        Dim slot As clsHostSlot
        Try
            player = CreateNewPlayer(playerName, sock, externalIP, internalIP)
            If player.GetPID <> 255 AndAlso hashPlayer.Contains(player.GetPID) = False Then

                SID = GetEmptySlot()
                If SID <> 255 Then
                    slot = CType(hashSlot.Item(SID), clsHostSlot)
                    hashSlot(slot.GetSID) = New clsHostSlot(slot.GetSID, New Byte() {player.GetPID, 100, 2, slot.GetIsComputer, slot.GetTeam, slot.GetColor, slot.GetRace, slot.GetComputerType, 100})
                    hashPlayer.Add(player.GetPID, player)
                    Return player.GetPID
                End If
            End If

            Return 255
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return 255
        End Try
    End Function


#Region "protocol"
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

    Public Function RECEIVE_W3GS_REQJOIN(ByVal data As Byte(), ByVal socket As clsSocketTCPClient) As clsHostPlayer
        Dim list As ArrayList
        Dim currentbyte As Byte
        Dim name As String
        Dim i As Integer
        Dim externalIP As Byte()
        Try
            If ValidateLength(data) = True Then
                i = 19
                list = New ArrayList
                Do
                    currentbyte = data(i)
                    list.Add(currentbyte)
                    i = i + 1
                    If currentbyte = 0 Then
                        name = clsHelper.ByteArrayToStringASCII(CType(list.ToArray(GetType(Byte)), Byte()))
                        Exit Do
                    End If
                Loop

                i = i + 6
                externalIP = New Byte(4 - 1) {}
                Array.Copy(data, i, externalIP, 0, 4)

                If name.Length > 0 Then
                    Return New clsHostPlayer(255, name, socket, socket.GetRemoteIP, externalIP)
                End If
            End If
            Return New clsHostPlayer
        Catch ex As Exception
            Return New clsHostPlayer
        End Try
    End Function
    Public Function RECEIVE_W3GS_MAPSIZE(ByVal data As Byte(), ByVal mapSize As Byte()) As Boolean
        Dim i As Integer
        Try
            If ValidateLength(data) = True Then
                i = 8
                If 1 = data(i) AndAlso mapSize.Length = 4 Then
                    If data(i + 1) = mapSize(0) AndAlso data(i + 2) = mapSize(1) AndAlso data(i + 3) = mapSize(2) AndAlso data(i + 4) = mapSize(3) Then
                        Return True
                    End If
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_W3GS_LEAVEGAME(ByVal data As Byte()) As Boolean
        Try
            If ValidateLength(data) = True Then
                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_W3GS_PONG_TO_HOST(ByVal data As Byte()) As Long
        Dim pong As Byte()
        Try
            If ValidateLength(data) = True AndAlso data.Length >= 4 Then
                pong = New Byte(4 - 1) {}
                Array.Copy(data, data.Length - 4, pong, 0, 4)

                'Debug.WriteLine(Environment.TickCount - clsHelper.ByteArrayToLong(pong))
                Return Environment.TickCount - clsHelper.ByteArrayToLong(pong)
            End If
            Return -1
        Catch ex As Exception
            Return -1
        End Try
    End Function
    Public Function RECEIVE_W3GS_OUTGOING_KEEPALIVE(ByVal data As Byte()) As Boolean
        Try
            If ValidateLength(data) = True Then
                If data.Length = 9 Then
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_W3GS_GAMELOADED_SELF(ByVal data As Byte()) As Boolean
        Try
            If ValidateLength(data) = True Then
                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_W3GS_OUTGOING_ACTION(ByVal data As Byte(), ByVal PID As Byte) As Boolean
        Dim CRC As Byte()
        Dim action As Byte()
        Try
            If ValidateLength(data) = True AndAlso data.Length >= 8 Then
                If PID <> 255 Then
                    CRC = New Byte(4 - 1) {}
                    Array.Copy(data, 4, CRC, 0, 4)

                    action = New Byte(data.Length - 8 - 1) {}
                    Array.Copy(data, 8, action, 0, action.Length)

                    RaiseEvent EventAction(PID, CRC, action)
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function RECEIVE_W3GS_CHAT_TO_HOST(ByVal data As Byte()) As Boolean
        Dim i As Integer
        Dim j As Integer
        Dim counter As Integer
        Dim fromPID As Byte
        Dim toPIDs As Byte()
        Dim flag As Byte
        Dim flagextra As Byte()
        Dim msg As String
        Dim team As Byte
        Dim list As ArrayList
        Dim currentbyte As Byte
        Try
            If ValidateLength(data) = True Then
                i = 4
                counter = data(i)

                list = New ArrayList
                For j = 1 To counter
                    i = i + 1
                    list.Add(data(i))
                Next
                toPIDs = CType(list.ToArray(GetType(Byte)), Byte())

                i = i + 1
                fromPID = data(i)

                i = i + 1
                flag = data(i)

                i = i + 1
                Select Case flag
                    Case Is = 16
                        list = New ArrayList
                        Do
                            currentbyte = data(i)
                            list.Add(currentbyte)
                            i = i + 1
                            If currentbyte = 0 Then
                                msg = clsHelper.ByteArrayToStringASCII(CType(list.ToArray(GetType(Byte)), Byte()))
                                RaiseEvent EventHostChat(fromPID, toPIDs, flag, New Byte() {}, msg)
                                Return True
                            End If
                        Loop
                    Case Is = 17
                        team = data(i)
                        RaiseEvent EventHostTeam(fromPID, toPIDs, flag, team)
                        Return True
                    Case Is = 32
                        flagextra = New Byte(4 - 1) {}
                        Array.Copy(data, i, flagextra, 0, 4)
                        i = i + 4

                        list = New ArrayList
                        Do
                            currentbyte = data(i)
                            list.Add(currentbyte)
                            i = i + 1
                            If currentbyte = 0 Then
                                msg = clsHelper.ByteArrayToStringASCII(CType(list.ToArray(GetType(Byte)), Byte()))
                                RaiseEvent EventHostChat(fromPID, toPIDs, flag, flagextra, msg)
                                Return True
                            End If
                        Loop
                        Return True

                End Select
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function SEND_W3GS_SLOTINFOJOIN(ByVal PID As Byte, ByVal slotInfo As Byte()) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Dim player As clsHostPlayer

        Try
            player = GetPlayerFromPID(PID)

            If PID <> 255 AndAlso slotInfo.Length > 0 Then
                packet = New ArrayList

                clsHelper.AddByteArray(packet, New Byte() {247})                        'W3GS header constant
                clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_SLOTINFOJOIN}) 'W3GS_SLOTINFOJOIN
                clsHelper.AddByteArray(packet, New Byte() {0, 0})                       'undefined packet length
                clsHelper.AddByteArray(packet, clsHelper.IntegerToWORD(slotInfo.Length, False)) 'slot info length
                clsHelper.AddByteArray(packet, slotInfo)
                clsHelper.AddByteArray(packet, New Byte() {PID})
                clsHelper.AddByteArray(packet, New Byte() {2, 0})           'sockaddr_in structure
                clsHelper.AddByteArray(packet, New Byte() {0, 0})           'port
                clsHelper.AddByteArray(packet, player.GetExternalIP)        'player ip
                clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})
                clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})

                buffer = CType(packet.ToArray(GetType(Byte)), Byte())
                AssignLength(buffer)
                Return buffer
            End If


            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_W3GS_PLAYERINFO(ByVal PID As Byte) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Dim slotinfo As Byte()
        Dim player As clsHostPlayer


        Try
            player = GetPlayerFromPID(PID)

            If PID <> 255 AndAlso player Is Nothing = False Then
                slotinfo = GetSlotInfo()
                packet = New ArrayList

                clsHelper.AddByteArray(packet, New Byte() {247})                        'W3GS header constant
                clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_PLAYERINFO})   'W3GS_PLAYERINFO
                clsHelper.AddByteArray(packet, New Byte() {0, 0})                       'undefined packet length
                clsHelper.AddByteArray(packet, New Byte() {2, 0, 0, 0})         'player join counter
                clsHelper.AddByteArray(packet, New Byte() {PID})                'player PID
                clsHelper.AddByteArray(packet, player.GetName())                'player name
                clsHelper.AddByteArray(packet, New Byte() {1, 0})               'unknown

                clsHelper.AddByteArray(packet, New Byte() {2, 0})               'sockaddr_in structure
                clsHelper.AddByteArray(packet, New Byte() {0, 0})               'port
                clsHelper.AddByteArray(packet, player.GetExternalIP)            'external ip
                clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})
                clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})

                clsHelper.AddByteArray(packet, New Byte() {2, 0})               'sockaddr_in structure
                clsHelper.AddByteArray(packet, New Byte() {0, 0})               'port
                clsHelper.AddByteArray(packet, player.GetInternalIP)            'internal ip
                clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})
                clsHelper.AddByteArray(packet, New Byte() {0, 0, 0, 0})

                buffer = CType(packet.ToArray(GetType(Byte)), Byte())
                AssignLength(buffer)
                Return buffer
            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_W3GS_SLOTINFO() As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Dim slotinfo As Byte()

        Try
            slotinfo = GetSlotInfo()
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {247})                        'W3GS header constant
            clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_SLOTINFO})     'W3GS_SLOTINFO
            clsHelper.AddByteArray(packet, New Byte() {0, 0})                       'undefined packet length

            clsHelper.AddByteArray(packet, clsHelper.IntegerToWORD(slotinfo.Length, False)) 'slot info length
            clsHelper.AddByteArray(packet, slotinfo)

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer

            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_W3GS_MAPCHECK(ByVal mapPath As String, ByVal mapSize As Byte(), ByVal mapInfo As Byte(), ByVal mapCRC As Byte()) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()

        Try
            If mapPath.Length > 0 AndAlso mapSize.Length = 4 AndAlso mapInfo.Length = 4 AndAlso mapCRC.Length = 4 Then
                packet = New ArrayList
                clsHelper.AddByteArray(packet, New Byte() {247})                        'W3GS header constant
                clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_MAPCHECK})     'W3GS_MAPCHECK
                clsHelper.AddByteArray(packet, New Byte() {0, 0})                       'undefined packet length

                clsHelper.AddByteArray(packet, New Byte() {1, 0, 0, 0})         'always 1,0,0,0
                clsHelper.AddByteArray(packet, mapPath)
                clsHelper.AddByteArray(packet, mapSize)
                clsHelper.AddByteArray(packet, mapInfo)
                clsHelper.AddByteArray(packet, mapCRC)

                buffer = CType(packet.ToArray(GetType(Byte)), Byte())
                AssignLength(buffer)
                Return buffer
            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_W3GS_PLAYERLEAVE_OTHERS(ByVal PID As Byte) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Dim slotinfo As Byte()
        Try
            If PID <> 255 Then
                slotinfo = GetSlotInfo()
                packet = New ArrayList
                clsHelper.AddByteArray(packet, New Byte() {247})                                'W3GS header constant
                clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_PLAYERLEAVE_OTHERS})   'W3GS_PLAYERLEAVE_OTHERS
                clsHelper.AddByteArray(packet, New Byte() {0, 0})                               'undefined packet length
                clsHelper.AddByteArray(packet, New Byte() {PID})            'leaver PID
                clsHelper.AddByteArray(packet, New Byte() {13, 0, 0, 0})

                buffer = CType(packet.ToArray(GetType(Byte)), Byte())
                AssignLength(buffer)
                Return buffer
            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_W3GS_INCOMING_ACTION(ByVal fromPID As Byte, ByVal action As Byte(), ByVal sendInterval As Integer) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Dim subpacket As ArrayList
        Dim subbuffer As Byte()
        Dim crc32 As clsCRC32
        Dim crc32value As Byte()
        Try

            subpacket = New ArrayList
            clsHelper.AddByteArray(subpacket, New Byte() {fromPID})
            clsHelper.AddByteArray(subpacket, clsHelper.IntegerToWORD(action.Length, False))  'length
            clsHelper.AddByteArray(subpacket, action)
            subbuffer = CType(subpacket.ToArray(GetType(Byte)), Byte())

            crc32 = New clsCRC32
            crc32value = crc32.ComputeHash(subbuffer)

            If crc32value.Length = 4 Then
                packet = New ArrayList
                clsHelper.AddByteArray(packet, New Byte() {247})                                'W3GS header constant
                clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_INCOMING_ACTION})      'W3GS_INCOMING_ACTION
                clsHelper.AddByteArray(packet, New Byte() {0, 0})                               'undefined packet length
                clsHelper.AddByteArray(packet, clsHelper.IntegerToWORD(sendInterval, False))
                clsHelper.AddByteArray(packet, New Byte() {crc32value(0), crc32value(1)})       'crc32 hash first 2 bytes
                clsHelper.AddByteArray(packet, subbuffer)                                       'pid,length,action

                buffer = CType(packet.ToArray(GetType(Byte)), Byte())
                AssignLength(buffer)
                Return buffer
            End If

            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_W3GS_INCOMING_ACTION(ByVal actionWithPIDAndLength As Byte()(), ByVal sendInterval As Integer) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Dim subpacket As ArrayList
        Dim subbuffer As Byte()
        Dim crc32 As clsCRC32
        Dim crc32value As Byte()

        Dim action As Byte()
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {247})                                'W3GS header constant
            clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_INCOMING_ACTION})      'W3GS_INCOMING_ACTION
            clsHelper.AddByteArray(packet, New Byte() {0, 0})                               'undefined packet length
            clsHelper.AddByteArray(packet, clsHelper.IntegerToWORD(sendInterval, False))

            If actionWithPIDAndLength.Length > 0 Then
                subpacket = New ArrayList
                For Each action In actionWithPIDAndLength
                    clsHelper.AddByteArray(subpacket, action)
                Next
                subbuffer = CType(subpacket.ToArray(GetType(Byte)), Byte())
                crc32 = New clsCRC32
                crc32value = crc32.ComputeHash(subbuffer)

                clsHelper.AddByteArray(packet, New Byte() {crc32value(0), crc32value(1)})       'crc32 hash first 2 bytes
                clsHelper.AddByteArray(packet, subbuffer)                                       'pid,length,action
            End If

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_W3GS_COUNTDOWN_START() As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {247})                                'W3GS header constant
            clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_COUNTDOWN_START})      'W3GS_COUNTDOWN_START
            clsHelper.AddByteArray(packet, New Byte() {0, 0})                               'undefined packet length

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_W3GS_COUNTDOWN_END() As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {247})                                'W3GS header constant
            clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_COUNTDOWN_END})        'W3GS_COUNTDOWN_END
            clsHelper.AddByteArray(packet, New Byte() {0, 0})                               'undefined packet length

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Function SEND_W3GS_GAMELOADED_OTHERS(ByVal PID As Byte) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {247})                                'W3GS header constant
            clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_GAMELOADED_OTHERS})    'W3GS_GAMELOADED_OTHERS
            clsHelper.AddByteArray(packet, New Byte() {0, 0})                               'undefined packet length
            clsHelper.AddByteArray(packet, New Byte() {PID})

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function

    Public Function SEND_W3GS_PING_FROM_HOST() As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Dim pingTime As Integer
        Try

            pingTime = Environment.TickCount
            packet = New ArrayList
            clsHelper.AddByteArray(packet, New Byte() {247})                            'W3GS header constant
            clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_PING_FROM_HOST})   'W3GS_PING_FROM_HOST
            clsHelper.AddByteArray(packet, New Byte() {0, 0})                           'undefined packet length
            clsHelper.AddByteArray(packet, clsHelper.LongToDWORD(Environment.TickCount, False))                     'ping value

            buffer = CType(packet.ToArray(GetType(Byte)), Byte())
            AssignLength(buffer)
            Return buffer

        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function


    Public Function SEND_W3GS_CHAT_FROM_HOST(ByVal fromPID As Byte, ByVal toPIDs As Byte(), ByVal flag As Byte, ByVal flagextra As Byte(), ByVal msg As String) As Byte()
        Dim packet As ArrayList
        Dim buffer As Byte()
        Try
            'MsgBox(String.Format("sending chat from {0} to {1} - {2}", fromPID, toPIDs, msg))
            If msg.Length > 0 AndAlso toPIDs Is Nothing = False AndAlso toPIDs.Length > 0 Then
                packet = New ArrayList
                clsHelper.AddByteArray(packet, New Byte() {247})                            'W3GS header constant
                clsHelper.AddByteArray(packet, New Byte() {Protocol.W3GS_CHAT_FROM_HOST})   'W3GS_CHAT_FROM_HOST
                clsHelper.AddByteArray(packet, New Byte() {0, 0})                       'undefined packet length
                clsHelper.AddByteArray(packet, New Byte() {CType(toPIDs.Length, Byte)}) 'number of receivers
                clsHelper.AddByteArray(packet, toPIDs)                                  'to list of receivers
                clsHelper.AddByteArray(packet, New Byte() {fromPID})                    'from sender
                clsHelper.AddByteArray(packet, New Byte() {flag})                       'msg flag
                clsHelper.AddByteArray(packet, flagextra)                               'extrat 4 byte flag
                clsHelper.AddByteArray(packet, msg)                                     'chat msg
                buffer = CType(packet.ToArray(GetType(Byte)), Byte())
                AssignLength(buffer)
                Return buffer

            End If
            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
#End Region


End Class
