Public Class ListManager
    'key: ipport, value = expiry
    Public ServerList As Dictionary(Of String, Dictionary(Of String, Integer))
    Private expire As Integer = (60 * 1000) * 10 ' 10 minute expire

    Private last_status As Integer = 0

    Public Sub New(ByVal ip_expire As Integer)
        expire = ip_expire * 1000
        ServerList = New Dictionary(Of String, Dictionary(Of String, Integer))
        ServerList.Add("jbnightfire", New Dictionary(Of String, Integer))
        ServerList.Add("sc2kne", New Dictionary(Of String, Integer))
        ServerList.Add("rmth2003", New Dictionary(Of String, Integer))
    End Sub

    Private Function binary_ipport(ByVal ipport As String) As Byte()
        Dim binary(5) As Byte
        Dim split = ipport.Split(":")

        Dim ip = split(0).Split(".")
        binary(0) = ip(0)
        binary(1) = ip(1)
        binary(2) = ip(2)
        binary(3) = ip(3)

        Dim port = split(1)
        Dim port1 As Integer = Math.Floor(port / 256) 'Conversion from string "6550 (" to type 'Double' is not valid.
        Dim port2 As Integer = port - (port1 * 256)
        binary(4) = port1
        binary(5) = port2

        Return binary
    End Function
    Public Sub Heartbeat(ByVal ipport As String, Optional ByVal gamename As String = "jbnightfire")
        If Not ServerList.ContainsKey(gamename) Then Return

        If ServerList(gamename).ContainsKey(ipport) Then
            ServerList(gamename)(ipport) = Environment.TickCount
        Else
            ServerList(gamename).Add(ipport, Environment.TickCount)
        End If
    End Sub
    Public Function GetListBinary(ByVal gamename As String) As Byte()
        Dim collect As New IO.MemoryStream(ServerList(gamename).Count * 6)
        For Each a In ServerList(gamename)
            Dim ipport = a.Key
            Dim binary = binary_ipport(ipport)
            collect.Write(binary, 0, 6)
        Next
        Dim final = Text.ASCIIEncoding.ASCII.GetBytes("\final\")
        collect.Write(final, 0, final.Length)
        Return collect.ToArray
    End Function
    Public Function GetListText(ByVal gamename As String) As String
        Dim collect As New IO.MemoryStream()
        For Each a In ServerList(gamename)
            Dim ipport = "\ip\" & a.Key
            Dim entry = Text.ASCIIEncoding.ASCII.GetBytes(ipport)
            collect.Write(entry, 0, entry.Length)
        Next
        Dim final = Text.ASCIIEncoding.ASCII.GetBytes("\final\")
        collect.Write(final, 0, final.Length)
        Dim text2 = Text.ASCIIEncoding.ASCII.GetString(collect.ToArray)
        Return text2
    End Function

    Public remove_queue As New Dictionary(Of String, List(Of String)) 'gamename, ipport
    Public Sub Tick()
        Dim time = Environment.TickCount

        BulkStatusRequest()
        Dim queuecount = 0
        For Each game In ServerList
            For Each server In ServerList(game.Key)
                Dim diff = time - server.Value
                If diff >= expire Then
                    Console.WriteLine("Removing server " & server.Key & " due to inactivity")
                    If Not remove_queue.ContainsKey(game.Key) Then remove_queue.Add(game.Key, New List(Of String))
                    remove_queue(game.Key).Add(server.Key)
                    queuecount += 1
                    'tcpclass.UpdateListData(game.Key)
                    'tcpclass.UpdateListText(game.Key)
                End If
            Next
        Next
        If queuecount > 0 Then
            For Each game In remove_queue
                For Each server In remove_queue(game.Key)
                    Dim ipport = server
                    ServerList(game.Key).Remove(ipport)
                Next
            Next
            remove_queue.Clear()
        End If
        UDPReceive()
    End Sub

    Private Function CheckIfBanned(ByRef ip As String) 'short ip contains 3 numbers
        Dim returnaddress As String = ip
        Dim returnaddressshort As String = ip
        Dim spl = returnaddressshort.Split(".")
        returnaddressshort = spl(0) & "." & spl(1) & "." & spl(2)

        If MasterListener.bannedips.Contains(returnaddress) Then
            Return True
        ElseIf MasterListener.bannedips.Contains(returnaddressshort) Then
            Return True
        End If
        Return False
    End Function

    Public Sub UDPReceive()
        While udp.Available > 0
            Dim ip = New Net.IPEndPoint(Net.IPAddress.Any, 0)
            Try
                Dim bytes As Byte() = udp.Receive(ip)
                If (bytes.Length > 0) Then
                    Dim data = Text.ASCIIEncoding.ASCII.GetString(bytes).Trim
                    Dim data2 = data.Split("\")

                    Select Case data2(1).ToLower
                        Case "heartbeat"
                            If KILL_NIGHTFIRE Then
                                Select Case ip.Address.ToString
                                    Case "192.168.100.160", "94.23.145.197", "94.23.119.13", "54.37.161.162"
                                        Console.WriteLine("Received server heartbeat from " & ip.Address.ToString & ":" & ip.Port)
                                        Heartbeat(ip.Address.ToString & ":" & ip.Port)
                                        Exit Select
                                    Case Else
                                        Console.WriteLine("BLOCKED server heartbeat from " & ip.Address.ToString & ":" & ip.Port)
                                        Exit Select
                                End Select
                            Else
                                If CheckIfBanned(ip.Address.ToString) Then
                                    Console.WriteLine("Received server heartbeat from BANNED IP " & ip.Address.ToString & ":" & ip.Port)
                                Else
                                    Console.WriteLine("Received server heartbeat from " & ip.Address.ToString & ":" & ip.Port)
                                    'Heartbeat(ip.Address.ToString & ":" & ip.Port)
                                    SendUDPPacket(ip.Address.ToString, ip.Port, Text.ASCIIEncoding.ASCII.GetBytes("\status\"), udp) 'Query the server to get game information
                                    'If Not ipports_waiting_to_be_added_to_serverlist.Contains(ip.Address.ToString & ":" & ip.Port) Then
                                    '    ipports_waiting_to_be_added_to_serverlist.Add(ip.Address.ToString & ":" & ip.Port)
                                    'End If
                                End If
                                Exit Select
                            End If
                        Case "gamename"
                            'Console.WriteLine("Received server status from " & ip.Address.ToString & ":" & ip.Port)
                            'todo: fix exception here
                            Try
                                Heartbeat(ip.Address.ToString & ":" & ip.Port, data2(2))
                                'tcpclass.UpdateListData(data2(2))
                                'tcpclass.UpdateListText(data2(2))
                            Catch ex As Exception
                                Console.WriteLine("Exception caught at UDPReceive() (gamename case): " & ex.Message)
                            End Try
                            Exit Select
                        Case "manualheartbeat"
                            Console.WriteLine("Received manual server heartbeat from " & ip.Address.ToString & ":" & ip.Port)
                            Dim hostipqueryport = data2(2)
                            Heartbeat(hostipqueryport)
                            Exit Select
                        Case Else
                            Console.WriteLine("Unknown packet data from " & ip.Address.ToString & ":" & ip.Port & " -> " & data)
                            Exit Select
                    End Select
                Else
                    Console.WriteLine("No bytes received at UDPReceive() from " & ip.Address.ToString & ":" & ip.Port)
                End If
            Catch ex As Exception
                Console.WriteLine("Exception caught at UDPReceive() (generic): " & ex.Message)
            End Try
        End While
    End Sub

    'Private ipports_waiting_to_be_added_to_serverlist As New List(Of String)

    Public Sub BulkStatusRequest()
        Dim current_time = Environment.TickCount
        If (current_time - last_status) > 30000 Then
            Console.WriteLine("Pinging for servers that no longer exist")
            For Each game In ServerList
                For Each server In ServerList(game.Key)
                    Dim ipport = server.Key.Split(":")
                    'Console.WriteLine("Checking if " & ipport(0) & ":" & ipport(1) & " is alive")
                    SendUDPPacket(ipport(0), ipport(1), Text.ASCIIEncoding.ASCII.GetBytes("\status\"), udp)
                Next
            Next
            last_status = current_time
        End If

    End Sub
End Class
