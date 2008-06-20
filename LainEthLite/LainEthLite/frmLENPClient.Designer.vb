<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmLENPClient
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmLENPClient))
        Me.txtChatLog = New System.Windows.Forms.TextBox
        Me.buttonGo = New System.Windows.Forms.Button
        Me.txtIP = New System.Windows.Forms.TextBox
        Me.txtPort = New System.Windows.Forms.TextBox
        Me.labelIP = New System.Windows.Forms.Label
        Me.labelPort = New System.Windows.Forms.Label
        Me.groupConnection = New System.Windows.Forms.GroupBox
        Me.labelStatus = New System.Windows.Forms.Label
        Me.comboDataType = New System.Windows.Forms.ComboBox
        Me.txtInput = New System.Windows.Forms.TextBox
        Me.txtBuffer = New System.Windows.Forms.TextBox
        Me.buttonAdd = New System.Windows.Forms.Button
        Me.buttonDelete = New System.Windows.Forms.Button
        Me.buttonClear = New System.Windows.Forms.Button
        Me.groupIO = New System.Windows.Forms.GroupBox
        Me.checkASCII = New System.Windows.Forms.CheckBox
        Me.checkHex = New System.Windows.Forms.CheckBox
        Me.checkDec = New System.Windows.Forms.CheckBox
        Me.buttonSend = New System.Windows.Forms.Button
        Me.buttonAssignLength = New System.Windows.Forms.Button
        Me.groupConnection.SuspendLayout()
        Me.groupIO.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtChatLog
        '
        Me.txtChatLog.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtChatLog.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtChatLog.Location = New System.Drawing.Point(6, 131)
        Me.txtChatLog.Multiline = True
        Me.txtChatLog.Name = "txtChatLog"
        Me.txtChatLog.ReadOnly = True
        Me.txtChatLog.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtChatLog.Size = New System.Drawing.Size(592, 194)
        Me.txtChatLog.TabIndex = 2
        Me.txtChatLog.Text = "======================" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Returned Data Log" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "======================" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        Me.txtChatLog.WordWrap = False
        '
        'buttonGo
        '
        Me.buttonGo.Font = New System.Drawing.Font("Courier New", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonGo.Location = New System.Drawing.Point(12, 9)
        Me.buttonGo.Name = "buttonGo"
        Me.buttonGo.Size = New System.Drawing.Size(68, 85)
        Me.buttonGo.TabIndex = 3
        Me.buttonGo.Text = ":)"
        Me.buttonGo.UseVisualStyleBackColor = True
        '
        'txtIP
        '
        Me.txtIP.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtIP.Location = New System.Drawing.Point(63, 21)
        Me.txtIP.Name = "txtIP"
        Me.txtIP.Size = New System.Drawing.Size(461, 22)
        Me.txtIP.TabIndex = 4
        Me.txtIP.Text = "localhost"
        '
        'txtPort
        '
        Me.txtPort.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtPort.Location = New System.Drawing.Point(63, 49)
        Me.txtPort.Name = "txtPort"
        Me.txtPort.Size = New System.Drawing.Size(461, 22)
        Me.txtPort.TabIndex = 5
        Me.txtPort.Text = "7000"
        '
        'labelIP
        '
        Me.labelIP.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelIP.Location = New System.Drawing.Point(6, 21)
        Me.labelIP.Name = "labelIP"
        Me.labelIP.Size = New System.Drawing.Size(51, 24)
        Me.labelIP.TabIndex = 32
        Me.labelIP.Text = "IP"
        '
        'labelPort
        '
        Me.labelPort.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelPort.Location = New System.Drawing.Point(6, 51)
        Me.labelPort.Name = "labelPort"
        Me.labelPort.Size = New System.Drawing.Size(51, 24)
        Me.labelPort.TabIndex = 33
        Me.labelPort.Text = "Port"
        '
        'groupConnection
        '
        Me.groupConnection.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.groupConnection.Controls.Add(Me.txtIP)
        Me.groupConnection.Controls.Add(Me.labelPort)
        Me.groupConnection.Controls.Add(Me.txtPort)
        Me.groupConnection.Controls.Add(Me.labelIP)
        Me.groupConnection.Location = New System.Drawing.Point(86, 9)
        Me.groupConnection.Name = "groupConnection"
        Me.groupConnection.Size = New System.Drawing.Size(530, 85)
        Me.groupConnection.TabIndex = 34
        Me.groupConnection.TabStop = False
        Me.groupConnection.Text = "Connection Configuration"
        '
        'labelStatus
        '
        Me.labelStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.labelStatus.BackColor = System.Drawing.Color.DarkGray
        Me.labelStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.labelStatus.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelStatus.Location = New System.Drawing.Point(12, 97)
        Me.labelStatus.Name = "labelStatus"
        Me.labelStatus.Size = New System.Drawing.Size(604, 26)
        Me.labelStatus.TabIndex = 34
        '
        'comboDataType
        '
        Me.comboDataType.DisplayMember = "String"
        Me.comboDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.comboDataType.ItemHeight = 16
        Me.comboDataType.Items.AddRange(New Object() {"Byte", "WORD", "DWORD", "String"})
        Me.comboDataType.Location = New System.Drawing.Point(6, 14)
        Me.comboDataType.Name = "comboDataType"
        Me.comboDataType.Size = New System.Drawing.Size(86, 24)
        Me.comboDataType.TabIndex = 35
        '
        'txtInput
        '
        Me.txtInput.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtInput.Location = New System.Drawing.Point(98, 16)
        Me.txtInput.Name = "txtInput"
        Me.txtInput.Size = New System.Drawing.Size(500, 22)
        Me.txtInput.TabIndex = 36
        Me.txtInput.Text = "lainethlite"
        '
        'txtBuffer
        '
        Me.txtBuffer.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtBuffer.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtBuffer.Location = New System.Drawing.Point(6, 73)
        Me.txtBuffer.Multiline = True
        Me.txtBuffer.Name = "txtBuffer"
        Me.txtBuffer.ReadOnly = True
        Me.txtBuffer.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal
        Me.txtBuffer.Size = New System.Drawing.Size(536, 52)
        Me.txtBuffer.TabIndex = 37
        Me.txtBuffer.WordWrap = False
        '
        'buttonAdd
        '
        Me.buttonAdd.Location = New System.Drawing.Point(6, 44)
        Me.buttonAdd.Name = "buttonAdd"
        Me.buttonAdd.Size = New System.Drawing.Size(86, 23)
        Me.buttonAdd.TabIndex = 38
        Me.buttonAdd.Text = "Add"
        Me.buttonAdd.UseVisualStyleBackColor = True
        '
        'buttonDelete
        '
        Me.buttonDelete.Location = New System.Drawing.Point(98, 44)
        Me.buttonDelete.Name = "buttonDelete"
        Me.buttonDelete.Size = New System.Drawing.Size(86, 23)
        Me.buttonDelete.TabIndex = 39
        Me.buttonDelete.Text = "Delete"
        Me.buttonDelete.UseVisualStyleBackColor = True
        '
        'buttonClear
        '
        Me.buttonClear.Location = New System.Drawing.Point(190, 44)
        Me.buttonClear.Name = "buttonClear"
        Me.buttonClear.Size = New System.Drawing.Size(86, 23)
        Me.buttonClear.TabIndex = 40
        Me.buttonClear.Text = "Clear"
        Me.buttonClear.UseVisualStyleBackColor = True
        '
        'groupIO
        '
        Me.groupIO.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.groupIO.Controls.Add(Me.buttonAssignLength)
        Me.groupIO.Controls.Add(Me.checkASCII)
        Me.groupIO.Controls.Add(Me.checkHex)
        Me.groupIO.Controls.Add(Me.checkDec)
        Me.groupIO.Controls.Add(Me.buttonSend)
        Me.groupIO.Controls.Add(Me.buttonClear)
        Me.groupIO.Controls.Add(Me.buttonDelete)
        Me.groupIO.Controls.Add(Me.buttonAdd)
        Me.groupIO.Controls.Add(Me.txtBuffer)
        Me.groupIO.Controls.Add(Me.txtInput)
        Me.groupIO.Controls.Add(Me.comboDataType)
        Me.groupIO.Controls.Add(Me.txtChatLog)
        Me.groupIO.Location = New System.Drawing.Point(12, 126)
        Me.groupIO.Name = "groupIO"
        Me.groupIO.Size = New System.Drawing.Size(604, 358)
        Me.groupIO.TabIndex = 41
        Me.groupIO.TabStop = False
        '
        'checkASCII
        '
        Me.checkASCII.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.checkASCII.AutoSize = True
        Me.checkASCII.Checked = True
        Me.checkASCII.CheckState = System.Windows.Forms.CheckState.Checked
        Me.checkASCII.Location = New System.Drawing.Point(209, 331)
        Me.checkASCII.Name = "checkASCII"
        Me.checkASCII.Size = New System.Drawing.Size(63, 21)
        Me.checkASCII.TabIndex = 44
        Me.checkASCII.Text = "ASCII"
        Me.checkASCII.UseVisualStyleBackColor = True
        '
        'checkHex
        '
        Me.checkHex.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.checkHex.AutoSize = True
        Me.checkHex.Checked = True
        Me.checkHex.CheckState = System.Windows.Forms.CheckState.Checked
        Me.checkHex.Location = New System.Drawing.Point(93, 331)
        Me.checkHex.Name = "checkHex"
        Me.checkHex.Size = New System.Drawing.Size(110, 21)
        Me.checkHex.TabIndex = 43
        Me.checkHex.Text = "Hexadecimal"
        Me.checkHex.UseVisualStyleBackColor = True
        '
        'checkDec
        '
        Me.checkDec.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.checkDec.AutoSize = True
        Me.checkDec.Checked = True
        Me.checkDec.CheckState = System.Windows.Forms.CheckState.Checked
        Me.checkDec.Location = New System.Drawing.Point(7, 331)
        Me.checkDec.Name = "checkDec"
        Me.checkDec.Size = New System.Drawing.Size(80, 21)
        Me.checkDec.TabIndex = 42
        Me.checkDec.Text = "Decimal"
        Me.checkDec.UseVisualStyleBackColor = True
        '
        'buttonSend
        '
        Me.buttonSend.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.buttonSend.Location = New System.Drawing.Point(548, 73)
        Me.buttonSend.Name = "buttonSend"
        Me.buttonSend.Size = New System.Drawing.Size(50, 52)
        Me.buttonSend.TabIndex = 41
        Me.buttonSend.Text = "Send"
        Me.buttonSend.UseVisualStyleBackColor = True
        '
        'buttonAssignLength
        '
        Me.buttonAssignLength.Location = New System.Drawing.Point(282, 44)
        Me.buttonAssignLength.Name = "buttonAssignLength"
        Me.buttonAssignLength.Size = New System.Drawing.Size(168, 23)
        Me.buttonAssignLength.TabIndex = 45
        Me.buttonAssignLength.Text = "Assign Length"
        Me.buttonAssignLength.UseVisualStyleBackColor = True
        '
        'frmLENPClient
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(628, 496)
        Me.Controls.Add(Me.groupIO)
        Me.Controls.Add(Me.labelStatus)
        Me.Controls.Add(Me.groupConnection)
        Me.Controls.Add(Me.buttonGo)
        Me.Icon = CType(resources.GetObject("trayIcon.Icon"), System.Drawing.Icon)
        Me.Name = "frmLENPClient"
        Me.Text = "LENP Client Simulator"
        Me.groupConnection.ResumeLayout(False)
        Me.groupConnection.PerformLayout()
        Me.groupIO.ResumeLayout(False)
        Me.groupIO.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents txtChatLog As System.Windows.Forms.TextBox
    Friend WithEvents buttonGo As System.Windows.Forms.Button
    Friend WithEvents txtIP As System.Windows.Forms.TextBox
    Friend WithEvents txtPort As System.Windows.Forms.TextBox
    Friend WithEvents labelIP As System.Windows.Forms.Label
    Friend WithEvents labelPort As System.Windows.Forms.Label
    Friend WithEvents groupConnection As System.Windows.Forms.GroupBox
    Friend WithEvents labelStatus As System.Windows.Forms.Label
    Friend WithEvents comboDataType As System.Windows.Forms.ComboBox
    Friend WithEvents txtInput As System.Windows.Forms.TextBox
    Public WithEvents txtBuffer As System.Windows.Forms.TextBox
    Friend WithEvents buttonAdd As System.Windows.Forms.Button
    Friend WithEvents buttonDelete As System.Windows.Forms.Button
    Friend WithEvents buttonClear As System.Windows.Forms.Button
    Friend WithEvents groupIO As System.Windows.Forms.GroupBox
    Friend WithEvents buttonSend As System.Windows.Forms.Button
    Friend WithEvents checkASCII As System.Windows.Forms.CheckBox
    Friend WithEvents checkHex As System.Windows.Forms.CheckBox
    Friend WithEvents checkDec As System.Windows.Forms.CheckBox
    Friend WithEvents buttonAssignLength As System.Windows.Forms.Button
End Class
