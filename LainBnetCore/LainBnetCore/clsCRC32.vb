Option Strict On
Option Explicit On 

Imports System.Security.Cryptography


Public Class clsCRC32
    Inherits HashAlgorithm

    Private Const _DefaultPolynomial As Integer = &HEDB88320
    Private _Table() As Integer
    Private _CRC32 As Integer = &HFFFFFFFF
    Private _Polynomial As Integer

    Public Sub New()
        Me.HashSizeValue = 32 ' CRC32 is a 32bit hash
        _Polynomial = _DefaultPolynomial
        Initialize()
    End Sub
    Public Sub New(ByVal Polynomial As Integer)
        _Polynomial = Polynomial
    End Sub

    Protected Overrides Sub HashCore(ByVal array() As Byte, ByVal ibStart As Integer, ByVal cbSize As Integer)

        Dim intLookup As Integer
        For i As Integer = 0 To cbSize - 1
            intLookup = (_CRC32 And &HFF) Xor array(i)
            'This is a workaround for a right bit-shift because vb.net
            'does not support unsigned Integers, so _CRC32 >> 8
            'gives the wrong value (any better fixes?)
            _CRC32 = ((_CRC32 And &HFFFFFF00) \ &H100) And &HFFFFFF
            _CRC32 = _CRC32 Xor _Table(intLookup)
        Next i
    End Sub
    Protected Overrides Function HashFinal() As Byte()
        Return BitConverter.GetBytes(Not _CRC32)
    End Function
    Public Overrides Sub Initialize()
        _CRC32 = &HFFFFFFFF
        _Table = BuildTable(_Polynomial)
    End Sub
    Private Function BuildTable(ByVal Polynomial As Integer) As Integer()
        Dim Table(255) As Integer

        Dim Value As Integer

        For I As Integer = 0 To 255
            Value = I
            For X As Integer = 0 To 7
                If (Value And 1) = 1 Then
                    'This is a workaround for a right bit-shift because vb.net
                    'does not support unsigned Integers, so _CRC32 >> 1
                    'gives the wrong value (any better fixes?)
                    Value = Convert.ToInt32(((Value And &HFFFFFFFE) \ 2&) And &H7FFFFFFF)
                    Value = Value Xor Polynomial
                Else
                    'Same as above.
                    Value = Convert.ToInt32(((Value And &HFFFFFFFE) \ 2&) And &H7FFFFFFF)
                End If
            Next
            Table(I) = Value
        Next

        Return Table
    End Function


End Class
