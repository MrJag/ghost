Option Explicit On 
Option Strict On

Imports System.Windows.Forms
Imports System.drawing

Public Class clsHelper

    Public Enum PrintType
        DEC
        HEX
        ASCII
    End Enum

    Public Delegate Sub DelegateControlBackColor(ByVal control As Control, ByVal color As Color)
    Public Delegate Sub DelegateControlText(ByVal control As Control, ByVal msg As String)
    Public Delegate Sub DelegateTextBoxAppend(ByVal control As TextBox, ByVal msg As String)
    Public Delegate Sub DelegateControlEnabled(ByVal control As Control, ByVal flag As Boolean)
    Public Delegate Sub DelegateControlTextBoxReadOnly(ByVal control As TextBox, ByVal flag As Boolean)
    Public Delegate Sub DelegateControlListBoxDataSource(ByVal control As ListBox, ByVal list As Object)
    Public Delegate Function DelegateControlTextGet(ByVal control As Control) As String



#Region "GUI Invoke Delegate fucntions"
    Public Shared Function ControlTextGet(ByVal control As Control) As String
        Return control.Text
    End Function
    Public Shared Sub ControlListBoxDataSource(ByVal control As ListBox, ByVal list As Object)
        control.BeginUpdate()
        control.DataSource = list
        control.EndUpdate()
    End Sub
    Public Shared Sub ControlEnabled(ByVal control As Control, ByVal flag As Boolean)
        control.Enabled = flag
    End Sub
    Public Shared Sub ControlTextBoxReadOnly(ByVal control As TextBox, ByVal flag As Boolean)
        control.ReadOnly = flag
    End Sub
    Public Shared Sub ControlText(ByVal control As Control, ByVal msg As String)
        control.Text = msg
    End Sub
    Public Shared Sub ControlBackColor(ByVal control As Control, ByVal color As Color)
        control.BackColor = color
    End Sub
    Public Shared Sub ControlTextBoxAppend(ByVal control As TextBox, ByVal msg As String)
        control.AppendText(msg)
    End Sub
#End Region


 
    Private Shared Function ToHex(ByVal num As Byte) As String
        Try
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
        Catch ex As Exception
            Return "[ERROR]"
        End Try

    End Function

    Public Shared Function PrintArray(ByVal data As Byte()) As String
        Return PrintArray(data, PrintType.DEC)
    End Function
    Public Shared Function PrintArray(ByVal data As Byte(), ByVal printtype As PrintType) As String
        Dim b As Byte
        Dim result As System.Text.StringBuilder

        Try
            result = New System.Text.StringBuilder
            Select Case printtype
                Case printtype.ASCII
                    For Each b In data
                        If b >= 32 AndAlso b <= 126 Then
                            result.Append(Convert.ToChar(b))
                        Else
                            result.Append(" ")
                        End If
                    Next
                Case printtype.DEC
                    For Each b In data
                        result.Append(String.Format("{0} ", CStr(b)))
                    Next
                Case printtype.HEX
                    For Each b In data
                        result.Append(String.Format("{0}{1} ", ToHex(CByte(b >> 4)), ToHex(CByte(b And 15))))
                    Next
            End Select

            Return (result.ToString())
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Shared Function ByteArrayToStringASCII(ByVal arrayInput As Byte()) As String
        Dim b As Byte
        Dim result As System.Text.StringBuilder

        Try
            result = New System.Text.StringBuilder
            For Each b In arrayInput
                If b >= 32 AndAlso b <= 126 Then
                    result.Append(Convert.ToChar(b))
                ElseIf b = 0 Then
                    result.Append("")
                Else
                    'not ascii char set
                End If
            Next
            Return (result.ToString())
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Public Shared Function ByteArrayToLong(ByVal arrayInput As Byte(), ByVal reverse As Boolean) As Long
        Dim index As Integer
        Dim result As Long
        Dim data As Byte()
        Try
            data = CType(arrayInput.Clone, Byte())
            If reverse = True Then
                Array.Reverse(data)
            End If
            For index = 0 To data.Length - 1
                result = result + data(index) * CType(Math.Pow(256, (data.Length - index - 1)), Long)
            Next
            Return result
        Catch ex As Exception
            Return 0
        End Try
    End Function
    Public Shared Function ByteArrayToLong(ByVal arrayInput As Byte()) As Long
        Return ByteArrayToLong(arrayInput, True)
    End Function

    Public Shared Function IntegerToWORD(ByVal number As Integer, ByVal isBigEndian As Boolean) As Byte()
        Dim buffer As Byte()
        Dim converted As Byte()
        Try
            converted = BitConverter.GetBytes(number)   'small endian
            buffer = New Byte(2 - 1) {}
            Array.Copy(converted, 0, buffer, 0, 2)

            If isBigEndian Then
                Array.Reverse(buffer)       'big endian
            End If
            Return buffer
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function
    Public Shared Function LongToDWORD(ByVal number As Long, ByVal isBigEndian As Boolean) As Byte()
        Dim buffer As Byte()
        Dim converted As Byte()
        Try
            converted = BitConverter.GetBytes(number)   'small endian
            buffer = New Byte(4 - 1) {}
            Array.Copy(converted, 0, buffer, 0, 4)

            If isBigEndian Then
                Array.Reverse(buffer)       'big endian
            End If
            Return buffer
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function

    Public Shared Function AddByteArray(ByVal list As ArrayList, ByVal content As Byte()) As Boolean
        Try
            If list Is Nothing = False AndAlso content Is Nothing = False Then
                For Each b As Byte In content
                    list.Add(b)
                Next
                Return True
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try

    End Function
    Public Shared Function AddByteArray(ByVal list As ArrayList, ByVal content As String) As Boolean
        Dim buffer As Byte()
        Dim complete As Byte()
        Try
            buffer = New System.Text.ASCIIEncoding().GetBytes(content)
            complete = New Byte(buffer.Length - 1 + 1) {}
            complete(complete.Length - 1) = 0   'set last byte to NULL terminator
            Array.Copy(buffer, complete, buffer.Length)
            Return AddByteArray(list, complete)
        Catch ex As Exception
            Return False
        End Try

    End Function


End Class