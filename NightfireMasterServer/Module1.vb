Imports System.Threading.Thread
Imports System.Text.Encoding
Imports System.Net.Sockets
Module Module1
    Public KILL_NIGHTFIRE As Boolean = False
    Public udp As Net.Sockets.UdpClient
    Public tcpclass As MasterListener
    Public listmgr As ListManager
    Public endpoint As Net.IPEndPoint
    Public masterudpport As Integer = 27900
    Public mastertcpport As Integer = 28900
    Public listenip As String = ""
    Sub Main()
        Console.Title = "Gamespy Master Server Emulator (enctype2)"
        If Not GetCommandLineArguments() Then
            listenip = "127.0.0.1"
            Console.WriteLine("WARNING: -ip argument not specified, using 127.0.0.1")
            Sleep(1000)
        End If

        tcpclass = New MasterListener(listenip, mastertcpport)
        InitializeUDP()
        listmgr = New ListManager(1200)

        'hard coded list of servers to populate the list
        If Not KILL_NIGHTFIRE Then
            'serverlist.Heartbeat("142.196.204.105:6550")
            'serverlist.Heartbeat("69.175.117.178:6551")
            'serverlist.Heartbeat("69.175.117.178:6552")
            'serverlist.Heartbeat("69.175.117.178:6550")
            'serverlist.Heartbeat("173.234.63.50:26015")
            'serverlist.Heartbeat("91.121.119.13:6551")
            'serverlist.Heartbeat("91.121.119.13:6550")
            'serverlist.Heartbeat("91.121.119.13:6552")
            'serverlist.Heartbeat("91.121.119.13:6553")
            'serverlist.Heartbeat("91.121.119.13:6554")
            'serverlist.Heartbeat("91.121.119.13:6590")
            'serverlist.Heartbeat("2.89.46.19:6550")
            'serverlist.Heartbeat("193.111.139.210:6550")
            'serverlist.Heartbeat("72.46.158.122:6550")
            'serverlist.Heartbeat("72.46.158.122:6551")
            'serverlist.Heartbeat("72.46.158.122:6552")
            'serverlist.Heartbeat("82.18.14.151:6550")
            'MasterListener.bannedips.Add("188.138.95.24") 'LAG FREE ROMANIA
            'MasterListener.bannedips.Add("68.62.135.144") 'Ray Ray
            'MasterListener.bannedips.Add("84.58.19")
            'MasterListener.bannedips.Add("84.58.59")
            'MasterListener.bannedips.Add("69.137.220")
            'MasterListener.bannedips.Add("41.234.122") 'server alpha
            'MasterListener.bannedips.Add("41.44.250") 'server alpha
            'MasterListener.bannedips.Add("86.5.21.233")
            'MasterListener.bannedips.Add("86.5.21")
        End If
        endpoint = New Net.IPEndPoint(Net.IPAddress.Any, 0)

        tcpclass.Start()

        'MAIN LOOP
        Do
            listmgr.Tick()

            'tcp.UpdateListData(listmgr.GetListBinary) 'list used by game
            'tcp.UpdateListText(listmgr.GetListText) 'list by used by web query and nfbsp client
            tcpclass.Tick()
            Sleep(3)
        Loop
    End Sub

    Private Function GetCommandLineArguments()
        Dim argl As New Dictionary(Of String, String)
        Dim argsingle As New List(Of String)
        Dim arg = System.Environment.GetCommandLineArgs
        Dim hasargs = False
        If arg.Length > 1 Then
            hasargs = True
        End If
        Dim key As String = ""
        Dim value As String = ""
        For i As Integer = 0 To arg.Length - 1
            Dim temp = arg(i)
            If temp.StartsWith("-") Then
                key = temp
                argsingle.Add(key.ToLower)
            ElseIf key.Length > 0 Then
                value = temp
                If argl.ContainsKey(key) Then
                    argl(key) = value
                Else
                    argl.Add(key, value)
                End If
                key = ""
            End If
        Next
        argl.TryGetValue("-ip", listenip)
        If Not listenip = Nothing Then
            Return True
        End If
        Return False
    End Function

    Private Sub InitializeUDP()
        Try
            udp = New Net.Sockets.UdpClient(masterudpport)
        Catch
            Console.WriteLine("ERROR: Failed to create socket on UDP Port " & masterudpport)
            Sleep(2000)
            Environment.Exit(1) 'failed, better crash now
        End Try
    End Sub

    Public Sub SendUDPPacket(ByVal host As String, ByVal port As Integer, ByVal data As Byte(), ByVal client As Net.Sockets.UdpClient)
        Try
            client.Send(data, data.Length, host, port)
        Catch ex As Exception
            Console.WriteLine("ERROR: Failed to send UDP packet, reason: " & ex.Message)
        End Try
    End Sub

    'Private Function ReopenSocket()
    '    endpoint = New Net.IPEndPoint(Net.IPAddress.Any, 0)
    '    udp.Close()
    '    Try
    '        udp = New Net.Sockets.UdpClient(masterport)
    '    Catch
    '        Console.WriteLine("ERROR: UDP Port 27900 already in use!")
    '        Sleep(2000)
    '        Return False 'failed
    '    End Try
    '    Return True
    'End Function
End Module

