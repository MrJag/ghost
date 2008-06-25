Option Explicit On 
Option Strict On

Imports System.IO
Imports LainBnetCore
Imports LainHelper

Public Class frmLainEthLite
    Inherits System.Windows.Forms.Form
    'Netrunner|0.11|txt revolution|
    Public Shared RootAdmin As String
    Public Shared Parameters As Integer
    Public CurrentAdminList As ArrayList

    Public Shared data As New clsData

    Friend WithEvents menuLENP As System.Windows.Forms.MenuItem
    Friend WithEvents menuLENPServer As System.Windows.Forms.MenuItem
    Friend WithEvents menuLENPClient As System.Windows.Forms.MenuItem
    Friend WithEvents menuConvertor As System.Windows.Forms.MenuItem

    Private Const GAME_PRIVATE As Integer = 17
    Private Const GAME_PUBLIC As Integer = 16

    Public Const ProjectLainVersion As String = "Battle.net Game Host v0.16 (beta)"
    Public Const ProjectLainName As String = "LainEthLite"
    Public Const ProjectLainConfig As String = "LainEthLiteConfiguration"
    Public Const ProjectLainMap As String = "LainEthMap"
    Public Const ProjectLainUser As String = "LainEthLiteUser"
    Public Const ProjectLainCustomData As String = "LainEthLiteCustomData"

    Private WithEvents bnet As clsBNET
    Private WithEvents channel As clsBNETChannel
    Private WithEvents bot As clsBotCommandHostChannel

    Private WithEvents aliveTimer As Timers.Timer
    Private aliveCounter As Integer
    Private channelJoinCounter As Integer

    Private listHost As ArrayList
    Private currentHost As clsGameHost
    Private map As clsGameHostMap
    Private customData As clsBNETCustomData

    Private formLENPServer As frmLENPServer
    Friend WithEvents botParam As System.Windows.Forms.GroupBox
    Friend WithEvents CheckBox2 As System.Windows.Forms.CheckBox
    Friend WithEvents CheckBox1 As System.Windows.Forms.CheckBox
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents CheckBox3 As System.Windows.Forms.CheckBox
    Friend WithEvents MenuItem1 As System.Windows.Forms.MenuItem
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents TextBox3 As System.Windows.Forms.TextBox
    Private lenp As clsLENPServer


