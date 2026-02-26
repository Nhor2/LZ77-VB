\## LZ77-VB


High-Integrity LZ77 Compression for Rich Text Format (RTF)

Questo progetto implementa un algoritmo di compressione LZ77 custom in VB.NET, progettato specificamente per gestire file RTF complessi contenenti immagini e metadati binari. A differenza delle implementazioni standard, questa versione garantisce l'integrità binaria al 100% (Binary Identity).

🚀 Caratteristiche principali
Binary Perfect: Supera i test SequenceEqual, garantendo che il file decompresso sia identico all'originale bit-per-bit.

Ottimizzato per RTF: Gestisce perfettamente i tag di formattazione e le stringhe esadecimali delle immagini senza corruzioni.

Ricerca "Smart-Brute": Algoritmo di ricerca ottimizzato con filtri a 2-byte e Early Exit per bilanciare velocità e rapporto di compressione.

Header di Sicurezza: Include un Magic Number (LZ77) e il salvataggio della dimensione originale per una decompressione sicura e veloce.

Zero Dipendenze: Scritto in puro VB.NET, non richiede librerie esterne o DLL.

🛠 Specifiche Tecniche
Finestra mobile (Sliding Window): 4096 byte.

Lunghezza Match: 3 - 18 byte (codificati in 4 bit).

Token: 16-bit (12 bit distanza, 4 bit lunghezza).

Escape System: Gestione del MarkerByte (\&HFF) tramite raddoppio per evitare conflitti con i dati binari.

