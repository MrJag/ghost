Option Explicit On 
Option Strict On

Imports LainBnetCore

Enum action As Byte
    INVALID
    ADD
    REMOVE
    SHOW
End Enum

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
        MAXGAMES
        BAN

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

        TEST

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

Public Class clsBotAdminClassifier
    
    Private commandType As adminFlags
    Private commandParameter() As String
    Private commandPayload As String

    Public Sub New()
        commandType = adminFlags.NORMAL
        commandParameter = New String() {}
        commandPayload = ""
    End Sub
    Public Sub New(ByVal command As String)
        Dim param As String()
        Dim i As Integer
        Dim list As ArrayList

        param = command.Split(Convert.ToChar(" "))
        If param.Length >= 1 Then

            commandType = adminFlags.NORMAL
            For Each comm As adminFlags In [Enum].GetValues(GetType(adminFlags))
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
            commandType = adminFlags.NORMAL
            commandParameter = New String() {}
            commandPayload = ""
        End If


    End Sub
    Public Function GetCommandType() As adminFlags
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
                'Debug.WriteLine(String.Format("Admin Test: [{0}] =?= [{1}]", user.ToLower, name.ToLower))
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
    Public Event EventBotMaxGames(ByVal max As Byte)
    Public Event EventBotBan(ByVal name As String, ByVal reason As String)
    Public Event EventBotTest(ByVal text As String)

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
                        If command.commandParamameter.Length > 0 And CStr(data.GetUser).ToLower = frmLainEthLite.data.botSettings.RootAdmin.ToLower Then
                            channel = ""
                            For i = 0 To command.commandParamameter.Length - 1
                                channel = channel & command.commandParamameter(i) & " "
                            Next
                            RaiseEvent EventBotChangeChannel(channel.Trim(CChar(" ")))
                        Else
                            If CStr(data.GetUser).ToLower = frmLainEthLite.data.botSettings.RootAdmin Then
                                RaiseEvent EventBotResponse("!CHANNEL [channelname]", True, data.GetUser)
                            End If
                        End If
                        'Netrunner|0.1|txt revolution|
                    Case clsBotCommandClassifier.BotCommandType.RECONNECT
                        If command.commandParamameter.Length = 1 And CStr(data.GetUser).ToLower = frmLainEthLite.data.botSettings.RootAdmin.ToLower Then
                            If command.commandParamameter(0).ToLower = "on" Then
                                RaiseEvent EventBotToggleReconnect(True)
                            ElseIf command.commandParamameter(0).ToLower = "off" Then
                                RaiseEvent EventBotToggleReconnect(False)
                            End If
                        Else
                            If CStr(data.GetUser).ToLower = frmLainEthLite.data.botSettings.RootAdmin Then
                                RaiseEvent EventBotResponse("!RECONNECT [on | off]", True, data.GetUser)
                            End If
                        End If
                        'Netrunner|0.1|txt revolution|
                    Case clsBotCommandClassifier.BotCommandType.ADDADMIN
                        If command.commandParamameter.Length = 1 AndAlso CStr(data.GetUser).ToLower = frmLainEthLite.data.botSettings.RootAdmin.ToLower Then
                            RaiseEvent EventBotModifyAdmin(command.commandParamameter(0).ToLower, True)
                        Else
                            If CStr(data.GetUser).ToLower = frmLainEthLite.data.botSettings.RootAdmin Then
                                RaiseEvent EventBotResponse("!ADDADMIN [name]", True, data.GetUser)
                            End If
                        End If
                        'Netrunner|0.1|txt revolution|
                    Case clsBotCommandClassifier.BotCommandType.REMOVEADMIN
                        If command.commandParamameter.Length = 1 And CStr(data.GetUser).ToLower = frmLainEthLite.data.botSettings.RootAdmin.ToLower Then
                            RaiseEvent EventBotModifyAdmin(command.commandParamameter(0).ToLower, False)
                        Else
                            If CStr(data.GetUser).ToLower = frmLainEthLite.data.botSettings.RootAdmin Then
                                RaiseEvent EventBotResponse("!REMOVEADMIN [name]", True, data.GetUser)
                            End If
                        End If
                    Case clsBotCommandClassifier.BotCommandType.MAXGAMES
                        If command.commandParamameter.Length = 1 AndAlso CStr(data.GetUser).ToLower = frmLainEthLite.data.botSettings.RootAdmin.ToLower Then
                            RaiseEvent EventBotMaxGames(CByte(command.commandParamameter(0).ToLower))
                        Else
                            If CStr(data.GetUser).ToLower = frmLainEthLite.data.botSettings.RootAdmin Then
                                RaiseEvent EventBotResponse("!MAXGAMES [0 < X < 255]", True, data.GetUser)
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
                    Case clsBotCommandClassifier.BotCommandType.ADMIN
                        '-admin add wimble host 
                        'cmd    0   1      2    3    4
                        Dim output As New System.Text.StringBuilder

                        Dim actionType As Byte = action.INVALID

                        If command.commandParamameter(0).ToLower = "add" Then
                            output.Append(String.Format("Added to {0}: ", command.commandParamameter(1)))
                            For i = 2 To command.commandParamameter.Length - 1
                                For Each flag As adminFlags In [Enum].GetValues(GetType(adminFlags))
                                    Debug.WriteLine(String.Format("comparing [{0}] to [{1}]", command.commandParamameter(i).ToLower, flag.ToString.ToLower))
                                    If command.commandParamameter(i).ToLower = flag.ToString.ToLower Then
                                        Debug.WriteLine(String.Format("Getting user [{0}] and setting access flag [{1}]", command.commandParamameter(1), flag.ToString))
                                        frmLainEthLite.data.adminList.getUser(command.commandParamameter(1)).addAccess(flag)
                                        output.Append(String.Format("{0}, ", flag.ToString))
                                        Exit For
                                    End If
                                Next
                            Next
                        ElseIf command.commandParamameter(0).ToLower = "rem" Or command.commandParamameter(0).ToLower = "remove" Then
                            output.Append(String.Format("Removed from {0}: ", command.commandParamameter(1)))
                            For i = 2 To command.commandParamameter.Length - 1
                                For Each flag As adminFlags In [Enum].GetValues(GetType(adminFlags))
                                    Debug.WriteLine(String.Format("comparing [{0}] to [{1}]", command.commandParamameter(i).ToLower, flag.ToString.ToLower))
                                    If command.commandParamameter(i).ToLower = flag.ToString.ToLower Then
                                        Debug.WriteLine(String.Format("Getting user [{0}] and setting access flag [{1}]", command.commandParamameter(1), flag.ToString))
                                        frmLainEthLite.data.adminList.getUser(command.commandParamameter(1)).removeAccess(flag)
                                        output.Append(String.Format("{0}, ", flag.ToString))
                                        Exit For
                                    End If
                                Next
                            Next
                        ElseIf command.commandParamameter(0).ToLower = "show" Then
                            Dim username As String = frmLainEthLite.data.adminList.getUser(command.commandParamameter(1)).name
                            If username.Length > 0 Then
                                output.Append(String.Format("Access for {0}: ", username))
                                For Each flag As adminFlags In [Enum].GetValues(GetType(adminFlags))
                                    If flag <> 0 Then
                                        If ((frmLainEthLite.data.adminList.getUser(command.commandParamameter(1)).flags And flag) = flag) Then
                                            output.Append(String.Format("{0}, ", flag.ToString))
                                        End If
                                    End If
                                Next
                            End If
                        ElseIf command.commandParamameter(0).ToLower = "list" Then
                            'Dim listOutput As New System.Text.StringBuilder
                            output.Append("Current admins: ")
                            For Each adminUser As clsAdmin In frmLainEthLite.data.adminList.getList
                                output.Append(String.Format("{0}, ", adminUser.name))
                            Next
                        Else
                            output.Append(String.Format("{0} does not exist.", command.commandParamameter(1)))
                        End If
                        RaiseEvent EventBotResponse(output.ToString, isWhisper, data.GetUser)
                    Case clsBotCommandClassifier.BotCommandType.BAN
                        Dim output As New System.Text.StringBuilder
                        Dim username As String = frmLainEthLite.data.userList.fixName(command.commandParamameter(1))
                        If username.Length > 0 Then
                            If frmLainEthLite.data.userList.getUser(username).ban.Length > 0 Then
                                output.Append(String.Format("{0} is already banned -- {1}.", username, frmLainEthLite.data.userList.getUser(username).ban))
                            Else
                                Dim description As String = ""
                                For i = 1 To command.commandParamameter.Length - 1
                                    description = description & command.commandParamameter(i) & " "
                                Next
                                description = description.Trim
                                RaiseEvent EventBotBan(command.commandParamameter(0), description)
                            End If
                        Else
                            output.Append(String.Format("{0} does not exist.", username))
                        End If
                        RaiseEvent EventBotResponse(output.ToString, isWhisper, data.GetUser)
                    Case clsBotCommandClassifier.BotCommandType.TEST
                        RaiseEvent EventBotTest(command.commandParamameter(0))
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
    'Public Event EventDataSetAccess(ByVal name As String, ByVal accessLevel As Integer)


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

                            If number >= 1 AndAlso number <= 12 AndAlso number2 >= 1 AndAlso number2 <= 3 Then
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
                Case clsBotCommandClassifier.BotCommandType.ADMIN
                    '-admin add wimble host 
                    'cmd    0   1      2    3    4
                    Dim output As New System.Text.StringBuilder

                    Dim actionType As Byte = action.INVALID

                    If command.commandParamameter(0).ToLower = "add" Then
                        output.Append(String.Format("Added to {0}: ", command.commandParamameter(1)))
                        For i = 2 To command.commandParamameter.Length - 1
                            For Each flag As adminFlags In [Enum].GetValues(GetType(adminFlags))
                                Debug.WriteLine(String.Format("comparing [{0}] to [{1}]", command.commandParamameter(i).ToLower, flag.ToString.ToLower))
                                If command.commandParamameter(i).ToLower = flag.ToString.ToLower Then
                                    Debug.WriteLine(String.Format("Getting user [{0}] and setting access flag [{1}]", command.commandParamameter(1), flag.ToString))
                                    frmLainEthLite.data.adminList.getUser(command.commandParamameter(1)).addAccess(flag)
                                    output.Append(String.Format("{0}, ", flag.ToString))
                                    Exit For
                                End If
                            Next
                        Next
                    ElseIf command.commandParamameter(0).ToLower = "rem" Or command.commandParamameter(0).ToLower = "remove" Then
                        output.Append(String.Format("Removed from {0}: ", command.commandParamameter(1)))
                        For i = 2 To command.commandParamameter.Length - 1
                            For Each flag As adminFlags In [Enum].GetValues(GetType(adminFlags))
                                Debug.WriteLine(String.Format("comparing [{0}] to [{1}]", command.commandParamameter(i).ToLower, flag.ToString.ToLower))
                                If command.commandParamameter(i).ToLower = flag.ToString.ToLower Then
                                    Debug.WriteLine(String.Format("Getting user [{0}] and setting access flag [{1}]", command.commandParamameter(1), flag.ToString))
                                    frmLainEthLite.data.adminList.getUser(command.commandParamameter(1)).removeAccess(flag)
                                    output.Append(String.Format("{0}, ", flag.ToString))
                                    Exit For
                                End If
                            Next
                        Next
                    ElseIf command.commandParamameter(0).ToLower = "show" Then
                        Dim username As String = frmLainEthLite.data.adminList.getUser(command.commandParamameter(1)).name
                        If username.Length > 0 Then
                            output.Append(String.Format("Access for {0}: ", username))
                            For Each flag As adminFlags In [Enum].GetValues(GetType(adminFlags))
                                If flag <> 0 Then
                                    If ((frmLainEthLite.data.adminList.getUser(command.commandParamameter(1)).flags And flag) = flag) Then
                                        output.Append(String.Format("{0}, ", flag.ToString))
                                    End If
                                End If
                            Next
                        End If
                    ElseIf command.commandParamameter(0).ToLower = "list" Then
                        'Dim listOutput As New System.Text.StringBuilder
                        output.Append("Current admins: ")
                        For Each adminUser As clsAdmin In frmLainEthLite.data.adminList.getList
                            output.Append(String.Format("{0}, ", adminUser.name))
                        Next
                    Else
                        output.Append(String.Format("{0} does not exist.", command.commandParamameter(1)))
                    End If
                    RaiseEvent EventBotResponse(output.ToString)

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