# Changelog

Tutte le modifiche importanti a questo progetto verranno documentate in questo file.

Il formato è basato su [Keep a Changelog](https://keepachangelog.com/it/1.0.0/),
e questo progetto aderisce al [Semantic Versioning](https://semver.org/lang/it/).

## [1.0.0] - 2026-01-04

### ? Aggiunto

#### ??? Architettura
- Architettura modulare con interfaccia `ITool` per estensibilità
- Sistema di contesto di esecuzione (`ToolContext`) per operazioni asincrone
- Sistema di progress reporting con `ToolProgress`
- Pattern MVVM con ViewModels e Views separate
- Gestione errori con `ToolResult`

#### ?? Interfaccia Utente
- Tema dark moderno con accent color viola (#8A7CFF)
- Design responsivo con scrolling automatico
- Sidebar di navigazione con categorie
- Componenti stilizzati: Card, Button (Primary/Ghost), TextBox, ProgressBar
- ScrollBar personalizzate a tema dark
- Supporto cancellazione operazioni in corso

#### ?? Strumenti Crittografici
- **Hash SHA-256**: Calcolo hash di file con streaming ottimizzato
  - Progress bar in tempo reale
  - Supporto file di grandi dimensioni (buffer 81920 byte)
  - Copia hash negli appunti
  - Log dettagliato operazioni
- **Verifica Hash SHA-256**: Confronto e verifica integrità file
  - Risultato visivo chiaro (?/?)
  - Mostra entrambi gli hash per confronto

#### ?? Strumenti PDF
- **Merge PDF**: Unione di più file PDF
  - Selezione multipla file
  - Riordino con pulsanti Up/Down
  - Rimozione file dalla lista
  - Progress bar durante l'unione
- **Split PDF**: Divisione documenti PDF
  - Modalità per pagine (1, 2, 3, N pagine per file)
  - Modalità range personalizzati (es: "1-3,5-7,10")
  - Nomi file automatici con indicatori pagine
  - Progress bar per pagina
- **Compress PDF**: Ottimizzazione dimensioni PDF
  - 3 livelli di compressione (Bassa/Media/Alta)
  - Full compression mode iText7
  - Statistiche dettagliate (dimensione originale/compressa, % riduzione)
  - Avviso per file già ottimizzati

### ??? Tecnologie
- .NET 8.0 con WPF
- iText7 9.4.0 per manipolazione PDF
- Pattern MVVM per separazione logica/UI
- Operazioni asincrone con Task/async-await

### ?? Documentazione
- README.md completo con:
  - Guida all'installazione e build
  - Struttura progetto dettagliata
  - Documentazione tool disponibili
  - Guida per sviluppatori (aggiungere nuovi tool)
  - Roadmap e contributi
- Licenza MIT

[1.0.0]: https://github.com/k0m1d3v/SwissKnife/releases/tag/v1.0.0
