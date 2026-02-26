Imports System.IO

Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RichTextBox1.LoadFile("C:\Temp\images-text.rtf")
        Dim rtfOriginal As String = RichTextBox1.Rtf

        ' Compressione RTF 100%
        Dim inputBytes As Byte() = System.IO.File.ReadAllBytes("C:\Temp\images-text.rtf")
        Dim comp = SimpleLz77.Compress(inputBytes)
        File.WriteAllBytes("C:\Temp\compressed.rtf", comp)

        ' Decompressione RTF 100%
        Dim inputDecompBytes As Byte() = System.IO.File.ReadAllBytes("C:\Temp\compressed.rtf")
        Dim decomp = SimpleLz77.Decompress(inputDecompBytes)

        ' Salva direttamente i byte, senza conversione in stringa
        File.WriteAllBytes("C:\Temp\roundtrip.rtf", decomp)

        ' Carica il file nel RichTextBox
        RichTextBox2.Clear()
        ' Forza il sistema a liberare risorse prima di caricare 4MB di RTF
        GC.Collect()
        RichTextBox2.LoadFile("C:\Temp\roundtrip.rtf")

        ' Confronto binario
        Dim identici As Boolean = inputBytes.SequenceEqual(decomp)
        MessageBox.Show("I file sono identici byte-per-byte? " & identici.ToString())

        ' Comparazione
        Compare("C:\Temp\images-text.rtf", "C:\Temp\roundtrip.rtf")
    End Sub

    Public Sub Compare(name1 As String, name2 As String)
        ' Comprarazione di files RTF
        If Not File.Exists(name1) OrElse Not File.Exists(name2) Then
            RichTextBox2.AppendText("Uno dei due file non esiste." & vbCrLf)
            Return
        End If

        Dim bytes1() As Byte = File.ReadAllBytes(name1)
        Dim bytes2() As Byte = File.ReadAllBytes(name2)

        Dim maxLen As Integer = Math.Max(bytes1.Length, bytes2.Length)
        Dim diffCount As Integer = 0

        RichTextBox2.Clear()
        RichTextBox2.AppendText($"Confronto file '{name1}' vs '{name2}'" & vbCrLf)
        RichTextBox2.AppendText($"Lunghezze: {bytes1.Length} vs {bytes2.Length}" & vbCrLf)

        For i As Integer = 0 To maxLen - 1
            Dim b1 As String = If(i < bytes1.Length, bytes1(i).ToString("X2"), "??")
            Dim b2 As String = If(i < bytes2.Length, bytes2(i).ToString("X2"), "??")

            If b1 <> b2 Then
                RichTextBox2.AppendText($"Offset {i}: {b1} != {b2}" & vbCrLf)
                diffCount += 1
            End If

            ' Limita a 500 differenze per non saturare il RichTextBox
            If diffCount >= 500 Then
                RichTextBox2.AppendText("... altre differenze non mostrate ..." & vbCrLf)
                Exit For
            End If
        Next

        RichTextBox2.AppendText($"Totale differenze trovate: {diffCount}" & vbCrLf)
    End Sub
End Class

