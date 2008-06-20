Option Explicit On 
Option Strict On

Imports MBNCSUtil
Imports System.IO
Imports LainHelper

Public Class clsMBNCSUtiliInterface

    Private exeversion As Byte()
    Private exeversionhash As Byte()
    Private exeinfo As String

    Private infoROC As Byte()
    Private infoTFT As Byte()

    Private nls As NLS
    Private clientKey As Byte()
    Private serverKey As Byte()
    Private salt As Byte()
    Private m1 As Byte()
    Private pvpgnPasswordHash As Byte()


    Public Sub New(ByVal username As String, ByVal password As String)
        Try
            exeversion = New Byte() {}
            exeversionhash = New Byte() {}
            exeinfo = ""
            infoROC = New Byte() {}
            infoTFT = New Byte() {}

            nls = New NLS(username, password)
            clientKey = New Byte() {}
            serverKey = New Byte() {}
            salt = New Byte() {}
            m1 = New Byte() {}
            pvpgnPasswordHash = New Byte() {}
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub

    Public Function GetPvPGNPasswordHash() As Byte()
        Return pvpgnPasswordHash
    End Function
    Public Function GetM1() As Byte()
        Return m1
    End Function
    Public Function GetSalt() As Byte()
        Return salt
    End Function
    Public Function GetServerKEy() As Byte()
        Return serverKey
    End Function
    Public Function GetClientKey() As Byte()
        Return clientKey
    End Function
    Public Function GetExeVersion() As Byte()
        Return exeversion
    End Function
    Public Function GetExeVersionHash() As Byte()
        Return exeversionhash
    End Function
    Public Function GetExeInfo() As String
        Return exeinfo
    End Function
    Public Function GetKeyInfoROC() As Byte()
        Return infoROC
    End Function
    Public Function GetKeyInfoTFT() As Byte()
        Return infoTFT
    End Function
    Public Function SetExeVersion(ByVal exeversionArray As Byte()) As Boolean
        exeversion = exeversionArray
    End Function
    Public Function SetExeVersionHash(ByVal exeversionhashArray As Byte()) As Boolean
        exeversionhash = exeversionhashArray
    End Function

    Private Function CreateKeyInfo(ByVal key As String, ByVal clientToken As Long, ByVal serverToken As Long) As Byte()
        Dim info As ArrayList
        Dim decoder As CdKey
        Try
            info = New ArrayList
            decoder = CdKey.CreateDecoder(key)

            If decoder.IsValid Then
                clsHelper.AddByteArray(info, clsHelper.LongToDWORD(decoder.Key.Length, False))
                clsHelper.AddByteArray(info, clsHelper.LongToDWORD(decoder.Product, False))
                clsHelper.AddByteArray(info, clsHelper.LongToDWORD(decoder.Value1, False))
                clsHelper.AddByteArray(info, New Byte() {0, 0, 0, 0})
                clsHelper.AddByteArray(info, decoder.GetHash(Convert.ToUInt32(clientToken), Convert.ToUInt32(serverToken)))
                Return CType(info.ToArray(GetType(Byte)), Byte())
            End If

            Return New Byte() {}
        Catch ex As Exception
            Return New Byte() {}
        End Try
    End Function

    Public Function HELP_SID_AUTH_CHECK(ByVal war3path As String, ByVal keyROC As String, ByVal keyTFT As String, ByVal valueStringForumla As String, ByVal MPQFileName As String, ByVal clientToken As Byte(), ByVal serverToken As Byte()) As Boolean
        Dim file_war3_exe As String = "war3.exe"
        Dim file_storm_dll As String = "storm.dll"
        Dim file_game_dll As String = "game.dll"
        Dim war3files As String()


        Try
            If war3path.EndsWith("\") = False Then
                war3path = war3path & "\"
            End If
            If IO.File.Exists(war3path & file_war3_exe) AndAlso IO.File.Exists(war3path & file_storm_dll) AndAlso IO.File.Exists(war3path & file_game_dll) Then
                war3files = New String() {war3path & file_war3_exe, war3path & file_storm_dll, war3path & file_game_dll}
                exeversion = clsHelper.LongToDWORD(MBNCSUtil.CheckRevision.GetExeInfo(war3files(0), exeinfo), False)
                exeversionhash = clsHelper.LongToDWORD(MBNCSUtil.CheckRevision.DoCheckRevision(valueStringForumla, war3files, MBNCSUtil.CheckRevision.ExtractMPQNumber(MPQFileName)), False)

                infoROC = CreateKeyInfo(keyROC, clsHelper.ByteArrayToLong(clientToken), clsHelper.ByteArrayToLong(serverToken))
                infoTFT = CreateKeyInfo(keyTFT, clsHelper.ByteArrayToLong(clientToken), clsHelper.ByteArrayToLong(serverToken))
                If infoROC.Length = 36 AndAlso infoTFT.Length = 36 Then
                    Return True
                End If
            End If
            Return False
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function HELP_SID_AUTH_ACCOUNTLOGON() As Boolean
        Dim packet As BncsPacket

        Try
            packet = New BncsPacket(clsProtocolBNET.Protocol.SID_AUTH_ACCOUNTLOGON)
            nls.LoginAccount(packet)

            clientKey = New Byte(32 - 1) {}
            Array.Copy(packet.GetData(), 4, clientKey, 0, clientKey.Length)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function HELP_SID_AUTH_ACCOUNTLOGONPROOF(ByVal salt As Byte(), ByVal serverKey As Byte()) As Boolean
        Dim packet As BncsPacket

        Try
            Me.salt = CType(salt.Clone, Byte())
            Me.serverKey = CType(serverKey.Clone, Byte())

            packet = New BncsPacket(clsProtocolBNET.Protocol.SID_AUTH_ACCOUNTLOGONPROOF)
            nls.LoginProof(packet, salt, serverKey)

            m1 = New Byte(20 - 1) {}
            Array.Copy(packet.GetData(), 4, m1, 0, m1.Length)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function HELP_PVPGNPasswordHash(ByVal password As String) As Boolean
        Try
            pvpgnPasswordHash = OldAuth.HashPassword(password)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

End Class









'