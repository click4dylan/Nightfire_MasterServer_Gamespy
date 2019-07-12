Imports System.Net
Imports System.Net.Sockets
Imports System.Text


Public Class MasterListener

    Private tcp As TcpListener
    Public Shared bannedips As New List(Of String)
    Private clients As New Dictionary(Of String, TcpClient)
    Private lastrecv As New Dictionary(Of String, Integer)
    Private remove As New Queue(Of String)

    Private serverlist As Byte()
    Private serverlist_text As Byte()

    Public Sub UpdateListData(ByVal gamename As String) '(ByVal list As Byte())
        Dim list As Byte() = listmgr.GetListBinary(gamename)
        Dim encoded As Byte() = enctype2_encoder(list, list.Length, ReturnGamespySalt(gamename)) '(list, list.Length)
        serverlist = encoded
    End Sub
    Public Sub UpdateListText(ByVal gamename As String) '(ByVal list As String)
        Dim data = Text.ASCIIEncoding.ASCII.GetBytes(listmgr.GetListText(gamename)) '(list)

        Dim encoded As Byte() = enctype2_encoder(data, data.Length, ReturnGamespySalt(gamename))
        serverlist_text = encoded
    End Sub


    Public Sub New(ByVal ip As String, ByVal port As Integer)
        tcp = New TcpListener(IPAddress.Parse(ip), port)
    End Sub

    Public Sub Tick()
        ' Accept Pending Connections
        If tcp.Pending Then
            Dim client = tcp.AcceptTcpClient()
            client.NoDelay = False
            clients.Add(client.Client.RemoteEndPoint.ToString, client)
            lastrecv(client.Client.RemoteEndPoint.ToString) = Environment.TickCount
            Connected(client)
        End If

        'Processing for each client
        For Each a As KeyValuePair(Of String, TcpClient) In clients
            Dim client = a.Value
            Dim stream = client.GetStream
            If stream.DataAvailable Then
                lastrecv(a.Key) = Environment.TickCount
                Receive(client)
            End If

            'Check heartbeat timeout
            Dim time = (Environment.TickCount - lastrecv(a.Key)) / 1000
            If time > 4 Then
                remove.Enqueue(a.Key)
            End If
        Next

        'Process IP Removal
        ProcessIPRemove()
    End Sub

    Private Sub ProcessIPRemove()
        While remove.Count > 0
            On Error Resume Next
            Dim item = remove.Dequeue
            clients(item).Client.Disconnect(False)
            clients(item).Client.Close()
            clients(item).Close()
            clients.Remove(item)
            lastrecv.Remove(item)
        End While
    End Sub

    Public Sub Start()
        tcp.Start()
        Tick()
    End Sub

    Private Sub Connected(ByRef client As TcpClient)
        Dim stream = client.GetStream
        Dim send = GenerateToken(client)

        Dim data = Text.ASCIIEncoding.ASCII.GetBytes(send)
        stream.Write(data, 0, data.Length)
    End Sub

    Private tokens As New Dictionary(Of String, Integer)
    Private newtokens As New Dictionary(Of TcpClient, String)
    Private Function GenerateToken(ByRef client As TcpClient) As String
        Dim s As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        Dim r As New Random
        Dim sb As New StringBuilder
        For i As Integer = 1 To 6
            Dim idx As Integer = r.Next(0, 25)
            sb.Append(s.Substring(idx, 1))
        Next
        Dim token = "\basic\\secure\" & sb.ToString

        If newtokens.ContainsKey(client) Then
            newtokens(client) = sb.ToString
        Else
            newtokens.Add(client, sb.ToString)
        End If

        'Original below, dylan added above

        'Dim verify = makesec(token)

        'If tokens.ContainsKey(verify) Then
        '    tokens(verify) = Environment.TickCount
        'Else
        '    tokens.Add(verify, Environment.TickCount)
        'End If

        'Dim remove As New List(Of String)
        'For Each a As KeyValuePair(Of String, Integer) In tokens
        '    If (Environment.TickCount - a.Value) > 8 Then
        '        remove.Add(a.Key)
        '    End If
        'Next
        'For Each a In remove
        '    tokens.Remove(a)
        'Next
        Return token
    End Function
    Private Function CheckIfBanned(ByRef client As TcpClient) 'short ip contains 3 numbers
        Dim returnaddress As String = client.Client.RemoteEndPoint.ToString
        Dim returnaddressshort As String
        Dim spl = returnaddress.Split(":")
        returnaddressshort = spl(0)
        returnaddress = spl(0)
        spl = returnaddressshort.Split(".")
        returnaddressshort = spl(0) & "." & spl(1) & "." & spl(2)
        If bannedips.Contains(returnaddress) Then
            Return True
        ElseIf bannedips.Contains(returnaddressshort) Then
            Return True
        End If
        Return False
    End Function
    Private Sub Receive(ByRef client As TcpClient)
        'Vars
        Dim stream = client.GetStream
        Dim data(client.ReceiveBufferSize) As Byte
        stream.Read(data, 0, client.ReceiveBufferSize)
        Dim text = GetString(data)
        Dim gamename = SlashValue(text.Split("\"), "gamename")
        Dim validate = SlashValue(text.Split("\"), "validate")

        'Process all received data

        If CheckIfBanned(client) Then
            Console.WriteLine("Received query from banned IP address")
            Exit Sub
        End If

        If newtokens.ContainsKey(client) Then
            Dim verify = makesec(newtokens(client), gamename)
            If validate = verify Then
                'Validation code accepted
                If text.Contains("\cmp\") Or text.Contains("queryid") Then
                    tcpclass.UpdateListData(gamename)
                    SendListBinary(client)
                Else
                    tcpclass.UpdateListText(gamename)
                    SendListText(client)
                End If
            End If
            newtokens.Remove(client)
        End If

        'If tokens.ContainsKey(validate) Then
        '    tokens.Remove(validate)

        '    If text.Contains("\cmp\") Or text.Contains("queryid") Then
        '        SendListBinary(client)
        '    Else
        '        SendListText(client)
        '    End If

        'End If



    End Sub

    Private Function SlashValue(ByRef data As String(), ByVal key As String) As String
        Dim found = False
        For Each a In data
            If found Then
                Return a
            End If
            If a = key Then
                found = True
            End If
        Next
        Return ""
    End Function

    Private Sub SendListBinary(ByRef client As TcpClient)
        Dim stream As NetworkStream = client.GetStream
        stream.Write(serverlist, 0, serverlist.Length)
        Console.WriteLine("Client binary refresh received from " & client.Client.RemoteEndPoint.ToString)
        remove.Enqueue(client.Client.RemoteEndPoint.ToString)
    End Sub

    Private Sub SendListText(ByRef client As TcpClient)
        Dim stream As NetworkStream = client.GetStream
        stream.Write(serverlist_text, 0, serverlist_text.Length)

        Console.WriteLine("Client text refresh received from " & client.Client.RemoteEndPoint.ToString)
        remove.Enqueue(client.Client.RemoteEndPoint.ToString)
    End Sub

    Private Function GetString(ByRef data As Byte()) As String
        Dim length = 0
        For i As Integer = 0 To data.Length - 1
            If data(i) = 0 Then
                length = i
                Exit For
            End If
        Next
        Return Text.ASCIIEncoding.ASCII.GetString(data, 0, length)
    End Function

    Private Function makesec(ByRef data As String, ByVal gamename As String) As String
        Dim chrs() As Char = {"\", Chr(0)}
        Dim spl() As String = data.Split(chrs, StringSplitOptions.RemoveEmptyEntries)
        Return gsseckey(System.Text.Encoding.ASCII.GetBytes(spl(spl.Length - 1)), 2, ReturnGamespySalt(gamename))
    End Function

    Private Function gsseckey(ByRef SecureKey() As Byte, ByVal enctype As Integer, ByVal handoff As String) As String

        Dim gamekey As String = ""

        Dim Temp(3) As Integer
        Dim Table(255) As Byte
        Dim Length(1) As Integer
        Dim Key(5) As Byte
        Dim i As Integer

        Dim enctype1_data() As Byte = {1, 186, 250, 178, 81, 0, 84, 128, 117, 22, 142, 142, 2, 8, 54, 165, 45, 5, 13, 22, 82, 7, 180, 34, 140, 233, 9, 214, 185, 38, 0, 4, 6, 5, 0, 19, 24, 196, 30, 91, 29, 118, 116, 252, 80, 81, 6, 22, 0, 81, 40, 0, 4, 10, 41, 120, 81, 0, 1, 17, 82, 22, 6, 74, 32, 132, 1, 162, 30, 22, 71, 22, 50, 81, 154, 196, 3, 42, 115, 225, 45, 79, 24, 75, 147, 76, 15, 57, 10, 0, 4, 192, 18, 12, 154, 94, 2, 179, 24, 184, 7, 12, 205, 33, 5, 192, 169, 65, 67, 4, 60, 82, 117, 236, 152, 128, 29, 8, 2, 29, 88, 132, 1, 78, 59, 106, 83, 122, 85, 86, 87, 30, 127, 236, 184, 173, 0, 112, 31, 130, 216, 252, 151, 139, 240, 131, 254, 14, 118, 3, 190, 57, 41, 119, 48, 224, 43, 255, 183, 158, 1, 4, 248, 1, 14, 232, 83, 255, 148, 12, 178, 69, 158, 10, 199, 6, 24, 1, 100, 176, 3, 152, 1, 235, 2, 176, 1, 180, 18, 73, 7, 31, 95, 94, 93, 160, 79, 91, 160, 90, 89, 88, 207, 82, 84, 208, 184, 52, 2, 252, 14, 66, 41, 184, 218, 0, 186, 177, 240, 18, 253, 35, 174, 182, 69, 169, 187, 6, 184, 136, 20, 36, 169, 0, 20, 203, 36, 18, 174, 204, 87, 86, 238, 253, 8, 48, 217, 253, 139, 62, 10, 132, 70, 250, 119, 184}

        ' 1) buffer creation with incremental data 

        For i = 0 To 255
            Table(i) = i
        Next
        Length(0) = handoff.Length
        Length(1) = SecureKey.Length

        ' 2) buffer scrambled with key 

        For i = 0 To 255
            Temp(0) = (Temp(0) + Table(i) + AscW(handoff(i Mod 6))) And 255
            Temp(2) = Table(Temp(0))
            Table(Temp(0)) = Table(i)
            Table(i) = Temp(2)
        Next

        Temp(0) = 0
        Dim keyredim As Integer = Length(1) - 1
        Length(1) = Length(1) / 3

        ' 3) source string scrambled with the buffer

        For i = 0 To keyredim
            Key(i) = SecureKey(i)

            Temp(0) = (Temp(0) + Key(i) + 1) And 255
            Temp(2) = Table(Temp(0))

            Temp(1) = (Temp(1) + Temp(2)) And 255
            Temp(3) = Table(Temp(1))

            Table(Temp(1)) = Temp(2)
            Table(Temp(0)) = Temp(3)
            Key(i) = (Key(i) Xor Table((Temp(2) + Temp(3)) And 255))

        Next

        If enctype = 1 Then
            For i = 0 To 5
                Key(i) = enctype1_data(Key(i))
            Next i
        ElseIf enctype = 2 Then
            For i = 0 To 5
                Key(i) = Key(i) Xor AscW(handoff(i Mod 6))
            Next i
        End If

        ' 5) splitting of the source string from 3 to 4 bytes 

        Dim str As String = ""
        Dim j As Integer = 0
        While (Length(1) >= 1)
            Length(1) = Length(1) - 1

            Temp(2) = Key(j)
            Temp(3) = Key(j + 1)

            str = str & gsvalfunc(Temp(2) >> 2)
            str = str & gsvalfunc(((Temp(2) And 3) << 4) Or Temp(3) >> 4)

            Temp(2) = Key(j + 2)

            str = str & gsvalfunc(((Temp(3) And 15) << 2) Or (Temp(2) >> 6))
            str = str & gsvalfunc(Temp(2) And 63)

            j += 3
        End While

        Return str
    End Function
    Private Function gsvalfunc(ByVal number As Integer) As Char
        Dim newChar As Char

        Select Case number
            Case Is < 26
                newChar = Chr(number + 65)
            Case Is < 52
                newChar = Chr(number + 71)
            Case Is < 62
                newChar = Chr(number - 4)
            Case 62
                newChar = "+"
            Case 63
                newChar = "/"
        End Select

        Return newChar
    End Function
End Class