Public Class SimpleLz77
    'Il segreto è stato gestire correttamente il Token di 16 bit.
    'Dividendo i 16 bit in 12 per la distanza (che copre la finestra da 4KB) e 4 per la lunghezza (che copre i match da 3 a 18 byte),
    'abbiamo creato un sistema compatto e privo di ambiguità. Inoltre, l'uso di List(Of Byte) ha eliminato i problemi di
    'posizionamento dello stream che mandavano in crash la memoria.

    ' LZ77
    '4 Byte: Identificativo "LZ77" (Magic Number)
    '4 Byte: Dimensione del file originale (Uncompressed Size)
    'N Byte: Dati compressi

    Private Const WindowSize As Integer = 4095
    Private Const MaxMatch As Integer = 15 ' Ridotto a 15 per stare in 4 bit (0-15)
    Private Const MinMatch As Integer = 3
    Private Const MarkerByte As Byte = &HFF

    Public Shared Function Compress(input As Byte()) As Byte()
        Using ms As New MemoryStream()
            Dim bw As New BinaryWriter(ms)

            ' --- SCRITTURA HEADER ---
            bw.Write(System.Text.Encoding.ASCII.GetBytes("LZ77")) ' Magic Number
            bw.Write(input.Length) ' Dimensione Originale (4 byte)

            Dim pos As Integer = 0
            While pos < input.Length
                Dim matchDist As Integer = 0
                Dim matchLen As Integer = FindMatch(input, pos, matchDist)

                If matchLen >= MinMatch Then
                    Dim lengthCode As Integer = matchLen - MinMatch
                    Dim token As UShort = CUShort(((lengthCode And &HF) << 12) Or (matchDist And &HFFF))

                    ms.WriteByte(MarkerByte)
                    ms.WriteByte(CByte((token >> 8) And &HFF))
                    ms.WriteByte(CByte(token And &HFF))
                    pos += matchLen
                Else
                    Dim b As Byte = input(pos)
                    ms.WriteByte(b)
                    If b = MarkerByte Then ms.WriteByte(MarkerByte)
                    pos += 1
                End If
            End While
            Return ms.ToArray()
        End Using
    End Function

    Public Shared Function Decompress(input As Byte()) As Byte()
        Using msInput As New MemoryStream(input)
            Dim br As New BinaryReader(msInput)

            ' --- LETTURA E VERIFICA HEADER ---
            Dim magic As String = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4))
            If magic <> "LZ77" Then Throw New Exception("Formato file non supportato o non compresso.")

            Dim originalSize As Integer = br.ReadInt32()

            ' Pre-allochiamo la lista con la dimensione esatta
            Dim output As New List(Of Byte)(originalSize)

            ' Continuiamo a leggere fino alla fine dello stream
            While msInput.Position < msInput.Length
                Dim b As Byte = br.ReadByte()

                If b = MarkerByte Then
                    ' Controllo di sicurezza per non sforare lo stream
                    If msInput.Position >= msInput.Length Then Exit While

                    Dim nextByte As Byte = br.ReadByte()
                    If nextByte = MarkerByte Then
                        output.Add(MarkerByte)
                    Else
                        If msInput.Position < msInput.Length Then
                            Dim secondByte As Byte = br.ReadByte()
                            Dim token As Integer = (CInt(nextByte) << 8) Or secondByte
                            Dim length As Integer = ((token >> 12) And &HF) + MinMatch
                            Dim distance As Integer = token And &HFFF

                            Dim readPos As Integer = output.Count - distance
                            For k As Integer = 0 To length - 1
                                output.Add(output(readPos + k))
                            Next
                        End If
                    End If
                Else
                    output.Add(b)
                End If
            End While
            Return output.ToArray()
        End Using
    End Function

    Private Shared Function FindMatch(data As Byte(), pos As Integer, ByRef matchDist As Integer) As Integer
        Dim bestLen As Integer = 0
        Dim startLook As Integer = Math.Max(0, pos - WindowSize)
        Dim remaining As Integer = data.Length - pos
        Dim limit As Integer = Math.Min(MaxMatch, remaining)

        If limit < MinMatch Then Return 0

        ' Più alta è la soglia, più comprime ma più è lento. 
        ' 8-10 è il "punto dolce" per gli RTF.
        Dim GoodEnoughMatch As Integer = 10

        For i As Integer = pos - 1 To startLook Step -1
            ' Sostituisci la parte interna del ciclo For in FindMatch con questa:

            ' 1. Filtro veloce: controlla i primi due byte. 
            ' Aggiungiamo il controllo per assicurarci che ci sia spazio per il secondo byte
            If (i + 1 >= data.Length) OrElse (pos + 1 >= data.Length) OrElse
   data(i) <> data(pos) OrElse data(i + 1) <> data(pos + 1) Then Continue For

            ' 2. Se abbiamo già un bestLen, verifichiamo se questa posizione può almeno pareggiarlo
            ' Usiamo 'bestLen - 1' per restare entro i limiti già scoperti
            If bestLen > 0 AndAlso (i + bestLen < data.Length) AndAlso (pos + bestLen < data.Length) Then
                If data(i + bestLen) <> data(pos + bestLen) Then Continue For
            End If

            Dim currentLen As Integer = 2 ' Abbiamo già verificato i primi 2 con il filtro sopra
            While currentLen < limit AndAlso data(i + currentLen) = data(pos + currentLen)
                currentLen += 1
            End While

            If currentLen > bestLen Then
                bestLen = currentLen
                matchDist = pos - i

                ' --- OTTIMIZZAZIONE TURBO ---
                ' Se il match trovato è "abbastanza buono", usciamo. 
                ' Risparmiamo migliaia di iterazioni.
                If bestLen >= GoodEnoughMatch Then Return bestLen
            End If
        Next

        Return If(bestLen >= MinMatch, bestLen, 0)
    End Function
End Class