#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents menuMain As System.Windows.Forms.MainMenu
    Friend WithEvents contextMain As System.Windows.Forms.ContextMenu
    Friend WithEvents trayIcon As System.Windows.Forms.NotifyIcon
    Friend WithEvents menuFile As System.Windows.Forms.MenuItem
    Friend WithEvents menuExit As System.Windows.Forms.MenuItem
    Friend WithEvents labelStatus As System.Windows.Forms.Label
    Friend WithEvents contextShow As System.Windows.Forms.MenuItem
    Friend WithEvents contextExit As System.Windows.Forms.MenuItem
    Public WithEvents txtChatLog As System.Windows.Forms.TextBox
    Friend WithEvents txtChat As System.Windows.Forms.TextBox
    Friend WithEvents buttonChat As System.Windows.Forms.Button
    Friend WithEvents buttonGo As System.Windows.Forms.Button
    Friend WithEvents labelChannel As System.Windows.Forms.Label
    Friend WithEvents comboRealm As System.Windows.Forms.ComboBox
    Friend WithEvents txtChannel As System.Windows.Forms.TextBox
    Friend WithEvents txtPassword As System.Windows.Forms.TextBox
    Friend WithEvents txtAccount As System.Windows.Forms.TextBox
    Friend WithEvents txtTFT As System.Windows.Forms.TextBox
    Friend WithEvents txtROC As System.Windows.Forms.TextBox
    Friend WithEvents groupParam As System.Windows.Forms.GroupBox
    Friend WithEvents labelSummary As System.Windows.Forms.Label
    Friend WithEvents labelBotServer As System.Windows.Forms.Label
    Friend WithEvents labelFirstChannel As System.Windows.Forms.Label
    Friend WithEvents labelPassword As System.Windows.Forms.Label
    Friend WithEvents labelUserName As System.Windows.Forms.Label
    Friend WithEvents labelTFTKey As System.Windows.Forms.Label
    Friend WithEvents labelROCKey As System.Windows.Forms.Label
    Friend WithEvents labelRealm As System.Windows.Forms.Label
    Friend WithEvents txtWar3 As System.Windows.Forms.TextBox
    Friend WithEvents listChannelPeople As System.Windows.Forms.ListBox
    Friend WithEvents labelHostPort As System.Windows.Forms.Label
    Friend WithEvents txtHostPort As System.Windows.Forms.TextBox
    Friend WithEvents groupUser As System.Windows.Forms.GroupBox
    Friend WithEvents listUser As System.Windows.Forms.ListBox
    Friend WithEvents txtUserName As System.Windows.Forms.TextBox
    Friend WithEvents buttonUserAdd As System.Windows.Forms.Button
    Friend WithEvents buttonUserRemove As System.Windows.Forms.Button
    Friend WithEvents buttonGameStop As System.Windows.Forms.Button
    Friend WithEvents listGame As System.Windows.Forms.ListBox
    Friend WithEvents buttonGameInfo As System.Windows.Forms.Button
    Friend WithEvents groupGame As System.Windows.Forms.GroupBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmLainEthLite))
        Me.menuMain = New System.Windows.Forms.MainMenu(Me.components)
        Me.menuFile = New System.Windows.Forms.MenuItem
        Me.menuConvertor = New System.Windows.Forms.MenuItem
        Me.menuExit = New System.Windows.Forms.MenuItem
        Me.menuLENP = New System.Windows.Forms.MenuItem
        Me.menuLENPServer = New System.Windows.Forms.MenuItem
        Me.menuLENPClient = New System.Windows.Forms.MenuItem
        Me.contextMain = New System.Windows.Forms.ContextMenu
        Me.contextShow = New System.Windows.Forms.MenuItem
        Me.MenuItem1 = New System.Windows.Forms.MenuItem
        Me.contextExit = New System.Windows.Forms.MenuItem
        Me.trayIcon = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.labelStatus = New System.Windows.Forms.Label
        Me.txtChatLog = New System.Windows.Forms.TextBox
        Me.txtChat = New System.Windows.Forms.TextBox
        Me.buttonChat = New System.Windows.Forms.Button
        Me.buttonGo = New System.Windows.Forms.Button
        Me.labelChannel = New System.Windows.Forms.Label
        Me.comboRealm = New System.Windows.Forms.ComboBox
        Me.labelBotServer = New System.Windows.Forms.Label
        Me.labelFirstChannel = New System.Windows.Forms.Label
        Me.labelPassword = New System.Windows.Forms.Label
        Me.labelUserName = New System.Windows.Forms.Label
        Me.labelTFTKey = New System.Windows.Forms.Label
        Me.labelROCKey = New System.Windows.Forms.Label
        Me.labelRealm = New System.Windows.Forms.Label
        Me.txtChannel = New System.Windows.Forms.TextBox
        Me.txtPassword = New System.Windows.Forms.TextBox
        Me.txtAccount = New System.Windows.Forms.TextBox
        Me.txtTFT = New System.Windows.Forms.TextBox
        Me.txtROC = New System.Windows.Forms.TextBox
        Me.groupParam = New System.Windows.Forms.GroupBox
        Me.txtHostPort = New System.Windows.Forms.TextBox
        Me.labelHostPort = New System.Windows.Forms.Label
        Me.txtWar3 = New System.Windows.Forms.TextBox
        Me.labelSummary = New System.Windows.Forms.Label
        Me.listChannelPeople = New System.Windows.Forms.ListBox
        Me.groupUser = New System.Windows.Forms.GroupBox
        Me.buttonUserRemove = New System.Windows.Forms.Button
        Me.buttonUserAdd = New System.Windows.Forms.Button
        Me.txtUserName = New System.Windows.Forms.TextBox
        Me.listUser = New System.Windows.Forms.ListBox
        Me.groupGame = New System.Windows.Forms.GroupBox
        Me.buttonGameInfo = New System.Windows.Forms.Button
        Me.buttonGameStop = New System.Windows.Forms.Button
        Me.listGame = New System.Windows.Forms.ListBox
        Me.botParam = New System.Windows.Forms.GroupBox
        Me.Label3 = New System.Windows.Forms.Label
        Me.TextBox3 = New System.Windows.Forms.TextBox
        Me.CheckBox3 = New System.Windows.Forms.CheckBox
        Me.Label2 = New System.Windows.Forms.Label
        Me.TextBox2 = New System.Windows.Forms.TextBox
        Me.CheckBox2 = New System.Windows.Forms.CheckBox
        Me.CheckBox1 = New System.Windows.Forms.CheckBox
        Me.TextBox1 = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.groupParam.SuspendLayout()
        Me.groupUser.SuspendLayout()
        Me.groupGame.SuspendLayout()
        Me.botParam.SuspendLayout()
        Me.SuspendLayout()
        '
        'menuMain
        '
        Me.menuMain.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.menuFile, Me.menuLENP})
        '
        'menuFile
        '
        Me.menuFile.Index = 0
        Me.menuFile.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.menuConvertor, Me.menuExit})
        Me.menuFile.Text = "File"
        '
        'menuConvertor
        '
        Me.menuConvertor.Index = 0
        Me.menuConvertor.Text = "Hexadecimal Convertor"
        '
        'menuExit
        '
        Me.menuExit.Index = 1
        Me.menuExit.Text = "Exit"
        '
        'menuLENP
        '
        Me.menuLENP.Index = 1
        Me.menuLENP.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.menuLENPServer, Me.menuLENPClient})
        Me.menuLENP.Text = "LENP"
        '
        'menuLENPServer
        '
        Me.menuLENPServer.Index = 0
        Me.menuLENPServer.Text = "LENP Server"
        '
        'menuLENPClient
        '
        Me.menuLENPClient.Index = 1
        Me.menuLENPClient.Text = "LENP Client"
        '
        'contextMain
        '
        Me.contextMain.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.contextShow, Me.MenuItem1, Me.contextExit})
        '
        'contextShow
        '
        Me.contextShow.Index = 0
        Me.contextShow.Text = "Show/Hide GHost"
        '
        'MenuItem1
        '
        Me.MenuItem1.Index = 1
        Me.MenuItem1.Text = "-"
        '
        'contextExit
        '
        Me.contextExit.Index = 2
        Me.contextExit.Text = "Exit"
        '
        'trayIcon
        '
        Me.trayIcon.Icon = CType(resources.GetObject("trayIcon.Icon"), System.Drawing.Icon)
        Me.trayIcon.Text = "Lain Host"
        Me.trayIcon.Visible = True
        '
        'labelStatus
        '
        Me.labelStatus.BackColor = System.Drawing.Color.DarkGray
        Me.labelStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.labelStatus.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelStatus.Location = New System.Drawing.Point(0, 524)
        Me.labelStatus.Name = "labelStatus"
        Me.labelStatus.Size = New System.Drawing.Size(731, 22)
        Me.labelStatus.TabIndex = 7
        '
        'txtChatLog
        '
        Me.txtChatLog.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtChatLog.Location = New System.Drawing.Point(7, 284)
        Me.txtChatLog.Multiline = True
        Me.txtChatLog.Name = "txtChatLog"
        Me.txtChatLog.ReadOnly = True
        Me.txtChatLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtChatLog.Size = New System.Drawing.Size(714, 192)
        Me.txtChatLog.TabIndex = 1
        Me.txtChatLog.Text = "======================" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Chat Log" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "======================"
        '
        'txtChat
        '
        Me.txtChat.Location = New System.Drawing.Point(7, 483)
        Me.txtChat.Multiline = True
        Me.txtChat.Name = "txtChat"
        Me.txtChat.Size = New System.Drawing.Size(661, 34)
        Me.txtChat.TabIndex = 3
        '
        'buttonChat
        '
        Me.buttonChat.Location = New System.Drawing.Point(675, 483)
        Me.buttonChat.Name = "buttonChat"
        Me.buttonChat.Size = New System.Drawing.Size(46, 34)
        Me.buttonChat.TabIndex = 4
        Me.buttonChat.Text = "Send"
        '
        'buttonGo
        '
        Me.buttonGo.Font = New System.Drawing.Font("Trebuchet MS", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonGo.Location = New System.Drawing.Point(647, 222)
        Me.buttonGo.Name = "buttonGo"
        Me.buttonGo.Size = New System.Drawing.Size(241, 28)
        Me.buttonGo.TabIndex = 0
        Me.buttonGo.Text = "Start/Stop"
        '
        'labelChannel
        '
        Me.labelChannel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.labelChannel.Font = New System.Drawing.Font("Courier New", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelChannel.Location = New System.Drawing.Point(7, 257)
        Me.labelChannel.Name = "labelChannel"
        Me.labelChannel.Size = New System.Drawing.Size(881, 19)
        Me.labelChannel.TabIndex = 5
        Me.labelChannel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'comboRealm
        '
        Me.comboRealm.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.comboRealm.ItemHeight = 13
        Me.comboRealm.Items.AddRange(New Object() {"uswest.battle.net", "useast.battle.net", "asia.battle.net", "europe.battle.net"})
        Me.comboRealm.Location = New System.Drawing.Point(107, 49)
        Me.comboRealm.Name = "comboRealm"
        Me.comboRealm.Size = New System.Drawing.Size(200, 21)
        Me.comboRealm.TabIndex = 1
        Me.comboRealm.Text = "uswest.battle.net"
        '
        'labelBotServer
        '
        Me.labelBotServer.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelBotServer.Location = New System.Drawing.Point(7, 21)
        Me.labelBotServer.Name = "labelBotServer"
        Me.labelBotServer.Size = New System.Drawing.Size(93, 21)
        Me.labelBotServer.TabIndex = 31
        Me.labelBotServer.Text = "War3 Path"
        '
        'labelFirstChannel
        '
        Me.labelFirstChannel.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelFirstChannel.Location = New System.Drawing.Point(7, 187)
        Me.labelFirstChannel.Name = "labelFirstChannel"
        Me.labelFirstChannel.Size = New System.Drawing.Size(93, 21)
        Me.labelFirstChannel.TabIndex = 28
        Me.labelFirstChannel.Text = "Home Channel"
        '
        'labelPassword
        '
        Me.labelPassword.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelPassword.Location = New System.Drawing.Point(7, 159)
        Me.labelPassword.Name = "labelPassword"
        Me.labelPassword.Size = New System.Drawing.Size(93, 21)
        Me.labelPassword.TabIndex = 27
        Me.labelPassword.Text = "Password"
        '
        'labelUserName
        '
        Me.labelUserName.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelUserName.Location = New System.Drawing.Point(7, 132)
        Me.labelUserName.Name = "labelUserName"
        Me.labelUserName.Size = New System.Drawing.Size(93, 21)
        Me.labelUserName.TabIndex = 26
        Me.labelUserName.Text = "User Name"
        '
        'labelTFTKey
        '
        Me.labelTFTKey.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelTFTKey.Location = New System.Drawing.Point(7, 104)
        Me.labelTFTKey.Name = "labelTFTKey"
        Me.labelTFTKey.Size = New System.Drawing.Size(93, 14)
        Me.labelTFTKey.TabIndex = 25
        Me.labelTFTKey.Text = "TFT Key"
        '
        'labelROCKey
        '
        Me.labelROCKey.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelROCKey.Location = New System.Drawing.Point(7, 76)
        Me.labelROCKey.Name = "labelROCKey"
        Me.labelROCKey.Size = New System.Drawing.Size(93, 21)
        Me.labelROCKey.TabIndex = 24
        Me.labelROCKey.Text = "ROC Key"
        '
        'labelRealm
        '
        Me.labelRealm.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelRealm.Location = New System.Drawing.Point(7, 49)
        Me.labelRealm.Name = "labelRealm"
        Me.labelRealm.Size = New System.Drawing.Size(93, 20)
        Me.labelRealm.TabIndex = 23
        Me.labelRealm.Text = "Realm"
        '
        'txtChannel
        '
        Me.txtChannel.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtChannel.Location = New System.Drawing.Point(107, 187)
        Me.txtChannel.Name = "txtChannel"
        Me.txtChannel.Size = New System.Drawing.Size(200, 20)
        Me.txtChannel.TabIndex = 6
        Me.txtChannel.Text = "The Void"
        '
        'txtPassword
        '
        Me.txtPassword.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtPassword.Location = New System.Drawing.Point(107, 159)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtPassword.Size = New System.Drawing.Size(200, 20)
        Me.txtPassword.TabIndex = 5
        Me.txtPassword.Text = "blacksheepwall"
        '
        'txtAccount
        '
        Me.txtAccount.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtAccount.Location = New System.Drawing.Point(107, 132)
        Me.txtAccount.Name = "txtAccount"
        Me.txtAccount.Size = New System.Drawing.Size(200, 20)
        Me.txtAccount.TabIndex = 4
        Me.txtAccount.Text = "lain"
        '
        'txtTFT
        '
        Me.txtTFT.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtTFT.Location = New System.Drawing.Point(107, 104)
        Me.txtTFT.MaxLength = 26
        Me.txtTFT.Name = "txtTFT"
        Me.txtTFT.Size = New System.Drawing.Size(200, 20)
        Me.txtTFT.TabIndex = 3
        Me.txtTFT.Text = "XXXXXXXXXXXXXXXXXXXXXXXXXX"
        '
        'txtROC
        '
        Me.txtROC.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtROC.Location = New System.Drawing.Point(107, 76)
        Me.txtROC.MaxLength = 26
        Me.txtROC.Name = "txtROC"
        Me.txtROC.Size = New System.Drawing.Size(200, 20)
        Me.txtROC.TabIndex = 2
        Me.txtROC.Text = "XXXXXXXXXXXXXXXXXXXXXXXXXX"
        '
        'groupParam
        '
        Me.groupParam.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.groupParam.Controls.Add(Me.txtHostPort)
        Me.groupParam.Controls.Add(Me.labelHostPort)
        Me.groupParam.Controls.Add(Me.txtWar3)
        Me.groupParam.Controls.Add(Me.comboRealm)
        Me.groupParam.Controls.Add(Me.labelBotServer)
        Me.groupParam.Controls.Add(Me.labelFirstChannel)
        Me.groupParam.Controls.Add(Me.labelPassword)
        Me.groupParam.Controls.Add(Me.labelUserName)
        Me.groupParam.Controls.Add(Me.labelTFTKey)
        Me.groupParam.Controls.Add(Me.labelROCKey)
        Me.groupParam.Controls.Add(Me.labelRealm)
        Me.groupParam.Controls.Add(Me.txtChannel)
        Me.groupParam.Controls.Add(Me.txtPassword)
        Me.groupParam.Controls.Add(Me.txtAccount)
        Me.groupParam.Controls.Add(Me.txtTFT)
        Me.groupParam.Controls.Add(Me.txtROC)
        Me.groupParam.Location = New System.Drawing.Point(220, 7)
        Me.groupParam.Name = "groupParam"
        Me.groupParam.Size = New System.Drawing.Size(314, 243)
        Me.groupParam.TabIndex = 0
        Me.groupParam.TabStop = False
        Me.groupParam.Text = "Warcraft Frozen Throne Configuration"
        '
        'txtHostPort
        '
        Me.txtHostPort.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtHostPort.Location = New System.Drawing.Point(107, 215)
        Me.txtHostPort.Name = "txtHostPort"
        Me.txtHostPort.Size = New System.Drawing.Size(200, 20)
        Me.txtHostPort.TabIndex = 7
        Me.txtHostPort.Text = "6000"
        '
        'labelHostPort
        '
        Me.labelHostPort.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelHostPort.Location = New System.Drawing.Point(7, 215)
        Me.labelHostPort.Name = "labelHostPort"
        Me.labelHostPort.Size = New System.Drawing.Size(93, 21)
        Me.labelHostPort.TabIndex = 34
        Me.labelHostPort.Text = "Host Port"
        '
        'txtWar3
        '
        Me.txtWar3.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtWar3.Location = New System.Drawing.Point(107, 21)
        Me.txtWar3.Name = "txtWar3"
        Me.txtWar3.Size = New System.Drawing.Size(200, 20)
        Me.txtWar3.TabIndex = 0
        Me.txtWar3.Text = "C:\Program Files\Warcraft III\"
        '
        'labelSummary
        '
        Me.labelSummary.BackColor = System.Drawing.Color.DarkGray
        Me.labelSummary.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelSummary.Location = New System.Drawing.Point(727, 483)
        Me.labelSummary.Name = "labelSummary"
        Me.labelSummary.Size = New System.Drawing.Size(167, 62)
        Me.labelSummary.TabIndex = 9
        Me.labelSummary.Text = "-=[Lain]=-"
        '
        'listChannelPeople
        '
        Me.listChannelPeople.Location = New System.Drawing.Point(728, 284)
        Me.listChannelPeople.Name = "listChannelPeople"
        Me.listChannelPeople.Size = New System.Drawing.Size(160, 147)
        Me.listChannelPeople.TabIndex = 2
        '
        'groupUser
        '
        Me.groupUser.Controls.Add(Me.buttonUserRemove)
        Me.groupUser.Controls.Add(Me.buttonUserAdd)
        Me.groupUser.Controls.Add(Me.txtUserName)
        Me.groupUser.Controls.Add(Me.listUser)
        Me.groupUser.Location = New System.Drawing.Point(7, 7)
        Me.groupUser.Name = "groupUser"
        Me.groupUser.Size = New System.Drawing.Size(94, 243)
        Me.groupUser.TabIndex = 10
        Me.groupUser.TabStop = False
        Me.groupUser.Text = "Admins"
        '
        'buttonUserRemove
        '
        Me.buttonUserRemove.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.buttonUserRemove.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonUserRemove.Location = New System.Drawing.Point(7, 215)
        Me.buttonUserRemove.Name = "buttonUserRemove"
        Me.buttonUserRemove.Size = New System.Drawing.Size(80, 20)
        Me.buttonUserRemove.TabIndex = 3
        Me.buttonUserRemove.Text = "Remove"
        '
        'buttonUserAdd
        '
        Me.buttonUserAdd.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.buttonUserAdd.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonUserAdd.Location = New System.Drawing.Point(7, 187)
        Me.buttonUserAdd.Name = "buttonUserAdd"
        Me.buttonUserAdd.Size = New System.Drawing.Size(80, 20)
        Me.buttonUserAdd.TabIndex = 2
        Me.buttonUserAdd.Text = "Add"
        '
        'txtUserName
        '
        Me.txtUserName.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtUserName.Location = New System.Drawing.Point(7, 159)
        Me.txtUserName.Name = "txtUserName"
        Me.txtUserName.Size = New System.Drawing.Size(80, 20)
        Me.txtUserName.TabIndex = 1
        '
        'listUser
        '
        Me.listUser.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.listUser.Location = New System.Drawing.Point(7, 21)
        Me.listUser.Name = "listUser"
        Me.listUser.Size = New System.Drawing.Size(80, 121)
        Me.listUser.TabIndex = 0
        '
        'groupGame
        '
        Me.groupGame.Controls.Add(Me.buttonGameInfo)
        Me.groupGame.Controls.Add(Me.buttonGameStop)
        Me.groupGame.Controls.Add(Me.listGame)
        Me.groupGame.Location = New System.Drawing.Point(107, 7)
        Me.groupGame.Name = "groupGame"
        Me.groupGame.Size = New System.Drawing.Size(107, 243)
        Me.groupGame.TabIndex = 11
        Me.groupGame.TabStop = False
        Me.groupGame.Text = "Games"
        '
        'buttonGameInfo
        '
        Me.buttonGameInfo.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.buttonGameInfo.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonGameInfo.Location = New System.Drawing.Point(7, 187)
        Me.buttonGameInfo.Name = "buttonGameInfo"
        Me.buttonGameInfo.Size = New System.Drawing.Size(93, 20)
        Me.buttonGameInfo.TabIndex = 4
        Me.buttonGameInfo.Text = "Info"
        '
        'buttonGameStop
        '
        Me.buttonGameStop.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.buttonGameStop.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonGameStop.Location = New System.Drawing.Point(7, 215)
        Me.buttonGameStop.Name = "buttonGameStop"
        Me.buttonGameStop.Size = New System.Drawing.Size(93, 20)
        Me.buttonGameStop.TabIndex = 3
        Me.buttonGameStop.Text = "Stop"
        '
        'listGame
        '
        Me.listGame.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.listGame.Location = New System.Drawing.Point(7, 21)
        Me.listGame.Name = "listGame"
        Me.listGame.Size = New System.Drawing.Size(93, 147)
        Me.listGame.TabIndex = 0
        '
        'botParam
        '
        Me.botParam.Controls.Add(Me.Label3)
        Me.botParam.Controls.Add(Me.TextBox3)
        Me.botParam.Controls.Add(Me.CheckBox3)
        Me.botParam.Controls.Add(Me.Label2)
        Me.botParam.Controls.Add(Me.TextBox2)
        Me.botParam.Controls.Add(Me.CheckBox2)
        Me.botParam.Controls.Add(Me.CheckBox1)
        Me.botParam.Controls.Add(Me.TextBox1)
        Me.botParam.Controls.Add(Me.Label1)
        Me.botParam.Location = New System.Drawing.Point(545, 7)
        Me.botParam.Name = "botParam"
        Me.botParam.Size = New System.Drawing.Size(342, 215)
        Me.botParam.TabIndex = 12
        Me.botParam.TabStop = False
        Me.botParam.Text = "Bot Settings"
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(182, 74)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(154, 21)
        Me.Label3.TabIndex = 47
        Me.Label3.Text = "# of Hosted Games Max"
        '
        'TextBox3
        '
        Me.TextBox3.Location = New System.Drawing.Point(7, 74)
        Me.TextBox3.Name = "TextBox3"
        Me.TextBox3.Size = New System.Drawing.Size(168, 20)
        Me.TextBox3.TabIndex = 46
        Me.TextBox3.Text = "255"
        '
        'CheckBox3
        '
        Me.CheckBox3.AutoSize = True
        Me.CheckBox3.Checked = True
        Me.CheckBox3.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CheckBox3.Font = New System.Drawing.Font("Trebuchet MS", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CheckBox3.Location = New System.Drawing.Point(6, 149)
        Me.CheckBox3.Name = "CheckBox3"
        Me.CheckBox3.Size = New System.Drawing.Size(158, 20)
        Me.CheckBox3.TabIndex = 45
        Me.CheckBox3.Text = "Lobby Refresh Message?"
        Me.CheckBox3.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(182, 47)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(108, 21)
        Me.Label2.TabIndex = 44
        Me.Label2.Text = "Command Trigger"
        '
        'TextBox2
        '
        Me.TextBox2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextBox2.Location = New System.Drawing.Point(6, 48)
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.Size = New System.Drawing.Size(170, 20)
        Me.TextBox2.TabIndex = 43
        Me.TextBox2.Text = "."
        '
        'CheckBox2
        '
        Me.CheckBox2.AutoSize = True
        Me.CheckBox2.Checked = True
        Me.CheckBox2.CheckState = System.Windows.Forms.CheckState.Checked
        Me.CheckBox2.Font = New System.Drawing.Font("Trebuchet MS", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CheckBox2.Location = New System.Drawing.Point(6, 98)
        Me.CheckBox2.Name = "CheckBox2"
        Me.CheckBox2.Size = New System.Drawing.Size(105, 20)
        Me.CheckBox2.TabIndex = 42
        Me.CheckBox2.Text = "LC Style Pings?"
        Me.CheckBox2.UseVisualStyleBackColor = True
        '
        'CheckBox1
        '
        Me.CheckBox1.AutoSize = True
        Me.CheckBox1.Font = New System.Drawing.Font("Trebuchet MS", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.CheckBox1.Location = New System.Drawing.Point(6, 124)
        Me.CheckBox1.Name = "CheckBox1"
        Me.CheckBox1.Size = New System.Drawing.Size(134, 20)
        Me.CheckBox1.TabIndex = 41
        Me.CheckBox1.Text = "Reconnect on Disc?"
        Me.CheckBox1.UseVisualStyleBackColor = True
        '
        'TextBox1
        '
        Me.TextBox1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TextBox1.Location = New System.Drawing.Point(6, 22)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(170, 20)
        Me.TextBox1.TabIndex = 39
        Me.TextBox1.Text = "UberAdmin"
        '
        'Label1
        '
        Me.Label1.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(182, 21)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(93, 21)
        Me.Label1.TabIndex = 40
        Me.Label1.Text = "Root Admin"
        '
        'frmLainEthLite
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(895, 547)
        Me.Controls.Add(Me.botParam)
        Me.Controls.Add(Me.groupGame)
        Me.Controls.Add(Me.groupUser)
        Me.Controls.Add(Me.listChannelPeople)
        Me.Controls.Add(Me.labelSummary)
        Me.Controls.Add(Me.labelChannel)
        Me.Controls.Add(Me.labelStatus)
        Me.Controls.Add(Me.buttonChat)
        Me.Controls.Add(Me.buttonGo)
        Me.Controls.Add(Me.groupParam)
        Me.Controls.Add(Me.txtChat)
        Me.Controls.Add(Me.txtChatLog)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Menu = Me.menuMain
        Me.MinimizeBox = False
        Me.Name = "frmLainEthLite"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.WindowState = System.Windows.Forms.FormWindowState.Minimized
        Me.groupParam.ResumeLayout(False)
        Me.groupParam.PerformLayout()
        Me.groupUser.ResumeLayout(False)
        Me.groupUser.PerformLayout()
        Me.groupGame.ResumeLayout(False)
        Me.botParam.ResumeLayout(False)
        Me.botParam.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region "menu"
    Private Sub menuConvertor_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles menuConvertor.Click
        Dim form As frmConvertor

        form = New frmConvertor
        form.Show()
    End Sub
    Private Sub menuExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles menuExit.Click
        ExitProgram()
    End Sub
    Private Sub menuAutoReconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub
    Private Sub menuLENPServer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles menuLENPServer.Click
        formLENPServer.Show()
        formLENPServer.BringToFront()
    End Sub
    Private Sub menuLENPClient_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles menuLENPClient.Click
        Dim form As frmLENPClient

        form = New frmLENPClient
        form.Show()
    End Sub
#End Region

#Region "xml management"
    Private Sub LoadAll()
        Dim ds As DataSet

        Try
            ds = New DataSet(ProjectLainName)
            If IO.File.Exists(String.Format("{0}\{1}.xml", Application.StartupPath, ProjectLainConfig)) Then
                ds.ReadXml(String.Format("{0}\{1}.xml", Application.StartupPath, ProjectLainConfig))
                If ds.Tables.Contains(ProjectLainConfig) Then
                    LoadConfigurationTable(ds.Tables(ProjectLainConfig))
                End If
            End If

            ds = New DataSet(ProjectLainName)
            If IO.File.Exists(String.Format("{0}\{1}.xml", Application.StartupPath, ProjectLainMap)) = False Then
                ds.Tables.Add(CreateMapTable)
                ds.WriteXml(String.Format("{0}\{1}.xml", Application.StartupPath, ProjectLainMap))
            End If
            ds.ReadXml(String.Format("{0}\{1}.xml", Application.StartupPath, ProjectLainMap))
            If ds.Tables.Contains(ProjectLainMap) Then
                LoadMapTable(ds.Tables(ProjectLainMap))
            End If

            ds = New DataSet(ProjectLainName)
            If IO.File.Exists(String.Format("{0}\{1}.xml", Application.StartupPath, ProjectLainUser)) Then
                ds.ReadXml(String.Format("{0}\{1}.xml", Application.StartupPath, ProjectLainUser))
                If ds.Tables.Contains(ProjectLainUser) Then
                    LoadUserTable(ds.Tables(ProjectLainUser))
                End If
            Else
                listUser.Items.Add("UberAdmin")
            End If

            ds = New DataSet(ProjectLainName)
            If IO.File.Exists(String.Format("{0}\{1}.xml", Application.StartupPath, ProjectLainCustomData)) Then
                ds.ReadXml(String.Format("{0}\{1}.xml", Application.StartupPath, ProjectLainCustomData))
                If ds.Tables.Contains(ProjectLainCustomData) Then
                    LoadCustomDataTable(ds.Tables(ProjectLainCustomData))
                End If
            End If

        Catch ex As Exception
        End Try
    End Sub

    Private Function LoadCustomDataTable(ByVal table As DataTable) As Boolean
        Dim row As DataRow
        Dim exeversion As String()
        Dim exeversionhash As String()
        Dim passwordhashtype As String
        Try
            customData = New clsBNETCustomData
            If table.Rows.Count > 0 Then
                row = table.Rows(0)

                If table.Columns.Contains("exeversion") Then
                    exeversion = CStr(row("exeversion")).Split(Convert.ToChar(" "))
                    If exeversion.Length = 4 Then
                        customData.SetExeVersion(New Byte() {CByte(exeversion(0)), CByte(exeversion(1)), CByte(exeversion(2)), CByte(exeversion(3))})
                    End If
                End If

                If table.Columns.Contains("exeversionhash") Then
                    exeversionhash = CStr(row("exeversionhash")).Split(Convert.ToChar(" "))
                    If exeversionhash.Length = 4 Then
                        customData.SetExeVersionHash(New Byte() {CByte(exeversionhash(0)), CByte(exeversionhash(1)), CByte(exeversionhash(2)), CByte(exeversionhash(3))})
                    End If
                End If

                If table.Columns.Contains("passwordhashtype") Then
                    passwordhashtype = CStr(row("passwordhashtype"))
                    If passwordhashtype IsNot Nothing Then
                        customData.SetPasswordHashType(passwordhashtype)
                    End If
                End If

            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function LoadUserTable(ByVal table As DataTable) As Boolean
        Dim row As DataRow
        CurrentAdminList.Clear()
        Try
            If table.Rows.Count > 0 AndAlso table.Columns.Contains("user") Then
                listUser.BeginUpdate()
                listUser.Items.Clear()
                For Each row In table.Rows
                    listUser.Items.Add(CStr(row("user")))
                    CurrentAdminList.Add(CStr(row("user")))
                Next
                listUser.EndUpdate()
                Return True
            End If

            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Private Function CreateMapTable() As DataTable
        Dim table As DataTable
        Dim column As DataColumn
        Dim row As DataRow
        Try
            table = New DataTable(ProjectLainMap)

            column = New DataColumn("mappath", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("mapsize", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("mapinfo", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("mapcrc", GetType(String))
            table.Columns.Add(column)

            row = table.NewRow

            row("mappath") = "Maps\Download\DotA Allstars v6.49b.w3x"
            row("mapsize") = "34 28 38 0"
            row("mapinfo") = "46 233 147 143"
            row("mapcrc") = "173 166 209 216"

            table.Rows.Add(row)
            Return table
        Catch ex As Exception
            Return New DataTable
        End Try
    End Function
    Private Function LoadMapTable(ByVal table As DataTable) As Boolean
        Dim row As DataRow
        Dim mappath As String
        Dim mapsize As String()
        Dim mapinfo As String()
        Dim mapcrc As String()
        Try
            If table.Rows.Count > 0 Then
                row = table.Rows(0)
                If table.Columns.Contains("mappath") AndAlso table.Columns.Contains("mapsize") AndAlso table.Columns.Contains("mapinfo") AndAlso table.Columns.Contains("mapcrc") Then
                    mappath = CStr(row("mappath"))
                    mapsize = CStr(row("mapsize")).Split(Convert.ToChar(" "))
                    mapinfo = CStr(row("mapinfo")).Split(Convert.ToChar(" "))
                    mapcrc = CStr(row("mapcrc")).Split(Convert.ToChar(" "))

                    If mappath.Length > 0 AndAlso mapsize.Length = 4 AndAlso mapinfo.Length = 4 AndAlso mapcrc.Length = 4 Then
                        map = New clsGameHostMap(mappath, _
                                                New Byte() {CByte(mapsize(0)), CByte(mapsize(1)), CByte(mapsize(2)), CByte(mapsize(3))}, _
                                                New Byte() {CByte(mapinfo(0)), CByte(mapinfo(1)), CByte(mapinfo(2)), CByte(mapinfo(3))}, _
                                                New Byte() {CByte(mapcrc(0)), CByte(mapcrc(1)), CByte(mapcrc(2)), CByte(mapcrc(3))})
                        Return True
                    End If
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Private Function LoadConfigurationTable(ByVal table As DataTable) As Boolean
        Dim row As DataRow
        Try
            If table.Rows.Count > 0 Then
                row = table.Rows(0)
                If table.Columns.Contains("war3path") Then
                    data.botSettings.wc3Path = CStr(row("war3path"))
                    txtWar3.Text = data.botSettings.wc3Path
                End If
                If table.Columns.Contains("realm") Then
                    data.botSettings.realm = CStr(row("realm"))
                    comboRealm.Text = data.botSettings.realm
                End If
                If table.Columns.Contains("ROCkey") Then
                    data.botSettings.rocKey = CStr(row("ROCkey"))
                    txtROC.Text = data.botSettings.rocKey
                End If
                If table.Columns.Contains("TFTkey") Then
                    data.botSettings.tftKey = CStr(row("TFTkey"))
                    txtTFT.Text = data.botSettings.tftKey
                End If
                If table.Columns.Contains("account") Then
                    data.botSettings.username = CStr(row("account"))
                    txtAccount.Text = data.botSettings.username
                End If
                If table.Columns.Contains("password") Then
                    data.botSettings.password = CStr(row("password"))
                    txtPassword.Text = data.botSettings.password
                End If
                If table.Columns.Contains("channel") Then
                    data.botSettings.homeChannel = CStr(row("channel"))
                    txtChannel.Text = data.botSettings.homeChannel
                End If
                If table.Columns.Contains("hostport") Then
                    data.botSettings.port = CStr(row("hostport"))
                    txtHostPort.Text = data.botSettings.port
                End If

                If table.Columns.Contains("commandtrigger") Then
                    data.botSettings.commandTrigger = CStr(row("commandtrigger"))
                    TextBox2.Text = data.botSettings.commandTrigger
                Else
                    TextBox2.Text = data.botSettings.commandTrigger
                End If
                If table.Columns.Contains("refreshmessage") Then
                    If CStr(row("refreshmessage")) = "True" Then
                        data.botSettings.enable_refreshDisplay = True
                    Else
                        data.botSettings.enable_refreshDisplay = False
                    End If
                    CheckBox3.Checked = data.botSettings.enable_refreshDisplay
                Else
                    data.botSettings.enable_refreshDisplay = True
                    CheckBox3.Checked = data.botSettings.enable_refreshDisplay
                End If
                If table.Columns.Contains("rootadmin") Then
                    RootAdmin = CStr(row("rootadmin"))
                    TextBox1.Text = CStr(row("rootadmin"))
                Else
                    RootAdmin = "UberAdmin"
                    TextBox1.Text = "UberAdmin"
                End If
                If table.Columns.Contains("reconnect") Then
                    If CStr(row("reconnect")) = "True" Then
                        data.botSettings.enable_reconnect = True
                    Else
                        data.botSettings.enable_reconnect = False
                    End If
                    CheckBox1.Checked = data.botSettings.enable_reconnect
                Else
                    data.botSettings.enable_reconnect = True
                    CheckBox1.Checked = data.botSettings.enable_reconnect
                End If
                If table.Columns.Contains("lcpings") Then
                    If CStr(row("lcpings")) = "True" Then
                        data.botSettings.enable_LCPings = True
                    Else
                        data.botSettings.enable_LCPings = False
                    End If
                    CheckBox2.Checked = data.botSettings.enable_LCPings
                Else
                    data.botSettings.enable_LCPings = True
                    CheckBox2.Checked = True
                End If
                If table.Columns.Contains("maxgames") Then
                    data.botSettings.maxGames = CByte(row("maxgames"))
                    TextBox3.Text = CStr(data.botSettings.maxGames)
                Else
                    data.botSettings.maxGames = 255
                    TextBox3.Text = CStr(data.botSettings.maxGames)
                End If

                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function
    Private Sub SaveAll()
        Dim ds As DataSet
        Try
            ds = New DataSet(ProjectLainName)
            ds.Tables.Add(SaveConfigurationTable)
            ds.WriteXml(String.Format("{0}\{1}.xml", Application.StartupPath, ProjectLainConfig))

            ds = New DataSet(ProjectLainName)
            ds.Tables.Add(SaveUserTable)
            ds.WriteXml(String.Format("{0}\{1}.xml", Application.StartupPath, ProjectLainUser))
        Catch ex As Exception
        End Try
    End Sub

    Private Function SaveUserTable() As DataTable
        Dim table As DataTable
        Dim column As DataColumn
        Dim row As DataRow
        Dim name As String
        Try
            table = New DataTable(ProjectLainUser)

            column = New DataColumn("user", GetType(String))
            table.Columns.Add(column)

            For Each name In listUser.Items
                row = table.NewRow
                row("user") = name
                table.Rows.Add(row)
            Next

            Return table
        Catch ex As Exception
            Return New DataTable
        End Try
    End Function
    Private Function SaveConfigurationTable() As DataTable
        Dim table As DataTable
        Dim column As DataColumn
        Dim row As DataRow
        Try
            table = New DataTable(ProjectLainConfig)

            column = New DataColumn("war3path", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("realm", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("ROCkey", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("TFTkey", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("account", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("password", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("channel", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("hostport", GetType(String))
            table.Columns.Add(column)



            column = New DataColumn("rootadmin", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("reconnect", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("lcpings", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("commandtrigger", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("refreshmessage", GetType(String))
            table.Columns.Add(column)
            column = New DataColumn("maxgames", GetType(String))
            table.Columns.Add(column)

            row = table.NewRow
            row("war3path") = txtWar3.Text
            row("realm") = comboRealm.Text
            row("ROCkey") = txtROC.Text
            row("TFTkey") = txtTFT.Text
            row("account") = data.botSettings.username
            row("password") = txtPassword.Text
            row("channel") = data.botSettings.homeChannel
            row("hostport") = txtHostPort.Text

            row("rootadmin") = RootAdmin
            row("reconnect") = data.botSettings.enable_reconnect
            row("lcpings") = data.botSettings.enable_LCPings
            row("commandtrigger") = data.botSettings.commandTrigger
            row("refreshmessage") = data.botSettings.enable_refreshDisplay
            row("maxgames") = data.botSettings.maxGames
            table.Rows.Add(row)

            Return table
        Catch ex As Exception
            Return New DataTable
        End Try
    End Function
#End Region

#Region "bnet event"

    Private Sub bnet_EventBnetSIDPING() Handles bnet.EventBnetSIDPING
        labelStatus.Invoke(New clsHelper.DelegateControlText(AddressOf clsHelper.ControlText), New Object() {labelStatus, String.Format("Memory Usage : {0} MB, Threads : {1}, Handles : {2}", Math.Round(Diagnostics.Process.GetCurrentProcess.PrivateMemorySize64 / 1024 / 1024, 2), Diagnostics.Process.GetCurrentProcess.Threads.Count, Diagnostics.Process.GetCurrentProcess.HandleCount)})
    End Sub

    Private Sub bnet_EventEngingState() Handles bnet.EventEngingState
        ReflectEngineState()
    End Sub
    Private Sub bnet_EventLogOnStatus(ByVal failpoint As clsBNET.LogOnFailPoints, ByVal info As String) Handles bnet.EventLogOnStatus
        labelSummary.Invoke(New clsHelper.DelegateControlText(AddressOf clsHelper.ControlText), New Object() {labelSummary, info})
    End Sub
    Private Sub bnet_OnEventMessage(ByVal server As clsCommandPacket.PacketType, ByVal msg As String) Handles bnet.EventMessage
        labelStatus.Invoke(New clsHelper.DelegateControlText(AddressOf clsHelper.ControlText), New Object() {labelStatus, String.Format("{0} -> {1}", server, msg)})
    End Sub
    Private Sub bnet_OnEventError(ByVal errorFunction As String, ByVal errorString As String) Handles bnet.EventError
        labelStatus.Invoke(New clsHelper.DelegateControlText(AddressOf clsHelper.ControlText), New Object() {labelStatus, String.Format("{0} -> {1}", "Error", errorFunction)})
        txtChatLog.Invoke(New clsHelper.DelegateTextBoxAppend(AddressOf clsHelper.ControlTextBoxAppend), New Object() {txtChatLog, String.Format("ERROR[{0}]:{1}{2}", errorFunction, errorString, Environment.NewLine)})
    End Sub
    Private Sub bnetchannel_OnEventChannelChange() Handles channel.EventChannelChange
        If channel.GetChannelName.Length = 0 Then
            bnet.GoChannel("The Void")
        Else
            labelChannel.Invoke(New clsHelper.DelegateControlText(AddressOf clsHelper.ControlText), New Object() {labelChannel, channel.GetChannelName})
            listChannelPeople.Invoke(New clsHelper.DelegateControlListBoxDataSource(AddressOf clsHelper.ControlListBoxDataSource), New Object() {listChannelPeople, channel.GetChannelPeopleList})
        End If
    End Sub
    Private Sub bnet_EventIncomingChat(ByVal eventChat As clsIncomingChatChannel) Handles bnet.EventIncomingChat
        channel.Process(eventChat)
        bot.ProcessCommand(eventChat)
    End Sub
    Private Sub bnetchannel_OnEventChannelChatMessage(ByVal msg As String) Handles channel.EventChannelChatMessage
        txtChatLog.Invoke(New clsHelper.DelegateTextBoxAppend(AddressOf clsHelper.ControlTextBoxAppend), New Object() {txtChatLog, String.Format("{0}{1}", msg, Environment.NewLine)})
    End Sub

    Private Sub bnet_EventBnetSIDSTARTADVEX3Result(ByVal isOK As Boolean) Handles bnet.EventBnetSIDSTARTADVEX3Result
        If isOK Then
            'out of chat
        Else
            currentHost.Dispose("Host Game Failed")
            SendChat("/me : Game Failed to be Hosted, try another name")
        End If
    End Sub

#End Region

#Region "host event"
    Private Sub host_EventHostUncreate()
        Debug.WriteLine("uncreated")
        bnet.GameUnCreate()
        Debug.WriteLine("after trysend")
        If bnet.IsSendChatAble = False Then
            bnet.EnterChat()
        End If
        Debug.WriteLine("after enter chat")
    End Sub
    Private Sub host_EventHostDisposed(ByVal host As clsGameHost, ByVal reason As String)
        Try
            If currentHost Is host Then
                currentHost = New clsGameHost
            End If

            If listHost.Contains(host) Then
                listHost.Remove(host)
                UpdateGameList()

                RemoveHandler host.EventHostUncreate, AddressOf host_EventHostUncreate
                RemoveHandler host.EventHostDisposed, AddressOf host_EventHostDisposed
                RemoveHandler host.EventGameWon, AddressOf host_EventGameWon
            End If

            SendChat(String.Format("[{0}] : {1}", host.GetGameName, reason), ProjectLainVersion, True)
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try

    End Sub
    Private Sub host_EventGameWon(ByVal callerName As String, ByVal gameName As String, ByVal sentinelPlayer() As String, ByVal scourgePlayer() As String, ByVal referee() As String, ByVal winner As String)
        'SendChat(String.Format("Game:[{0}] Result Win -> [{1}]", gameName, winner))
        SendChat(String.Format("{0} wins the game {1}", winner, gameName))
    End Sub
#End Region

#Region "bot channel"
    Private Sub bot_EventBotMap(ByVal isWhisper As Boolean, ByVal owner As String, ByVal mapName As String) Handles bot.EventBotMap
        Dim ds As DataSet
        Dim msg As String

        Try
            ds = New DataSet(ProjectLainName)

            mapName = mapName.ToLower
            If mapName.EndsWith(".xml") Then
                mapName = mapName.Substring(0, mapName.Length - 4)
            End If

            If IO.File.Exists(String.Format("{0}\{1}.xml", Application.StartupPath, mapName)) Then
                ds.ReadXml(String.Format("{0}\{1}.xml", Application.StartupPath, mapName))
                If ds.Tables.Contains(ProjectLainMap) Then
                    If LoadMapTable(ds.Tables(ProjectLainMap)) Then
                        msg = String.Format("Map file [{0}] Loaded Succesfully", mapName)
                    Else
                        msg = String.Format("Map file [{0}] Fail to Load", mapName)
                    End If
                Else
                    msg = String.Format("Map file [{0}] not Valid", mapName)
                End If
            Else
                msg = String.Format("Map file [{0}] not Found", mapName)
            End If

            If isWhisper Then
                SendChat(String.Format("/w {0} {1}", owner, msg))
            Else
                SendChat(String.Format("/me : {0}", msg))
            End If

        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try


    End Sub
    Private Sub bot_EventBotSay(ByVal msg As String) Handles bot.EventBotSay
        SendChat(msg)
    End Sub
    Private Sub bot_EventBotResponse(ByVal msg As String, ByVal isWhisper As Boolean, ByVal owner As String) Handles bot.EventBotResponse
        If isWhisper Then
            SendChat(String.Format("/w {0} {1}", owner, msg))
        Else
            SendChat(String.Format("/me : {0}", msg))
        End If
    End Sub
    Private Sub bot_EventBotHost(ByVal isPublic As Boolean, ByVal numPlayers As Integer, ByVal gameName As String, ByVal callerName As String, ByVal isWhisper As Boolean, ByVal owner As String) Handles bot.EventBotHost
        If listHost.Count < data.botSettings.maxGames Then
            Dim host As clsGameHost
            Dim state As Byte

            If isPublic Then
                state = GAME_PUBLIC
            Else
                state = GAME_PRIVATE
            End If

            host = New clsGameHost(GetAdminList, state, numPlayers, gameName, bnet.GetUniqueUserName, callerName, bnet.GetHostPort, map.GetMapPath, map.GetMapSize, map.GetMapInfo, map.GetMapCRC, bnet, data)
            If host.HostStart Then
                listHost.Add(host)
                UpdateGameList()

                currentHost = host
                AddHandler host.EventHostUncreate, AddressOf host_EventHostUncreate
                AddHandler host.EventHostDisposed, AddressOf host_EventHostDisposed
                AddHandler host.EventGameWon, AddressOf host_EventGameWon

                If isWhisper Then
                    SendChat(String.Format("/w {1} Creating Game: [{0}] started by [{1}]...", host.GetGameName, host.GetCallerName))
                Else
                    SendChat(String.Format("/me : Creating Game: [{0}] started by [{1}]...", host.GetGameName, host.GetCallerName))
                End If

                'Threading.Thread.Sleep(1000)
                bnet.GetFriendList()    'MrJag|0.8c|reserve|get the current list of friends
                bnet.GetClanList()      'MrJag|0.8c|reserve|get the current list of clan members
                bnet.GameCreate(labelChannel.Text, state, numPlayers, gameName, bnet.GetUniqueUserName, 0, map.GetMapPath, map.GetMapCRC)
            Else
                If currentHost.GetGameName.Length > 0 Then
                    If isWhisper Then
                        SendChat(String.Format("/w {0} Game: [{1}] is waiting", owner, currentHost.GetGameName))
                    Else
                        SendChat(String.Format("/me : Game: [{0}] is waiting", currentHost.GetGameName))
                    End If
                Else
                    If isWhisper Then
                        SendChat(String.Format("/w {0} Hosting Failed, Possible Port In Use", owner))
                    Else
                        SendChat(String.Format("/me : Hosting Failed, Possible Port In Use"))
                    End If
                End If
            End If
        Else
            If isWhisper Then
                SendChat(String.Format("/w {0}: Maxgames has been reached ({1})", callerName, data.botSettings.maxGames))
            Else
                SendChat(String.Format("/me : Maxgames has been reached ({0})", data.botSettings.maxGames))
            End If
        End If
    End Sub
    'Netrunner|0.1|root admin commands|
    Private Sub bot_EventToggleReconnect(ByVal toggle As Boolean) Handles bot.EventBotToggleReconnect
        If toggle = True Then
            data.botSettings.enable_refreshDisplay = True
            SendChat(String.Format("/w {0} Reconnect is ON", RootAdmin))
        Else
            data.botSettings.enable_refreshDisplay = False
            SendChat(String.Format("/w {0} Reconnect is OFF", RootAdmin))
        End If
        CheckBox1.Checked = data.botSettings.enable_reconnect
    End Sub
    'Netrunner|0.1|root admin commands|
    Private Sub bot_EventChangeChannel(ByVal channelName As String) Handles bot.EventBotChangeChannel
        data.botSettings.homeChannel = channelName
        bnet.SendChatToQueue(New clsBNETChatMessage(String.Format("/channel {0}", channelName), data.botSettings.username, False))
        SendChat(String.Format("/w {0} Moved to channel {1}", RootAdmin, data.botSettings.homeChannel))
    End Sub
    'Netrunner|0.1|root admin commands|
    Private Sub bot_EventModifyAdmin(ByVal name As String, ByVal add As Boolean) Handles bot.EventBotModifyAdmin
        If add = True Then
            If name.Length > 0 AndAlso listUser.Items.Contains(name) = False Then
                listUser.Items.Add(name)
                SaveUserTable()
                CurrentAdminList.Add(name)
                bot = New clsBotCommandHostChannel(GetAdminList())
                SendChat(String.Format("/w {0} {1} is an admin now.", RootAdmin, name))
            Else
                SendChat(String.Format("/w {0} {1} is already an admin.", RootAdmin, name))
            End If
        Else
            If name.Length > 0 AndAlso listUser.Items.Contains(name) Then
                listUser.Items.Remove(name)
                SaveUserTable()
                CurrentAdminList.Remove(name)
                bot = New clsBotCommandHostChannel(GetAdminList())
                SendChat(String.Format("/w {0} {1} is no longer an admin.", RootAdmin, name))
            Else
                SendChat(String.Format("/w {0} {1} was never an admin.", RootAdmin, name))
            End If
        End If
    End Sub
    Private Sub bot_EventMaxGames(ByVal max As Byte) Handles bot.EventBotMaxGames
        If max >= 0 And max <= 255 Then
            Dim oldvalue = data.botSettings.maxGames
            data.botSettings.maxGames = max
            If max > oldvalue Then
                SendChat(String.Format("/w {0} the max gamecap has been increased from {1} games to {2} games.", RootAdmin, oldvalue, max))
            Else
                If max = oldvalue Then
                    SendChat(String.Format("/w {0} the max gamecap was {1} games.", RootAdmin, max))
                Else
                    SendChat(String.Format("/w {0} the max gamecap has been decreased from {1} games to {2} games.", RootAdmin, oldvalue, max))
                End If
            End If
        End If
    End Sub
    Private Sub bot_EventBotBan(ByVal name As String, ByVal reason As String) Handles bot.EventBotBan
        Dim user As clsUser = data.userList.getUser(name)
        If user.name.Length > 0 Then
            user.ban = reason
            SendChat(String.Format("Banned: {0} -- {1}.", name, reason))
        End If
    End Sub
    Private Sub bot_EventBotTest(ByVal text As String) Handles bot.EventBotTest
        If text = "save" Then
            data.save_botSettings()
        ElseIf text = "load" Then
            data.load_botSettings()
        End If


    End Sub

    Private Sub bot_EventBotUnHost(ByVal isWhisper As Boolean, ByVal owner As String) Handles bot.EventBotUnHost
        Dim gamename As String
        Dim result As String

        If currentHost.GetGameName.Length > 0 Then
            gamename = currentHost.GetGameName

            If currentHost.GetIsCountDownStarted = False Then
                currentHost.Dispose("Game Unhosted Manually")
                result = String.Format("Game [{0}] is stopped", gamename)
            Else
                result = String.Format("Game [{0}] already started", gamename)
            End If
        Else
            result = String.Format("No valid targeted game")
        End If

        If isWhisper Then
            SendChat(String.Format("/w {0} {1}", owner, result))
        Else
            SendChat(String.Format("/me : {0}", result))
        End If
    End Sub
    Private Sub bot_EventBotGetGames(ByVal isWhisper As Boolean, ByVal owner As String) Handles bot.EventBotGetGames
        Dim host As clsGameHost
        Dim result As System.Text.StringBuilder

        result = New System.Text.StringBuilder
        SyncLock listHost.SyncRoot
            For Each host In listHost
                If host.GetIsCountDownStarted = False Then
                    result.Append(String.Format("[{0} : {1} : {2}], ", host.GetGameName, host.GetCallerName, host.GetTotalPlayers))
                Else
                    result.Append(String.Format("[{0}], ", host.GetGameName))
                End If
            Next
        End SyncLock

        If result.Length = 0 Then
            result.Append("No games in progress")
        End If

        If isWhisper Then
            SendChat(String.Format("/w {0} {1}", owner, result))
        Else
            SendChat(String.Format("/me : {0}", result))
        End If

    End Sub

#End Region

    Private Sub frmLainEthLite_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        listHost = ArrayList.Synchronized(New ArrayList)
        currentHost = New clsGameHost
        map = New clsGameHostMap
        customData = New clsBNETCustomData
        bnet = New clsBNET
        bot = New clsBotCommandHostChannel(New String() {})
        channel = New clsBNETChannel

        Me.Text = ProjectLainVersion
        txtChatLog.AppendText(Environment.NewLine)
        trayIcon.ContextMenu = contextMain
        trayIcon.Text = ProjectLainVersion
        Me.ReflectEngineState()
        Me.buttonGo.Focus()

        CurrentAdminList = New ArrayList

        aliveTimer = New Timers.Timer
        aliveTimer.Interval = 1000
        aliveCounter = 0
        channelJoinCounter = 0

        LoadAll()

        formLENPServer = New frmLENPServer
        lenp = New clsLENPServer(listHost)

        Dim ResultFromParams = ParseCommandLineArgs()
        If CBool((1 And ResultFromParams)) Then 'no gui its hidden
            toggleGUI()
            If CBool((10 And ResultFromParams)) Then 'also no trayicon
                Me.trayIcon.Visible = False
            End If
            Start()
        Else
            Me.WindowState = FormWindowState.Normal
            Me.trayIcon.Visible = True
        End If
    End Sub
    Private Sub frmLainEthLite_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        e.Cancel = True
        Me.Visible = False
    End Sub

    Private Sub txtChat_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtChat.TextChanged
        If txtChat.Text.Length > 0 Then
            txtChat.Text = TrimNewLine(txtChat.Text)
        End If
    End Sub
    Private Sub txtChat_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtChat.KeyPress
        If txtChat.Text.Length > 0 AndAlso e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Return) Then
            buttonChat_Click("txtChat_KeyPress", New EventArgs)
        End If
    End Sub
    Private Sub txtChatLog_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtChatLog.TextChanged
        If txtChatLog.Text.Length > txtChatLog.MaxLength * 0.9 Then
            txtChatLog.Text = txtChatLog.Text.Substring(CInt(txtChatLog.Text.Length * 0.7))
        End If
    End Sub
    Private Sub txtHostPort_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtHostPort.LostFocus
        If IsNumeric(txtHostPort.Text) AndAlso CType(txtHostPort.Text, Integer) > 0 AndAlso CType(txtHostPort.Text, Integer) < 32000 Then
            Return
        Else
            txtHostPort.Text = "6000"
            MessageBox.Show("Invalid Host Port, Reverting to Default = 6000", ProjectLainVersion, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub
    Private Sub txtWar3_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtWar3.LostFocus
        Dim file_war3_exe As String = "war3.exe"
        Dim file_storm_dll As String = "storm.dll"
        Dim file_game_dll As String = "game.dll"
        Dim war3path As String

        If txtWar3.Text.EndsWith("\") = False Then
            txtWar3.Text = txtWar3.Text & "\"
        End If

        war3path = txtWar3.Text
        If Directory.Exists(war3path) Then
            If IO.File.Exists(war3path & file_war3_exe) AndAlso IO.File.Exists(war3path & file_storm_dll) AndAlso IO.File.Exists(war3path & file_game_dll) Then
                txtWar3.ForeColor = Color.Black
                Return
            End If
        End If
        txtWar3.ForeColor = Color.Red
    End Sub
    Private Sub buttonChat_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonChat.Click
        SendChat(txtChat.Text)
        txtChat.Text = ""
        txtChat.Focus()
    End Sub
    Private Sub trayIcon_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles trayIcon.DoubleClick
        contextShow_Click("trayIcon_DoubleClick", New EventArgs)
    End Sub
    Private Sub contextShow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles contextShow.Click
        If Me.Visible = False Then
            Me.Visible = True
            Me.WindowState = FormWindowState.Normal
        Else
            Me.Visible = False
        End If
    End Sub
    Private Sub contextExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles contextExit.Click
        ExitProgram()
    End Sub
    Private Sub buttonGo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonGo.Click
        toggleGUI()
        SaveAll()
        Start()
    End Sub
    'Netrunner|0.1|console mode|
    Private Function ParseCommandLineArgs() As Integer
        '  Invoke this sample with an arbitrary set of command line arguments.
        Dim arguments As [String]() = Environment.GetCommandLineArgs()

        '''''''''
        Dim inputArgument As String = "-"
        Dim bits As Integer = 0

        For Each s As String In arguments
            If s.ToLower.StartsWith(inputArgument) AndAlso s.ToLower() = "-nogui" AndAlso CBool((1 And bits)) = False Then
                bits += 1
            End If
            If s.ToLower.StartsWith(inputArgument) AndAlso s.ToLower() = "-notrayicon" AndAlso CBool((10 And bits)) = False Then
                bits += 2
            End If
        Next
        Return bits
    End Function
    'Netrunner|0.1|gui enhancements|
    Private Sub toggleGUI()
        If groupParam.Enabled = True Then
            buttonUserAdd.Enabled = False
            buttonUserRemove.Enabled = False
            txtUserName.Enabled = False
            groupParam.Enabled = False
            botParam.Enabled = False
        Else
            buttonUserAdd.Enabled = True
            buttonUserRemove.Enabled = True
            txtUserName.Enabled = True
            groupParam.Enabled = True
            botParam.Enabled = True
        End If
    End Sub
    Private Function Start() As Boolean
        If listUser.Items.Contains(RootAdmin) = False Then
            listUser.Items.Add(RootAdmin)
            SaveUserTable()
            CurrentAdminList.Add(RootAdmin)
        End If
        Try

            If bnet.IsEngineRunning() Then
                bnet.Stop()
                aliveTimer.Stop()
            Else
                'Netrunner|0.1|root admin commands|
                If data.botSettings.enable_reconnect = True Then
                    aliveTimer.Start()
                End If

                listChannelPeople.Invoke(New clsHelper.DelegateControlListBoxDataSource(AddressOf clsHelper.ControlListBoxDataSource), New Object() {listChannelPeople, New Object() {}})
                labelSummary.Invoke(New clsHelper.DelegateControlText(AddressOf clsHelper.ControlText), New Object() {labelSummary, "Connecting to BattleNET..."})

                bot = New clsBotCommandHostChannel(GetAdminList())
                bnet.Start( _
                CType(comboRealm.Invoke(New clsHelper.DelegateControlTextGet(AddressOf clsHelper.ControlTextGet), New Object() {comboRealm}), String), _
                CType(txtWar3.Invoke(New clsHelper.DelegateControlTextGet(AddressOf clsHelper.ControlTextGet), New Object() {txtWar3}), String), _
                CType(txtROC.Invoke(New clsHelper.DelegateControlTextGet(AddressOf clsHelper.ControlTextGet), New Object() {txtROC}), String), _
                CType(txtTFT.Invoke(New clsHelper.DelegateControlTextGet(AddressOf clsHelper.ControlTextGet), New Object() {txtTFT}), String), _
                CType(txtAccount.Invoke(New clsHelper.DelegateControlTextGet(AddressOf clsHelper.ControlTextGet), New Object() {txtAccount}), String), _
                CType(txtPassword.Invoke(New clsHelper.DelegateControlTextGet(AddressOf clsHelper.ControlTextGet), New Object() {txtPassword}), String), _
                CType(txtChannel.Invoke(New clsHelper.DelegateControlTextGet(AddressOf clsHelper.ControlTextGet), New Object() {txtChannel}), String), _
                CType(txtHostPort.Invoke(New clsHelper.DelegateControlTextGet(AddressOf clsHelper.ControlTextGet), New Object() {txtHostPort}), Integer), _
                customData)

            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try

    End Function
    Private Function GetAdminList() As String()
        Try
            Return CType(CurrentAdminList.ToArray(GetType(String)), String())
        Catch ex As Exception
            Debug.WriteLine(ex)
            Return New String() {}
        End Try
    End Function
    Private Sub SendChat(ByVal chat As String)
        bnet.SendChatToQueue(New clsBNETChatMessage(TrimNewLine(chat), ProjectLainVersion, False))
    End Sub
    Private Sub SendChat(ByVal chat As String, ByVal owner As String, ByVal isPersistent As Boolean)
        bnet.SendChatToQueue(New clsBNETChatMessage(TrimNewLine(chat), owner, isPersistent))
    End Sub

    Private Sub ReflectEngineState()
        If bnet.IsEngineRunning() Then
            LockControl(True)
            labelStatus.Invoke(New clsHelper.DelegateControlBackColor(AddressOf clsHelper.ControlBackColor), New Object() {labelStatus, Color.CornflowerBlue})
            labelSummary.Invoke(New clsHelper.DelegateControlBackColor(AddressOf clsHelper.ControlBackColor), New Object() {labelSummary, Color.CornflowerBlue})
        Else
            LockControl(False)
            labelStatus.Invoke(New clsHelper.DelegateControlBackColor(AddressOf clsHelper.ControlBackColor), New Object() {labelStatus, Color.DarkGray})
            labelSummary.Invoke(New clsHelper.DelegateControlBackColor(AddressOf clsHelper.ControlBackColor), New Object() {labelSummary, Color.DarkGray})

        End If
    End Sub
    Private Sub LockControl(ByVal lock As Boolean)
        comboRealm.Invoke(New clsHelper.DelegateControlEnabled(AddressOf clsHelper.ControlEnabled), New Object() {comboRealm, Not lock})
        txtWar3.Invoke(New clsHelper.DelegateControlTextBoxReadOnly(AddressOf clsHelper.ControlTextBoxReadOnly), New Object() {txtWar3, lock})
        txtROC.Invoke(New clsHelper.DelegateControlTextBoxReadOnly(AddressOf clsHelper.ControlTextBoxReadOnly), New Object() {txtROC, lock})
        txtTFT.Invoke(New clsHelper.DelegateControlTextBoxReadOnly(AddressOf clsHelper.ControlTextBoxReadOnly), New Object() {txtTFT, lock})
        txtAccount.Invoke(New clsHelper.DelegateControlTextBoxReadOnly(AddressOf clsHelper.ControlTextBoxReadOnly), New Object() {txtAccount, lock})
        txtPassword.Invoke(New clsHelper.DelegateControlTextBoxReadOnly(AddressOf clsHelper.ControlTextBoxReadOnly), New Object() {txtPassword, lock})
        txtChannel.Invoke(New clsHelper.DelegateControlTextBoxReadOnly(AddressOf clsHelper.ControlTextBoxReadOnly), New Object() {txtChannel, lock})
        txtHostPort.Invoke(New clsHelper.DelegateControlTextBoxReadOnly(AddressOf clsHelper.ControlTextBoxReadOnly), New Object() {txtHostPort, lock})
        listChannelPeople.Invoke(New clsHelper.DelegateControlEnabled(AddressOf clsHelper.ControlEnabled), New Object() {listChannelPeople, lock})
        'listUser.Invoke(New clsHelper.DelegateControlEnabled(AddressOf clsHelper.ControlEnabled), New Object() {listUser, Not lock})
        txtUserName.Invoke(New clsHelper.DelegateControlTextBoxReadOnly(AddressOf clsHelper.ControlTextBoxReadOnly), New Object() {txtUserName, lock})
        buttonUserAdd.Invoke(New clsHelper.DelegateControlEnabled(AddressOf clsHelper.ControlEnabled), New Object() {buttonUserAdd, Not lock})
        buttonUserRemove.Invoke(New clsHelper.DelegateControlEnabled(AddressOf clsHelper.ControlEnabled), New Object() {buttonUserRemove, Not lock})
    End Sub
    Private Function TrimNewLine(ByVal msg As String) As String
        Try
            Return msg.Replace(Environment.NewLine, "")
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Private Sub ExitProgram()
        Try
            SaveAll()
            bnet.Stop()
            Me.Enabled = False
            trayIcon.Visible = False
            Me.Dispose()
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub
    Private Sub aliveTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles aliveTimer.Elapsed
        Static isRunning As Boolean = False
        Try
            If isRunning = False Then
                isRunning = True

                If bnet.IsEngineRunning = False Then
                    If aliveCounter > 0 Then
                        aliveCounter = aliveCounter - 1
                        labelStatus.Invoke(New clsHelper.DelegateControlText(AddressOf clsHelper.ControlText), New Object() {labelStatus, String.Format("Auto-Reconnect will initialise in {0} seconds", aliveCounter)})
                    Else
                        Start()
                        aliveCounter = 60
                    End If
                Else
                    aliveCounter = 60

                    channelJoinCounter = (channelJoinCounter + 1) Mod 60
                    If channelJoinCounter = 0 AndAlso bnet.IsSendChatAble Then
                        If labelChannel.Text.ToLower = "the void" Then
                            bnet.SendChatToQueue(New clsBNETChatMessage(String.Format("/channel {0}", txtChannel.Text), txtAccount.Text, False))
                        Else
                            bnet.SendChatToQueue(New clsBNETChatMessage(String.Format("/rejoin"), txtAccount.Text, False))
                        End If

                    End If

                End If

                isRunning = False
            End If
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub


    Private Sub listUser_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles listUser.SelectedIndexChanged
        txtUserName.Text = CType(listUser.SelectedItem, String)
    End Sub
    Private Sub buttonUserAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonUserAdd.Click
        Dim name As String
        name = txtUserName.Text.Trim
        If name.Length > 0 AndAlso listUser.Items.Contains(name) = False Then
            listUser.Items.Add(name)
            SaveUserTable()
            CurrentAdminList.Add(name)
            txtUserName.Text = ""
        End If
    End Sub
    Private Sub buttonUserRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonUserRemove.Click
        Dim name As String
        name = txtUserName.Text.Trim
        If name.Length > 0 AndAlso listUser.Items.Contains(name) Then
            listUser.Items.Remove(name)
            SaveUserTable()
            CurrentAdminList.Remove(name)
        End If
    End Sub
    Private Sub UpdateGameList()
        Dim host As clsGameHost
        Dim list As ArrayList

        list = New ArrayList
        SyncLock listHost.SyncRoot
            For Each host In listHost
                list.Add(host.GetGameName)
            Next
        End SyncLock

        listGame.Invoke(New clsHelper.DelegateControlListBoxDataSource(AddressOf clsHelper.ControlListBoxDataSource), New Object() {listGame, list.ToArray(GetType(String))})

    End Sub
    Private Sub buttonGameInfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonGameInfo.Click
        Dim gamename As String
        Dim host As clsGameHost

        If listGame.SelectedItem Is Nothing = False AndAlso CType(listGame.SelectedItem, String).Length > 0 Then
            gamename = CType(listGame.SelectedItem, String)
            For Each host In listHost
                If host.GetGameName = gamename Then

                    MessageBox.Show(String.Format("Game: {0} currently with {1} Players, Game Started -> {2} ", host.GetGameName, host.GetTotalPlayers, host.GetIsCountDownStarted), ProjectLainVersion, MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Exit For
                End If
            Next
        End If
    End Sub
    Private Sub buttonGameStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonGameStop.Click
        Dim gamename As String
        Dim host As clsGameHost

        If listGame.SelectedItem Is Nothing = False AndAlso CType(listGame.SelectedItem, String).Length > 0 Then
            gamename = CType(listGame.SelectedItem, String)
            For Each host In listHost
                If host.GetGameName = gamename Then
                    If MessageBox.Show(String.Format("Are You Sure You Want To Force Stop Game: {0} ?", host.GetGameName), ProjectLainVersion, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                        host.Dispose("Game Stopped Manually")
                    End If
                    Exit For
                End If
            Next
        End If
    End Sub

    Public Function GetLENPServer() As clsLENPServer
        Return lenp
    End Function

    'Netrunner|0.1|txt revolution|
    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        RootAdmin = TextBox1.Text
    End Sub
    'Netrunner|0.1|txt revolution|
    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        data.botSettings.enable_reconnect = CheckBox1.Checked
    End Sub
    'Netrunner|0.1|txt revolution|
    Private Sub txtChannel_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtChannel.TextChanged
        data.botSettings.homeChannel = txtChannel.Text
    End Sub

    Private Sub txtAccount_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtAccount.TextChanged
        data.botSettings.username = txtAccount.Text
    End Sub
    Private Sub TextBox2_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox2.TextChanged
        If TextBox2.TextLength > 0 Then
            data.botSettings.commandTrigger = TextBox2.Text.Substring(0, 1)
        End If
        TextBox2.Text = data.botSettings.commandTrigger
    End Sub
    Private Sub CheckBox3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox3.CheckedChanged
        data.botSettings.enable_refreshDisplay = CheckBox3.Checked
    End Sub

    Private Sub CheckBox2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox2.CheckedChanged
        data.botSettings.enable_LCPings = CheckBox2.Checked
    End Sub

    Private Sub trayIcon_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles trayIcon.MouseDown
        Me.TopMost = True
    End Sub
    Private Sub trayIcon_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles trayIcon.MouseUp
        Me.TopMost = False
    End Sub

    Private Sub TextBox3_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox3.TextChanged
        If IsNumeric(TextBox3.Text) AndAlso TextBox3.Text <> "" Then
            Dim value As Integer = CInt(TextBox3.Text)
            value = Math.Min(value, 255)
            value = Math.Max(value, 0)
            data.botSettings.maxGames = CByte(value)
            TextBox3.Text = CStr(data.botSettings.maxGames)
        Else
            TextBox3.Text = CStr(data.botSettings.maxGames)
        End If
    End Sub
End Class