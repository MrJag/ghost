Option Explicit On 
Option Strict On



Public Class clsBNETChannel
    Private currentChannel As String
    Private currentChannelPeople As ArrayList
    Public Event EventChannelChange()
    Public Event EventChannelChatMessage(ByVal msg As String)
    Public Event EventChannelUserJoin(ByVal user As String)
    Public Event EventChannelUserLeave(ByVal user As String)
    Public Event EventChannelUserCurrent(ByVal user As String)

    Public Sub New()
        currentChannel = ""
        currentChannelPeople = New ArrayList
    End Sub

    Public Function GetChannelName() As String
        Return currentChannel
    End Function
    Public Function GetChannelPeopleList() As String()
        Try
            Return CType(currentChannelPeople.ToArray(GetType(String)), String())
        Catch ex As Exception
            Return New String() {}
        End Try
    End Function


    Public Function Process(ByVal data As clsIncomingChatChannel) As Boolean
        Try
            Select Case data.GetChatEvent
                Case clsProtocolBNET.IncomingChatEvent.EID_SHOWUSER
                    If currentChannelPeople.Contains(data.GetUser) = False Then
                        currentChannelPeople.Add(data.GetUser)
                        RaiseEvent EventChannelChange()
                        RaiseEvent EventChannelUserCurrent(data.GetUser)
                    End If
                Case clsProtocolBNET.IncomingChatEvent.EID_JOIN
                    If currentChannelPeople.Contains(data.GetUser) = False Then
                        currentChannelPeople.Add(data.GetUser)
                        RaiseEvent EventChannelChange()
                        RaiseEvent EventChannelUserJoin(data.GetUser)
                    End If
                Case clsProtocolBNET.IncomingChatEvent.EID_LEAVE
                    If currentChannelPeople.Contains(data.GetUser) = True Then
                        currentChannelPeople.Remove(data.GetUser)
                        RaiseEvent EventChannelChange()
                        RaiseEvent EventChannelUserLeave(data.GetUser)
                    End If
                Case clsProtocolBNET.IncomingChatEvent.EID_WHISPER
                    RaiseEvent EventChannelChatMessage(String.Format("[whisper]{0}: {1}", data.GetUser, data.GetMessage))
                Case clsProtocolBNET.IncomingChatEvent.EID_TALK
                    RaiseEvent EventChannelChatMessage(String.Format("{0}: {1}", data.GetUser, data.GetMessage))
                Case clsProtocolBNET.IncomingChatEvent.EID_BROADCAST
                    RaiseEvent EventChannelChatMessage(String.Format("[Battle.Net]: {0}", data.GetMessage))
                Case clsProtocolBNET.IncomingChatEvent.EID_WHISPERSENT
                    RaiseEvent EventChannelChatMessage(String.Format("[local whisper]: {0}", data.GetMessage))
                Case clsProtocolBNET.IncomingChatEvent.EID_CHANNEL
                    currentChannel = data.GetMessage
                    currentChannelPeople.Clear()
                    RaiseEvent EventChannelChatMessage(String.Format("[Battle.Net]: Channel - {0}", data.GetMessage))
                    RaiseEvent EventChannelChange()
                Case clsProtocolBNET.IncomingChatEvent.EID_CHANNELFULL
                    currentChannel = ""
                    currentChannelPeople.Clear()
                    RaiseEvent EventChannelChatMessage(String.Format("[Battle.Net]: Channel - {0} is full", data.GetMessage))
                    RaiseEvent EventChannelChange()
                Case clsProtocolBNET.IncomingChatEvent.EID_CHANNELDOESNOTEXIST
                    currentChannel = ""
                    currentChannelPeople.Clear()
                    RaiseEvent EventChannelChatMessage(String.Format("[Battle.Net]: Channel - {0} does not exsist", data.GetMessage))
                    RaiseEvent EventChannelChange()
                Case clsProtocolBNET.IncomingChatEvent.EID_CHANNELRESTRICTED
                    currentChannel = ""
                    currentChannelPeople.Clear()
                    RaiseEvent EventChannelChatMessage(String.Format("[Battle.Net]: Channel - {0} is restricted", data.GetMessage))
                    RaiseEvent EventChannelChange()
                Case clsProtocolBNET.IncomingChatEvent.EID_INFO
                    RaiseEvent EventChannelChatMessage(String.Format("[Battle.Net]: Info - {0}", data.GetMessage))
                Case clsProtocolBNET.IncomingChatEvent.EID_USERFLAGS
                Case clsProtocolBNET.IncomingChatEvent.EID_ERROR
                    RaiseEvent EventChannelChatMessage(String.Format("[Battle.Net]: Error - {0}", data.GetMessage))
                Case clsProtocolBNET.IncomingChatEvent.EID_EMOTE
                    RaiseEvent EventChannelChatMessage(String.Format("[emote]{0}: {1}", data.GetUser, data.GetMessage))
                Case clsProtocolBNET.IncomingChatEvent.LOCAL_LOOP_BACK
                    RaiseEvent EventChannelChatMessage(String.Format("[local]: {0}", data.GetMessage))
            End Select

            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function





End Class
































'
