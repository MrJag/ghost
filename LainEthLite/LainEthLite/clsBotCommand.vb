Option Explicit On 
Option Strict On

Imports LainBnetCore

Public Class clsBotCommandClassifier
    Public Enum BotCommandType As Integer
        INVALID
        SAY
        HOST
        HOSTBY
        UNHOST
        VERSION
        GETGAMES
        MAP
        RECONNECT 'Netrunner|0.1|txt revolution|
        CHANNEL
        ADDADMIN
        REMOVEADMIN

        'lobby  
        START
        [END]
        OPEN
        CLOSE
        SWAP
        KICK
        PING

        'user management
        ADMIN

        PUB
        PRIV
        REFRESH
        HOLD
        FROM        'MrJag|0.8c|country|command for country check
        SPOOF
        LOCK
        UNLOCK
        COMP
        SP         'inhouse shuffle players

        'game
        LATENCY
        'GAMECANCEL - replaced with END command
        ABORT

    End Enum
    Private commandType As BotCommandType
    Private commandParameter() As String
    Private commandPayload As String

    Public Sub New()
        commandType = BotCommandType.INVALID
        commandParameter = New String() {}
        commandPayload = ""
    End Sub
    Public Sub New(ByVal command As String)
        Dim param As String()
        Dim i As Integer
        Dim list As ArrayList

        param = command.Split(Convert.ToChar(" "))
        If param.Length >= 1 Then

            commandType = BotCommandType.INVALID
            For Each comm As clsBotCommandClassifier.BotCommandType In [Enum].GetValues(GetType(clsBotCommandClassifier.BotCommandType))
                If param(0).ToLower = String.Format("{0}{1}", frmLainEthLite.data.botSettings.commandTrigger, comm.ToString.ToLower()) Then
                    commandType = comm
                    Exit For
                End If
            Next

            commandPayload = ""
            If param.Length > 1 Then
                commandPayload = command.Substring(param(0).Length + 1)
            End If
            commandParameter = New String() {}
            If param.Length >= 2 Then
                list = New ArrayList
                For i = 1 To param.Length - 1
                    list.Add(param(i))
                Next
                commandParameter = CType(list.ToArray(GetType(String)), String())
            End If
        Else
            commandType = BotCommandType.INVALID
            commandParameter = New String() {}
            commandPayload = ""
        End If


    End Sub
    Public Function GetCommandType() As BotCommandType
        Return commandType
    End Function
    Public Function commandParamameter() As String()
        Return commandParameter
    End Function
    Public Function GetPayLoad() As String
        Return commandPayload
    End Function
End Class

Public MustInherit Class clsBotCommand
    Protected adminName() As String


    Protected Sub New(ByVal adminName() As String)
        Me.adminName = adminName
    End Sub
    Protected Function IsTargeted(ByVal command As String) As Boolean
        Try
            If command.StartsWith(frmLainEthLite.data.botSettings.commandTrigger) Then
                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Protected Function IsPrivileged(ByVal user As String, ByVal authorisedUser() As String) As Boolean
        Dim name As String
        Try
            For Each name In authorisedUser
                If user.ToLower = name.ToLower Then
                    Return True
                End If
            Next
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class

