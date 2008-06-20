<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmLENPServer
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmLENPServer))
        Me.txtServerPort = New System.Windows.Forms.TextBox
        Me.labelServerPort = New System.Windows.Forms.Label
        Me.labelStatus = New System.Windows.Forms.Label
        Me.groupServerConfig = New System.Windows.Forms.GroupBox
        Me.comboServerIP = New System.Windows.Forms.ComboBox
        Me.labelServerIP = New System.Windows.Forms.Label
        Me.buttonGo = New System.Windows.Forms.Button
        Me.groupServerConfig.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtServerPort
        '
        Me.txtServerPort.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtServerPort.Location = New System.Drawing.Point(110, 48)
        Me.txtServerPort.Name = "txtServerPort"
        Me.txtServerPort.Size = New System.Drawing.Size(209, 22)
        Me.txtServerPort.TabIndex = 35
        Me.txtServerPort.Text = "7000"
        '
        'labelServerPort
        '
        Me.labelServerPort.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelServerPort.Location = New System.Drawing.Point(6, 48)
        Me.labelServerPort.Name = "labelServerPort"
        Me.labelServerPort.Size = New System.Drawing.Size(98, 24)
        Me.labelServerPort.TabIndex = 36
        Me.labelServerPort.Text = "Server Port"
        '
        'labelStatus
        '
        Me.labelStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.labelStatus.BackColor = System.Drawing.Color.DarkGray
        Me.labelStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.labelStatus.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelStatus.Location = New System.Drawing.Point(6, 75)
        Me.labelStatus.Name = "labelStatus"
        Me.labelStatus.Size = New System.Drawing.Size(313, 21)
        Me.labelStatus.TabIndex = 38
        '
        'groupServerConfig
        '
        Me.groupServerConfig.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.groupServerConfig.Controls.Add(Me.comboServerIP)
        Me.groupServerConfig.Controls.Add(Me.labelServerIP)
        Me.groupServerConfig.Controls.Add(Me.labelStatus)
        Me.groupServerConfig.Controls.Add(Me.labelServerPort)
        Me.groupServerConfig.Controls.Add(Me.txtServerPort)
        Me.groupServerConfig.Location = New System.Drawing.Point(74, 12)
        Me.groupServerConfig.Name = "groupServerConfig"
        Me.groupServerConfig.Size = New System.Drawing.Size(325, 104)
        Me.groupServerConfig.TabIndex = 39
        Me.groupServerConfig.TabStop = False
        Me.groupServerConfig.Text = "Server Configuration"
        '
        'comboServerIP
        '
        Me.comboServerIP.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.comboServerIP.FormattingEnabled = True
        Me.comboServerIP.Items.AddRange(New Object() {"localhost"})
        Me.comboServerIP.Location = New System.Drawing.Point(110, 21)
        Me.comboServerIP.Name = "comboServerIP"
        Me.comboServerIP.Size = New System.Drawing.Size(209, 24)
        Me.comboServerIP.TabIndex = 40
        Me.comboServerIP.Text = "localhost"
        '
        'labelServerIP
        '
        Me.labelServerIP.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.labelServerIP.Location = New System.Drawing.Point(6, 24)
        Me.labelServerIP.Name = "labelServerIP"
        Me.labelServerIP.Size = New System.Drawing.Size(98, 24)
        Me.labelServerIP.TabIndex = 39
        Me.labelServerIP.Text = "Server IP"
        '
        'buttonGo
        '
        Me.buttonGo.Font = New System.Drawing.Font("Courier New", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.buttonGo.Location = New System.Drawing.Point(12, 12)
        Me.buttonGo.Name = "buttonGo"
        Me.buttonGo.Size = New System.Drawing.Size(56, 104)
        Me.buttonGo.TabIndex = 40
        Me.buttonGo.Text = ":)"
        Me.buttonGo.UseVisualStyleBackColor = True
        '
        'frmLENPServer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(411, 123)
        Me.Controls.Add(Me.buttonGo)
        Me.Controls.Add(Me.groupServerConfig)
        Me.Icon = CType(resources.GetObject("trayIcon.Icon"), System.Drawing.Icon)
        Me.Name = "frmLENPServer"
        Me.Text = "LENP Server"
        Me.groupServerConfig.ResumeLayout(False)
        Me.groupServerConfig.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents txtServerPort As System.Windows.Forms.TextBox
    Friend WithEvents labelServerPort As System.Windows.Forms.Label
    Friend WithEvents labelStatus As System.Windows.Forms.Label
    Friend WithEvents groupServerConfig As System.Windows.Forms.GroupBox
    Friend WithEvents comboServerIP As System.Windows.Forms.ComboBox
    Friend WithEvents labelServerIP As System.Windows.Forms.Label
    Friend WithEvents buttonGo As System.Windows.Forms.Button
End Class
