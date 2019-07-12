Module enctype2

    Public Function ReturnGamespySalt(ByVal gamename As String) As String
        Select Case gamename
            Case "jbnightfire"
                Return "S9j3L2"
            Case "sc2kne"
                Return "SC2KNE"
            Case "rmth2003"
                Return "Y4kC7S"
        End Select
        Return "??????"
    End Function

    'Public e2_key As String = "S9j3L2"


    'Public Function enctype2_encoder(ByVal data2 As Byte(), ByVal size As Integer)

    '    Dim dest(325) As UInteger
    '    Dim data(size + 6) As Byte
    '    Dim rand As New Random
    '    Dim header(7) As Byte
    '    header = System.Text.Encoding.ASCII.GetBytes("|.|.|.|")
    '    'rand.NextBytes(header)
    '    data2.CopyTo(data, 0)


    '    encshare4(header, 8, dest)

    '    encshare1(dest, data, size + 6)


    '    Dim datalast(header.Length + Data.Length) As Byte
    '    datalast(0) = 8
    '    header.CopyTo(datalast, 1)
    '    data.CopyTo(datalast, 9)

    '    For i As Integer = 0 To e2_key.Length - 1
    '        datalast(i + 1) = datalast(i + 1) Xor AscW(e2_key(i))
    '    Next

    '    datalast(0) = datalast(0) Xor &HEC


    '    Return datalast
    'End Function
    'non-encrypted header version

    'Public Function enctype2_encoder(ByVal data2 As Byte(), ByVal size As Integer)

    '    Dim dest(325) As UInteger
    '    Dim data(size + 6) As Byte 'size
    '    Dim header(7) As Byte
    '    data2.CopyTo(data, 0)


    '    encshare4(header, 8, dest)

    '    encshare1(dest, data, size + 6)
    '    'encshare1(dest, data, size + 7)


    '    Dim datalast(header.Length + data.Length) As Byte
    '    datalast(0) = 8
    '    header.CopyTo(datalast, 1)
    '    data.CopyTo(datalast, 9)

    '    For i As Integer = 0 To e2_key.Length - 1
    '        datalast(i + 1) = datalast(i + 1) Xor AscW(e2_key(i))
    '    Next

    '    datalast(0) = datalast(0) Xor &HEC


    '    Return datalast
    'End Function
    'Private Function enctype2_encoder(ByVal key() As Byte, ByVal data() As Byte, ByVal size As Integer) As Integer
    '    Dim dest(325) As UInteger
    '    Dim i As Integer
    '    'C++ TO VB CONVERTER TODO TASK: VB does not have an equivalent for pointers to value types:
    '    'ORIGINAL LINE: Byte *datap;
    '    Dim datap As Byte
    '    Dim header_size As Integer = 8

    '    For i = size - 1 To 0 Step -1
    '        data(1 + header_size + i) = data(i)
    '    Next i
    '    data = header_size

    '    datap = data + 1
    '    'C++ TO VB CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in VB:
    '    memset(datap, 0, data)

    '    For i = 256 To 325
    '        dest(i) = 0
    '    Next i
    '    encshare4(datap, data, dest)

    '    'C++ TO VB CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in VB:
    '    memset(data + 1 + data + size, 0, 6)
    '    encshare1(dest, datap + data, size + 6)

    '    i = 0
    '    Do While key(i) <> 0
    '        datap(i) = datap(i) Xor key(i)
    '        i += 1
    '    Loop
    '    size += 1 + data + 6
    '    data = data Xor &HEC
    '    Return size
    'End Function

    Public Function enctype2_encoder(ByVal data2 As Byte(), ByVal size As Integer, ByVal e2_key As String)

        Dim dest(325) As UInteger
        Dim data(size + 6) As Byte 'size
        Dim header(7) As Byte
        data2.CopyTo(data, 0)


        encshare4(header, 8, dest)

        encshare1(dest, data, size + 7)


        Dim datalast(header.Length + data.Length) As Byte
        datalast(0) = 8
        header.CopyTo(datalast, 1)
        data.CopyTo(datalast, 9)

        For i As Integer = 0 To e2_key.Length - 1
            datalast(i + 1) = datalast(i + 1) Xor AscW(e2_key(i))
        Next

        datalast(0) = datalast(0) Xor &HEC


        Dim clipped(datalast.Length - 8) As Byte

        Dim j As Integer = 0
        While j < clipped.Length
            clipped(j) = datalast(j)
            j += 1
        End While

        Return clipped
    End Function

    'Public Function enctype2_encoder(ByVal data As Byte(), ByVal size As Integer)
    '    'enctype2_encoder(data, size)
    '    Dim dest(326) As UInteger
    '    Dim i As Integer ' = size - 1
    '    Dim bloat = size + 15
    '    Dim data_bloated(bloat) As Byte
    '    Dim datap(bloat - 1) As Byte

    '    data.CopyTo(data_bloated, 0)
    '    Dim header_size As Integer = 8
    '    For i = size - 1 To 0 Step -1
    '        data_bloated(1 + header_size + i) = data(i)
    '    Next
    '    Dim datanew(header_size) As Byte

    '    data_bloated(0) = header_size
    '    Array.Copy(data_bloated, 1, datap, 0, datap.Length)

    '    Array.Clear(datap, 0, datap.Length)

    '    For i = 256 To 325
    '        dest(i) = 0
    '    Next
    '    encshare4(datap, data_bloated.Length - 1, dest)

    '    'Array.Clear(data + 1 + data + size, 0, 6)
    '    encshare1(dest, datap, size + 6)
    '    For i = 0 To e2_key.Length - 1
    '        datap(i + 1) = datap(i + 1) Xor AscW(e2_key(i))
    '    Next

    '    datap(0) = datap(0) Xor &HEC
    '    Return datap
    '    'data.Target = header_size

    '    'datap = data + 1
    'End Function

    'data must be big enough to contain also the following bytes: 1 + 8 + 6

    'int enctype2_encoder(unsigned char *key, unsigned char *data, int size) {
    '    unsigned int    dest[326];
    '    int             i;
    '    unsigned char   *datap;
    '    int             header_size = 8;

    '    for(i = size - 1; i >= 0; i--) {
    '        data[1 + header_size + i] = data[i];
    '    }
    '    *data = header_size;

    '    datap = data + 1;
    '    memset(datap, 0, *data);

    '    for(i = 256; i < 326; i++) dest[i] = 0;
    '    encshare4(datap, *data, dest);

    '    memset(data + 1 + *data + size, 0, 6);
    '    encshare1(dest, datap + *data, size + 6);

    '    for(i = 0; key[i]; i++) datap[i] ^= key[i];
    '    size += 1 + *data + 6;
    '    *data ^= 0xec;
    '    return size;
    '}

    Public Sub encshare4(ByRef src As Byte(), ByVal size As Integer, ByRef dest As UInteger())
        Dim tmp As UInteger
        Dim i, j, pos, x, y As Integer

        For i = 0 To 255
            dest(i) = 0
        Next

        For y = 0 To 3
            For i = 0 To 255
                dest(i) = ((dest(i) << 8) + i) And 4294967295
            Next

            pos = y
            For x = 0 To 1
                For i = 0 To 255
                    tmp = dest(i)
                    pos = (pos + (tmp + src(i Mod size)) And 255) 'crash here arithmetic overflow
                    dest(i) = dest(pos)
                    dest(pos) = tmp
                Next
            Next
        Next

        For i = 0 To 255
            dest(i) = dest(i) Xor i
        Next


        encshare3(dest, 0, 0)


    End Sub


    Function SumaOverF(ByVal a As ULong, ByVal b As ULong) As UInteger
        Dim res As ULong = a + b
        res = res Mod 4294967296
        Return res
    End Function

    Public Sub encshare3(ByRef data As UInteger(), ByVal n1 As Integer, ByVal n2 As Integer)
        Dim t1 As UInteger
        Dim t2 As UInteger
        Dim t3 As UInteger
        Dim t4 As UInteger
        Dim i As Integer
        t2 = n1
        t1 = 0
        t4 = 1
        data(304) = 0
        i = 32768

        While (Not i = 0)
            t2 = SumaOverF(t2, t4)
            t1 = SumaOverF(t1, t2)
            t2 = SumaOverF(t2, t1)
            If (Not ((n2 And i) = 0)) Then
                t2 = Not t2
                t4 = (t4 << 1) + 1
                t3 = (t2 << 24) Or (t2 >> 8)
                t3 = t3 Xor data(t3 And 255)
                t1 = t1 Xor data(t1 And 255)
                t2 = (t3 << 24) Or (t3 >> 8)
                t3 = (t1 >> 24) Or (t1 << 8)
                t2 = t2 Xor data(t2 And 255)
                t3 = t3 Xor data(t3 And 255)
                t1 = (t3 >> 24) Or (t3 << 8)
            Else
                data(data(304) + 256) = t2
                data(data(304) + 272) = t1
                data(data(304) + 288) = t4
                data(304) += 1
                t3 = ((t1 << 24) Or (t1 >> 8))
                t2 = t2 Xor data(t2 And 255)
                t3 = t3 Xor data(t3 And 255)
                t1 = ((t3 << 24) Or (t3 >> 8))
                t3 = ((t2 >> 24) Or (t2 << 8))
                t3 = t3 Xor data(t3 And 255)
                t1 = t1 Xor data(t1 And 255)
                t2 = ((t3 >> 24) Or (t3 << 8))
                t4 <<= 1
            End If

            i >>= 1
        End While
        data(305) = t2
        data(306) = t1
        data(307) = t4
        data(308) = n1
    End Sub

    Dim p_ind As Integer
    Sub encshare1(ByVal tbuff() As UInteger, ByRef datap() As Byte, ByVal len As Integer)
        p_ind = 309
        Dim s_ind As Integer = 309

        Dim datap_ind As Integer = 0
        Dim lalind As Integer = 309

        Dim bytepart As Integer = 4
        Dim ByteArray(3) As Byte
        While (len > 0)
            If (datap_ind Mod 63 = 0) Then
                p_ind = s_ind
                lalind = 309
                bytepart = 4
                encshare2(tbuff, 16)
            End If

            If bytepart > 3 Then
                Dim t As UInteger = tbuff(lalind)
                ByteArray = BitConverter.GetBytes(t)
                bytepart = 0
                lalind += 1
            End If
            datap(datap_ind) = (datap(datap_ind) Xor ByteArray(bytepart)) Mod 256
            datap_ind += 1
            p_ind += 1
            bytepart += 1
            len -= 1
        End While
    End Sub


    Sub encshare2(ByRef tbuff() As UInteger, ByVal len As Integer)
        Dim t1 As UInteger
        Dim t2 As UInteger
        Dim t3 As UInteger
        Dim t4 As UInteger
        Dim t5 As UInteger
        Dim old_p_ind As Integer = p_ind
        t2 = tbuff(304)
        t1 = tbuff(305)
        t3 = tbuff(306)
        t5 = tbuff(307)
        Dim cnt As Integer = 0
        For i As Integer = 0 To len - 1
            p_ind = t2 + 272
            While (t5 < 65536)
                t1 = SumaOverF(t1, t5)
                p_ind += 1
                t3 = SumaOverF(t3, t1)
                t1 = SumaOverF(t1, t3)

                tbuff(p_ind - 17) = t1
                tbuff(p_ind - 1) = t3
                t4 = (t3 << 24) Or (t3 >> 8)
                tbuff(p_ind + 15) = t5

                t5 <<= 1

                t2 += 1

                t1 = t1 Xor tbuff(t1 And 255)
                t4 = t4 Xor tbuff(t4 And 255)

                t3 = (t4 << 24) Or (t4 >> 8)

                t4 = (t1 >> 24) Or (t1 << 8)
                t4 = t4 Xor tbuff(t4 And 255)
                t3 = t3 Xor tbuff(t3 And 255)

                t1 = (t4 >> 24) Or (t4 << 8)
            End While
            t3 = t3 Xor t1
            tbuff(old_p_ind + i) = t3
            t2 -= 1

            t1 = tbuff(t2 + 256)
            t5 = tbuff(t2 + 272)
            t1 = Not t1 't1 = ~t1;

            t3 = (t1 << 24) Or (t1 >> 8)

            t3 = t3 Xor tbuff(t3 And 255)
            t5 = t5 Xor tbuff(t5 And 255)
            t1 = (t3 << 24) Or (t3 >> 8)

            t4 = (t5 >> 24) Or (t5 << 8)

            t1 = t1 Xor tbuff(t1 And 255)
            t4 = t4 Xor tbuff(t4 And 255)

            t3 = (t4 >> 24) Or (t4 << 8)

            t5 = (tbuff(t2 + 288) << 1) + 1
            cnt += 1
        Next
        tbuff(304) = t2
        tbuff(305) = t1
        tbuff(306) = t3
        tbuff(307) = t5
    End Sub

End Module