Public Class clsBotCommandHostChannel
    Inherits clsBotCommand

    Public Event EventBotSay(ByVal msg As String)
    Public Event EventBotResponse(ByVal msg As String, ByVal isWhisper As Boolean, ByVal owner As String)
    Public Event EventBotHost(ByVal isPublic As Boolean, ByVal numPlayers As Integer, ByVal gameName As String, ByVal callerName As String, ByVal isWhisper As Boolean, ByVal owner As String)
    Public Event EventBotUnHost(ByVal isWhisper As Boolean, ByVal owner As String)
    Public Event EventBotGetGames(ByVal isWhisper As Boolean, ByVal owner As String)
    Public Event EventBotMap(ByVal isWhisper As Boolean, ByVal owner As String, ByVal mapName As String)

    Public Event EventBotToggleReconnect(ByVal toggle As Boolean)
    Public Event EventBotChangeChannel(ByVal channelName As String)
    Public Event EventBotModifyAdmin(ByVal name As String, ByVal add As Boolean)

    Public Sub New(ByVal adminName() As String)
        MyBase.New(adminName)
    End Sub

    Public Function ProcessCommand(ByVal data As clsIncomingChatChannel) As Boolean
        Dim command As clsBotCommandClassifier
        Dim game As String
        Dim caller As String
        Dim channel As String
        Dim isWhisper As Boolean
        Dim i As Integer
        Try

            If data.GetChatEvent = clsProtocolBNET.IncomingChatEvent.EID_TALK OrElse data.GetChatEvent = clsProtocolBNET.IncomingChatEvent.EID_WHISPER Then
                If IsTargeted(data.GetMessage) = False Then
                    Return False
                End If
                command = New clsBotCommandClassifier(data.GetMessage)
                If IsPrivileged(data.GetUser, adminName) = False Then
                    Return False
                End If
                isWhisper = (data.GetChatEvent = clsProtocolBNET.IncomingChatEvent.EID_WHISPER)
                Select Case command.GetCommandType
                    Case clsBotCommandClassifier.BotCommandType.INVALID
                    Case clsBotCommandClassifier.BotCommandType.SAY
                        RaiseEvent EventBotSay(command.GetPayLoad)
                    Case clsBotCommandClassifier.BotCommandType.VERSION
                        RaiseEvent EventBotResponse(String.Format("{0}  (http://ghost.pwner.org/)", frmLainEthLite.ProjectLainVersion), isWhisper, data.GetUser)
                    Case clsBotCommandClassifier.BotCommandType.HOSTBY
                        If command.commandParamameter.Length >= 3 Then
                            caller = command.commandParamameter(0)

                            If caller.Trim.Length > 0 Then
                                game = ""
                                For i = 2 To command.commandParamameter.Length - 1
                                    game = game & command.commandParamameter(i) & " "
                                Next
                                game = game.Trim
                                If game.Length > 0 Then
                                    If command.commandParamameter(1).ToLower = "public" OrElse command.commandParamameter(1).ToLower = "private" Then
                                        Select Case command.commandParamameter(1).ToLower
                                            Case "public"
                                                RaiseEvent EventBotHost(True, 10, game, caller, isWhisper, data.GetUser)
                                            Case "private"
                                                RaiseEvent EventBotHost(False, 10, game, caller, isWhisper, data.GetUser)
                                        End Select
                                        Exit Select
                                    End If
                                End If
                            End If
                        End If

                        RaiseEvent EventBotResponse("-HOSTBY [creator name] [public|private] [game]", isWhisper, data.GetUser)
                    Case clsBotCommandClassifier.BotCommandType.HOST
                        If command.commandParamameter.Length >= 2 Then
                            game = ""
                            For i = 1 To command.commandParamameter.Length - 1
                                game = game & command.commandParamameter(i) & " "
                            Next
                            game = game.Trim
                            If game.Length > 0 Then
                                If command.commandParamameter(0).ToLower = "public" OrElse command.commandParamameter(0).ToLower = "private" Then
                                    Select Case command.commandParamameter(0).ToLower
                                        Case "public"
                                            RaiseEvent EventBotHost(True, 10, game, data.GetUser, isWhisper, data.GetUser)
                                        Case "private"
                                            RaiseEvent EventBotHost(False, 10, game, data.GetUser, isWhisper, data.GetUser)
                                    End Select
                                    Exit Select
                                End If
                            End If
                        End If
                        RaiseEvent EventBotResponse("-HOST [public|private] [game]", isWhisper, data.GetUser)
                    Case clsBotCommandClassifier.BotCommandType.UNHOST
                        RaiseEvent EventBotUnHost(isWhisper, data.GetUser)
                    Case clsBotCommandClassifier.BotCommandType.GETGAMES
                        RaiseEvent EventBotGetGames(isWhisper, data.GetUser)
                        'Netrunner|0.1|root admin commands|
                    Case clsBotCommandClassifier.BotCommandType.CHANNEL
                        If command.commandParamameter.Length > 0 And CStr(data.GetUser).ToLower = frmLainEthLite.RootAdmin.ToLower Then
                            channel = ""
                            For i = 0 To command.commandParamameter.Length - 1
                                channel = channel & command.commandParamameter(i) & " "
                            Next
                            RaiseEvent EventBotChangeChannel(channel.Trim(CChar(" ")))
                        Else
                            If CStr(data.GetUser).ToLower = frmLainEthLite.RootAdmin Then
                                RaiseEvent EventBotResponse("!CHANNEL [channelname]", True, data.GetUser)
                            End If
                        End If
                        'Netrunner|0.1|txt revolution|
                    Case clsBotCommandClassifier.BotCommandType.RECONNECT
                        If command.commandParamameter.Length = 1 And CStr(data.GetUser).ToLower = frmLainEthLite.RootAdmin.ToLower Then
                            If command.commandParamameter(0).ToLower = "on" Then
                                RaiseEvent EventBotToggleReconnect(True)
                            ElseIf command.commandParamameter(0).ToLower = "off" Then
                                RaiseEvent EventBotToggleReconnect(False)
                            End If
                        Else
                            If CStr(data.GetUser).ToLower = frmLainEthLite.RootAdmin Then
                                RaiseEvent EventBotResponse("!RECONNECT [on | off]", True, data.GetUser)
                            End If
                        End If
                        'Netrunner|0.1|txt revolution|
                    Case clsBotCommandClassifier.BotCommandType.ADDADMIN
                        If command.commandParamameter.Length = 1 AndAlso CStr(data.GetUser).ToLower = frmLainEthLite.RootAdmin.ToLower Then
                            RaiseEvent EventBotModifyAdmin(command.commandParamameter(0).ToLower, True)
                        Else
                            If CStr(data.GetUser).ToLower = frmLainEthLite.RootAdmin Then
                                RaiseEvent EventBotResponse("!ADDADMIN [name]", True, data.GetUser)
                            End If
                        End If
                        'Netrunner|0.1|txt revolution|
                    Case clsBotCommandClassifier.BotCommandType.REMOVEADMIN
                        If command.commandParamameter.Length = 1 And CStr(data.GetUser).ToLower = frmLainEthLite.RootAdmin.ToLower Then
                            RaiseEvent EventBotModifyAdmin(command.commandParamameter(0).ToLower, False)
                        Else
                            If CStr(data.GetUser).ToLower = frmLainEthLite.RootAdmin Then
                                RaiseEvent EventBotResponse("!REMOVEADMIN [name]", True, data.GetUser)
                            End If
                        End If
                    Case clsBotCommandClassifier.BotCommandType.MAP
                        If command.commandParamameter.Length > 0 Then
                            RaiseEvent EventBotMap(isWhisper, data.GetUser, command.GetPayLoad)
                        Else
                            RaiseEvent EventBotResponse("-MAP [map hash xml file name]", isWhisper, data.GetUser)
                        End If
                    Case clsBotCommandClassifier.BotCommandType.PUB 'MrJag|0.8c|commands|hosting a public game
                        Dim obs As Boolean = False
                        game = ""
                        If command.commandParamameter(0).ToLower = "obs" Then
                            obs = True
                        End If
                        If obs Then
                            For i = 1 To command.commandParamameter.Length - 1
                                game = game & command.commandParamameter(i) & " "
                            Next
                            game = game.Trim
                            If game.Length > 0 Then
                                RaiseEvent EventBotHost(True, 12, game, data.GetUser, isWhisper, data.GetUser)
                            End If
                        Else
                            For i = 0 To command.commandParamameter.Length - 1
                                game = game & command.commandParamameter(i) & " "
                            Next
                            game = game.Trim
                            If game.Length > 0 Then
                                RaiseEvent EventBotHost(True, 10, game, data.GetUser, isWhisper, data.GetUser)
                            End If
                        End If
                    Case clsBotCommandClassifier.BotCommandType.PRIV 'MrJag|0.8c|commands|hosting a public game
                        Dim obs As Boolean = False
                        game = ""
                        If command.commandParamameter(0).ToLower = "obs" Then
                            obs = True
                        End If
                        If obs Then
                            For i = 1 To command.commandParamameter.Length - 1
                                game = game & command.commandParamameter(i) & " "
                            Next
                            game = game.Trim
                            If game.Length > 0 Then
                                RaiseEvent EventBotHost(False, 12, game, data.GetUser, isWhisper, data.GetUser)
                            End If
                        Else
                            For i = 0 To command.commandParamameter.Length - 1
                                game = game & command.commandParamameter(i) & " "
                            Next
                            game = game.Trim
                            If game.Length > 0 Then
                                RaiseEvent EventBotHost(False, 10, game, data.GetUser, isWhisper, data.GetUser)
                            End If
                        End If
                End Select
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


