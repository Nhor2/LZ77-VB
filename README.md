# RTF-LZ77-VB

![License](https://img.shields.io/badge/License-MIT-green.svg)
![VB.NET](https://img.shields.io/badge/VB.NET-blue.svg)
![WinForms](https://img.shields.io/badge/WinForms-.NET-lightblue.svg)
![.NET Framework](https://img.shields.io/badge/.NET_Framework-4.8.1-purple.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-informational.svg)

### High-Integrity LZ77 Compression for Rich Text Format (RTF)

Questo progetto implementa un algoritmo di compressione **LZ77 custom** in VB.NET, progettato specificamente per gestire file RTF complessi contenenti immagini e metadati binari senza perdere un singolo byte.

---

## 🚀 Caratteristiche principali

* **Binary Perfect:** Supera i test `SequenceEqual`, garantendo che il file decompresso sia identico all'originale bit-per-bit.
* **Ottimizzato per RTF:** Gestisce perfettamente i tag di formattazione e le stringhe esadecimali delle immagini.
* **Ricerca "Smart-Brute":** Algoritmo di ricerca ottimizzato con filtri a 2-byte e *Early Exit* per bilanciare velocità e rapporto di compressione.
* **Header di Sicurezza:** Include un Magic Number (`LZ77`) e il salvataggio della dimensione originale.

---

## 🛠 Specifiche Tecniche

* **Finestra mobile (Sliding Window):** 4096 byte.
* **Lunghezza Match:** 3 - 15 byte (codificati in 4 bit).
* **Token:** 16-bit (12 bit distanza, 4 bit lunghezza).
* **Performance:** ~5-6 secondi per file da 4MB (Pure VB.NET Managed Code).

---

## 📖 Esempio d'uso

```vbnet
' Compressione con Header personalizzato
Dim inputBytes As Byte() = File.ReadAllBytes("C:\Temp\documento.rtf")
Dim comp = SimpleLz77.Compress(inputBytes)
File.WriteAllBytes("C:\Temp\documento.lz77", comp)

' Decompressione con verifica integrità
Dim decomp = SimpleLz77.Decompress(File.ReadAllBytes("C:\Temp\documento.lz77"))
' Risultato: identico all'originale!

```

## ⚖️ Licenza

Distribuito sotto licenza MIT. Vedi `LICENSE` per maggiori informazioni.
