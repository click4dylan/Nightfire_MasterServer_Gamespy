Module enctype2

    Public e2_key As String = "S9j3L2"

    Public Function enctype2_encoder(ByVal data2 As Byte(), ByVal size As Integer)

        Dim dest(325) As UInteger
        Dim data(size + 6) As Byte
        Dim header(7) As Byte
        data2.CopyTo(data, 0)


        encshare4(header, 8, dest)

        encshare1(dest, data, size + 6)


        Dim datalast(header.Length + data.Length) As Byte
        datalast(0) = 8
        header.CopyTo(datalast, 1)
        data.CopyTo(datalast, 9)

        For i As Integer = 0 To e2_key.Length - 1
            datalast(i + 1) = datalast(i + 1) Xor AscW(e2_key(i))
        Next

        datalast(0) = datalast(0) Xor &HEC


        Return datalast
    End Function


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
