Option Explicit On 
Option Strict On

Public Class frmConvertor
    Inherits System.Windows.Forms.Form

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
    Friend WithEvents txtDec As System.Windows.Forms.TextBox
    Friend WithEvents txtHex As System.Windows.Forms.TextBox
    Friend WithEvents buttonDec As System.Windows.Forms.Button
    Friend WithEvents splitConvertor As System.Windows.Forms.SplitContainer
    Friend WithEvents buttonHex As System.Windows.Forms.Button
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmConvertor))
        Me.txtDec = New System.Windows.Forms.TextBox
        Me.txtHex = New System.Windows.Forms.TextBox
        Me.buttonDec = New System.Windows.Forms.Button
        Me.buttonHex = New System.Windows.Forms.Button
        Me.splitConvertor = New System.Windows.Forms.SplitContainer
        Me.splitConvertor.Panel1.SuspendLayout()
        Me.splitConvertor.Panel2.SuspendLayout()
        Me.splitConvertor.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtDec
        '
        Me.txtDec.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtDec.Location = New System.Drawing.Point(0, 0)
        Me.txtDec.Multiline = True
        Me.txtDec.Name = "txtDec"
        Me.txtDec.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtDec.Size = New System.Drawing.Size(584, 213)
        Me.txtDec.TabIndex = 0
        Me.txtDec.Text = resources.GetString("txtDec.Text")
        '
        'txtHex
        '
        Me.txtHex.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtHex.Location = New System.Drawing.Point(0, 0)
        Me.txtHex.Multiline = True
        Me.txtHex.Name = "txtHex"
        Me.txtHex.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtHex.Size = New System.Drawing.Size(584, 164)
        Me.txtHex.TabIndex = 1
        Me.txtHex.Text = resources.GetString("txtHex.Text")
        '
        'buttonDec
        '
        Me.buttonDec.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.buttonDec.Location = New System.Drawing.Point(602, 12)
        Me.buttonDec.Name = "buttonDec"
        Me.buttonDec.Size = New System.Drawing.Size(56, 64)
        Me.buttonDec.TabIndex = 2
        Me.buttonDec.Text = "HEX To DEC"
        '
        'buttonHex
        '
        Me.buttonHex.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.buttonHex.Location = New System.Drawing.Point(602, 82)
        Me.buttonHex.Name = "buttonHex"
        Me.buttonHex.Size = New System.Drawing.Size(56, 64)
        Me.buttonHex.TabIndex = 3
        Me.buttonHex.Text = "DEC To HEX"
        '
        'splitConvertor
        '
        Me.splitConvertor.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.splitConvertor.Location = New System.Drawing.Point(12, 12)
        Me.splitConvertor.Name = "splitConvertor"
        Me.splitConvertor.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'splitConvertor.Panel1
        '
        Me.splitConvertor.Panel1.Controls.Add(Me.txtHex)
        '
        'splitConvertor.Panel2
        '
        Me.splitConvertor.Panel2.Controls.Add(Me.txtDec)
        Me.splitConvertor.Size = New System.Drawing.Size(584, 381)
        Me.splitConvertor.SplitterDistance = 164
        Me.splitConvertor.TabIndex = 4
        '
        'frmConvertor
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(6, 15)
        Me.ClientSize = New System.Drawing.Size(666, 405)
        Me.Controls.Add(Me.splitConvertor)
        Me.Controls.Add(Me.buttonHex)
        Me.Controls.Add(Me.buttonDec)
        Me.Icon = CType(resources.GetObject("trayIcon.Icon"), System.Drawing.Icon)
        Me.Name = "frmConvertor"
        Me.Text = "Byte Numeric Convertor"
        Me.splitConvertor.Panel1.ResumeLayout(False)
        Me.splitConvertor.Panel1.PerformLayout()
        Me.splitConvertor.Panel2.ResumeLayout(False)
        Me.splitConvertor.Panel2.PerformLayout()
        Me.splitConvertor.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub buttonDec_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonDec.Click
        Dim codes As String()
        Dim builder As System.Text.StringBuilder

        builder = New System.Text.StringBuilder
        txtHex.Text = txtHex.Text.Replace(Environment.NewLine, " ")
        txtHex.Text = txtHex.Text.Replace("[", " ")
        txtHex.Text = txtHex.Text.Replace("]", " ")
        txtHex.Text = txtHex.Text.Replace(",", " ")
        codes = txtHex.Text.Split(CChar(" "))
        For Each code As String In codes
            code = Trim(code)
            If code.StartsWith("0x") Then
                code = Trim(code.Substring(2))
            End If
            If code.Length > 0 Then
                If New System.Text.RegularExpressions.Regex("^[A-Fa-f0-9]*$").Match(code).Success AndAlso code.Length > 0 AndAlso code.Length <= 2 Then
                    builder.Append(Byte.Parse(code, System.Globalization.NumberStyles.HexNumber))
                    builder.Append(" ")
                Else
                    builder.Append(String.Format("[ERROR:{0}]", code))
                    builder.Append(" ")
                End If
            End If
        Next
        txtDec.Text = builder.ToString
    End Sub

    Private Function ToHex(ByVal num As Byte) As String

        Select Case num
            Case Is <= 9 : Return CStr(num)
            Case Is = 10 : Return "A"
            Case Is = 11 : Return "B"
            Case Is = 12 : Return "C"
            Case Is = 13 : Return "D"
            Case Is = 14 : Return "E"
            Case Is = 15 : Return "F"
            Case Else : Return CStr(String.Format("[ERROR:{0}]", num))
        End Select
    End Function

    Private Sub buttonHex_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles buttonHex.Click
        Dim codes As String()
        Dim builder As System.Text.StringBuilder
        Dim num As Byte
        Dim high As Byte
        Dim low As Byte

        builder = New System.Text.StringBuilder
        txtDec.Text = txtDec.Text.Replace(Environment.NewLine, " ")
        txtDec.Text = txtDec.Text.Replace("[", " ")
        txtDec.Text = txtDec.Text.Replace("]", " ")
        txtDec.Text = txtDec.Text.Replace(",", " ")
        codes = txtDec.Text.Split(CChar(" "))
        For Each code As String In codes
            code = Trim(code)
            If code.Length > 0 Then
                If New System.Text.RegularExpressions.Regex("^[0-9]*$").Match(code).Success AndAlso code.Length <= 3 AndAlso CInt(code) <= 255 Then
                    num = CByte(code)
                    high = CByte(num >> 4)
                    low = CByte(num And 15)
                    builder.Append(String.Format("{0}{1}", ToHex(high), ToHex(low)))
                    builder.Append(" ")
                Else
                    builder.Append(String.Format("[ERROR:{0}]", code))
                    builder.Append(" ")
                End If
            End If
        Next
        txtHex.Text = builder.ToString
    End Sub
End Class





































'