End Class
Public Class clsBotCommandHostLobby
    Inherits clsBotCommand

    Private callerName As String

    Public Event EventBotResponse(ByVal msg As String)
    Public Event EventBotSlot(ByVal open As Boolean, ByVal slotNumber As Byte)
    Public Event EventBotComputer(ByVal slotNumber As Byte, ByVal skill As Byte)
    Public Event EventBotStart(ByVal isForced As Boolean)
    Public Event EventBotEnd()
    Public Event EventBotSwap(ByVal slot1 As Byte, ByVal slot2 As Byte)
    Public Event EventBotKick(ByVal name As String)
    Public Event EventBotPing(ByVal maxPing As Integer)     'MrJag|0.8c|ping|
    Public Event EventBotCountry()
    Public Event EventBotToggleRefresh(ByVal enabled As Boolean)                    'MrJag|0.9b|refresh|
    Public Event EventBotHold(ByVal name As String)         'MrJag|0.9b|hold|
    Public Event EventBotSpoof(ByVal name As String, ByVal msg As String)
    Public Event EventBotSay(ByVal msg As String)
    Public Event EventBotLock(ByVal username As String)
    Public Event EventBotUnlock(ByVal username As String)
    Public Event EventBotShufflePlayers()
    Public Event EventDataSetAccess(ByVal name As String, ByVal accessLevel As Integer)


    Public Sub New(ByVal callerName As String, ByVal adminName As String())
        MyBase.New(adminName)
        Me.callerName = callerName
    End Sub

    Public Function ProcessCommand(ByVal user As String, ByVal msg As String) As Boolean
        Dim command As clsBotCommandClassifier
        Dim number As Byte
        Dim number2 As Byte
        Dim target As String

        Try
            If IsTargeted(msg) = False OrElse user.Length = 0 Then
                Return False
            End If
            command = New clsBotCommandClassifier(msg)
            If user.ToLower <> callerName.ToLower AndAlso IsPrivileged(user, adminName) = False Then
                Return False
            End If
            'MsgBox(String.Format("processing cmd {0} {1}", command.GetCommandType, command.GetPayLoad))
            Select Case command.GetCommandType
                Case clsBotCommandClassifier.BotCommandType.INVALID
                Case clsBotCommandClassifier.BotCommandType.SAY
                    'RaiseEvent EventBotResponse(command.GetPayLoad)
                    RaiseEvent EventBotSay(command.GetPayLoad)
                Case clsBotCommandClassifier.BotCommandType.VERSION
                    RaiseEvent EventBotResponse(String.Format("{0}  (http://ghost.pwner.org/)", frmLainEthLite.ProjectLainVersion))
                Case clsBotCommandClassifier.BotCommandType.START
                    If command.commandParamameter.Length = 1 Then
                        If command.commandParamameter(0).ToLower = "force" Then
                            RaiseEvent EventBotStart(True)
                        Else
                            RaiseEvent EventBotResponse("-START <force>")
                        End If
                        Exit Select
                    End If
                    RaiseEvent EventBotStart(False)
                Case clsBotCommandClassifier.BotCommandType.END
                    RaiseEvent EventBotEnd()
                Case clsBotCommandClassifier.BotCommandType.SWAP
                    If command.commandParamameter.Length = 2 Then
                        If IsNumeric(command.commandParamameter(0)) AndAlso IsNumeric(command.commandParamameter(1)) Then
                            number = CType(command.commandParamameter(0), Byte)
                            number2 = CType(command.commandParamameter(1), Byte)

                            If number >= 1 AndAlso number <= 12 AndAlso number2 >= 1 AndAlso number2 <= 12 Then
                                RaiseEvent EventBotSwap(CType(number - 1, Byte), CType(number2 - 1, Byte))
                                Exit Select
                            End If
                        End If
                    End If
                    RaiseEvent EventBotResponse("-SWAP [1..12] [1..12]")
                Case clsBotCommandClassifier.BotCommandType.OPEN
                    If command.commandParamameter.Length = 1 Then
                        If IsNumeric(command.commandParamameter(0)) Then
                            number = CType(command.commandParamameter(0), Byte)
                            If number >= 1 AndAlso number <= 12 Then
                                RaiseEvent EventBotSlot(True, CType(number - 1, Byte))
                                Exit Select
                            End If
                        End If
                    End If
                    RaiseEvent EventBotResponse("-OPEN [1..12]")
                Case clsBotCommandClassifier.BotCommandType.CLOSE
                    If command.commandParamameter.Length = 1 Then
                        If IsNumeric(command.commandParamameter(0)) Then
                            number = CType(command.commandParamameter(0), Byte)
                            If number >= 1 AndAlso number <= 12 Then
                                RaiseEvent EventBotSlot(False, CType(number - 1, Byte))
                                Exit Select
                            End If
                        End If
                    End If
                    RaiseEvent EventBotResponse("-CLOSE [1..12]")
                Case clsBotCommandClassifier.BotCommandType.COMP
                    If command.commandParamameter.Length = 2 Then
                        If IsNumeric(command.commandParamameter(0)) AndAlso IsNumeric(command.commandParamameter(1)) Then
                            number = CType(command.commandParamameter(0), Byte)
                            number2 = CType(command.commandParamameter(1), Byte)

                            If number >= 1 AndAlso number <= 12 AndAlso number2 >= 1 AndAlso number2 <= 12 Then
                                RaiseEvent EventBotComputer(CType(number - 1, Byte), CType(number2 - 1, Byte))
                                Exit Select
                            End If
                        End If
                    End If
                    RaiseEvent EventBotResponse("-COMP [1..12] [1..3]")
                Case clsBotCommandClassifier.BotCommandType.KICK
                    If command.commandParamameter.Length = 1 Then
                        target = command.commandParamameter(0)
                        If target.Trim.Length > 0 Then
                            RaiseEvent EventBotKick(target)
                            Exit Select
                        End If
                    End If
                    RaiseEvent EventBotResponse("-KICK [name]")
                Case clsBotCommandClassifier.BotCommandType.PING    'MrJag|0.8c|ping|
                    If command.commandParamameter.Length = 0 Then
                        RaiseEvent EventBotPing(1000)
                    ElseIf command.commandParamameter.Length = 1 Then
                        RaiseEvent EventBotPing(CInt(command.commandParamameter(0)))
                    Else
                        RaiseEvent EventBotResponse("!PING <max ping>")
                    End If
                Case clsBotCommandClassifier.BotCommandType.FROM
                    RaiseEvent EventBotCountry()
                Case clsBotCommandClassifier.BotCommandType.REFRESH    'MrJag|0.9b|hold|
                    If command.commandParamameter.Length = 1 Then
                        If command.commandParamameter(0).ToLower = "on" Then
                            RaiseEvent EventBotToggleRefresh(True)
                        ElseIf command.commandParamameter(0).ToLower = "off" Then
                            RaiseEvent EventBotToggleRefresh(False)
                        End If
                    Else
                        RaiseEvent EventBotResponse("-REFRESH [on | off]")
                    End If
                Case clsBotCommandClassifier.BotCommandType.HOLD    'MrJag|0.9b|hold|
                    If command.commandParamameter.Length = 1 Then
                        RaiseEvent EventBotHold(command.commandParamameter(0))
                    Else
                        RaiseEvent EventBotResponse("!HOLD [name]")
                    End If
                Case clsBotCommandClassifier.BotCommandType.SPOOF    'MrJag|0.9b|hold|
                    Dim spoofMsg As String = ""
                    For i = 1 To command.commandParamameter.Length - 1
                        spoofMsg = spoofMsg & command.commandParamameter(i) & " "
                    Next
                    spoofMsg = spoofMsg.Trim
                    RaiseEvent EventBotSpoof(command.commandParamameter(0), spoofMsg)
                Case clsBotCommandClassifier.BotCommandType.LOCK    'MrJag|0.9b|hold|
                    RaiseEvent EventBotLock(user)
                Case clsBotCommandClassifier.BotCommandType.UNLOCK    'MrJag|0.9b|hold|
                    RaiseEvent EventBotUnlock(user)
                Case clsBotCommandClassifier.BotCommandType.SP    'MrJag|0.9b|hold|
                    RaiseEvent EventBotShufflePlayers()
                Case clsBotCommandClassifier.BotCommandType.ADMIN    'MrJag|0.9b|hold|
                    'MsgBox(String.Format("stage 0:  {0} {1} {2} {3}", command.commandParamameter(0), command.commandParamameter(1), command.commandParamameter(2), command.commandParamameter(3)))
                    If command.commandParamameter(0).ToLower = "set" AndAlso command.commandParamameter(1).ToLower = "access" Then
                        'MsgBox(String.Format("stage 1:  {0} {1} {2} {3} {4}", command.commandParamameter(0), command.commandParamameter(1), command.commandParamameter(2), command.commandParamameter(3), command.commandParamameter(4)))
                        If IsNumeric(command.commandParamameter(3)) Then
                            'MsgBox(String.Format("stage 2:  {0} {1} {2} {3} {4}", command.commandParamameter(0), command.commandParamameter(1), command.commandParamameter(2), command.commandParamameter(3), command.commandParamameter(4)))
                            Dim accessLevel As Integer = CInt(command.commandParamameter(3))
                            accessLevel = Math.Min(255, accessLevel)
                            accessLevel = Math.Max(0, accessLevel)
                            RaiseEvent EventDataSetAccess(command.commandParamameter(2), accessLevel)
                        Else
                            RaiseEvent EventBotResponse("-ADMIN SET ACCESS [name] [0..255]")
                        End If
                    Else
                        RaiseEvent EventBotResponse(String.Format("{0} {1} {2} {3} {4}", command.commandParamameter(0), command.commandParamameter(1), command.commandParamameter(2), command.commandParamameter(3), command.commandParamameter(4)))
                    End If
            End Select
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class
Public Class clsBotCommandHostGame
    Inherits clsBotCommand

    Private callerName As String

    Public Event EventBotResponse(ByVal msg As String)
    Public Event EventBotLatency(ByVal ms As Integer)
    Public Event EventBotKick(ByVal name As String, ByVal kicker As String)
    Public Event EventBotGameCancel()
    Public Event EventBotAbort()
    Public Event EventBotSpoof(ByVal name As String, ByVal msg As String)
    Public Event EventBotSay(ByVal msg As String)


    Public Sub New(ByVal callerName As String, ByVal adminName As String())
        MyBase.New(adminName)
        Me.callerName = callerName
    End Sub

    Public Function ProcessCommand(ByVal user As String, ByVal msg As String) As Boolean
        Dim command As clsBotCommandClassifier
        Dim number As Integer
        Dim target As String

        Try
            If IsTargeted(msg) = False OrElse user.Length = 0 Then
                Return False
            End If
            command = New clsBotCommandClassifier(msg)
            If user.ToLower <> callerName.ToLower AndAlso IsPrivileged(user, adminName) = False Then
                Return False
            End If

            Select Case command.GetCommandType
                Case clsBotCommandClassifier.BotCommandType.INVALID
                Case clsBotCommandClassifier.BotCommandType.SAY
                    RaiseEvent EventBotResponse(command.GetPayLoad)
                Case clsBotCommandClassifier.BotCommandType.VERSION
                    RaiseEvent EventBotResponse(frmLainEthLite.ProjectLainVersion)
                Case clsBotCommandClassifier.BotCommandType.LATENCY
                    If command.commandParamameter.Length >= 1 AndAlso IsNumeric(command.commandParamameter(0)) Then
                        number = CType(command.commandParamameter(0), Integer)
                        number = Math.Max(number, 50)
                        number = Math.Min(number, 500)
                        RaiseEvent EventBotLatency(number)
                        Exit Select
                    End If
                    RaiseEvent EventBotLatency(0)
                Case clsBotCommandClassifier.BotCommandType.KICK
                    If command.commandParamameter.Length = 1 Then
                        target = command.commandParamameter(0)
                        If target.Trim.Length > 0 Then
                            RaiseEvent EventBotKick(target, user)
                            Exit Select
                        End If
                    End If
                Case clsBotCommandClassifier.BotCommandType.END
                    RaiseEvent EventBotGameCancel()
                Case clsBotCommandClassifier.BotCommandType.ABORT
                    RaiseEvent EventBotAbort()
                Case clsBotCommandClassifier.BotCommandType.SPOOF    'MrJag|0.9b|hold|
                    Dim spoofMsg As String = ""
                    For i = 1 To command.commandParamameter.Length - 1
                        spoofMsg = spoofMsg & command.commandParamameter(i) & " "
                    Next
                    spoofMsg = spoofMsg.Trim
                    RaiseEvent EventBotSpoof(command.commandParamameter(0), spoofMsg)
            End Select

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class


















































'