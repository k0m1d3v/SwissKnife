# ?? SwissKnife v1.0.0 - First Stable Release

Prima release ufficiale di **SwissKnife**, una toolbox WPF modulare ed estendibile per Windows con tema dark moderno.

---

## ? Caratteristiche Principali

### ??? Architettura Modulare
- Sistema basato su interfaccia `ITool` per facile estensibilità
- Pattern MVVM con separazione logica/UI
- Operazioni asincrone con progress reporting
- Gestione cancellazione operazioni in corso

### ?? Interfaccia Utente Moderna
- **Tema dark** con accent color viola (#8A7CFF)
- Design responsivo con scrolling automatico
- Sidebar di navigazione con categorie
- Componenti stilizzati personalizzati
- Progress bar e feedback visivo

---

## ??? Strumenti Disponibili

### ?? Crittografia

#### Hash SHA-256
Calcola l'hash SHA-256 di qualsiasi file con:
- Streaming ottimizzato per file di grandi dimensioni
- Progress bar in tempo reale
- Copia hash negli appunti
- Log dettagliato delle operazioni

#### Verifica Hash SHA-256
Verifica l'integrità dei file confrontando gli hash:
- Risultato visivo immediato (? verde / ? rosso)
- Mostra entrambi gli hash per confronto
- Ideale per verificare download e integrità

### ?? Manipolazione PDF

#### Merge PDF (Unisci PDF)
Combina più file PDF in un unico documento:
- Selezione multipla di file
- Riordino con pulsanti Up/Down
- Rimozione file dalla lista
- Progress bar durante l'unione

#### Split PDF (Dividi PDF)
Divide un PDF in più file con due modalità:
- **Per pagine**: Dividi ogni N pagine (1, 2, 3, etc.)
- **Range personalizzati**: Specifica range esatti (es: "1-3,5-7,10")
- Nomi file automatici con indicatori pagine
- Progress bar per pagina

#### Compress PDF (Comprimi PDF)
Ottimizza le dimensioni dei file PDF:
- **3 livelli di compressione**: Bassa, Media, Alta
- Full compression mode con iText7
- Statistiche dettagliate (dimensione originale/compressa, % riduzione)
- Avviso per file già ottimizzati

---

## ?? Requisiti di Sistema

- **Sistema Operativo**: Windows 10/11
- **.NET**: .NET 8.0 Runtime o superiore
- **Spazio su disco**: ~50 MB

---

## ?? Installazione

### Opzione 1: Eseguibile Standalone (Consigliato)
1. Scarica `SwissKnife-v1.0.0.zip` dagli assets
2. Estrai in una cartella a tua scelta
3. Esegui `SwissKnife.exe`

### Opzione 2: Build da Sorgente
```bash
git clone https://github.com/k0m1d3v/SwissKnife.git
cd SwissKnife
dotnet build -c Release
# Eseguibile in: src\SwissKnife\bin\Release\net8.0-windows\
```

---

## ??? Tecnologie Utilizzate

- **.NET 8.0** - Framework applicativo
- **WPF** - Interfaccia grafica
- **iText7 9.4.0** - Manipolazione PDF
- **Pattern MVVM** - Architettura UI
- **Async/Await** - Operazioni asincrone

---

## ?? Documentazione

- [README completo](https://github.com/k0m1d3v/SwissKnife/blob/main/README.md) con guida all'uso e sviluppo
- [CHANGELOG](https://github.com/k0m1d3v/SwissKnife/blob/main/CHANGELOG.md) con dettaglio modifiche

---

## ??? Roadmap

Funzionalità pianificate per versioni future:
- [ ] Strumenti di conversione (immagini, documenti)
- [ ] Utility varie (generatori, validator)
- [ ] Impostazioni persistenti
- [ ] PDF: Estrazione testo/immagini
- [ ] PDF: Protezione con password
- [ ] PDF: Rotazione pagine

---

## ?? Contributi

I contributi sono benvenuti! Vedi [CONTRIBUTING](https://github.com/k0m1d3v/SwissKnife#-contribuire) per maggiori informazioni.

---

## ?? Licenza

Questo progetto è distribuito sotto licenza [MIT](https://github.com/k0m1d3v/SwissKnife/blob/main/LICENSE).

---

## ?? Segnalazione Bug

Hai trovato un bug? [Apri una issue](https://github.com/k0m1d3v/SwissKnife/issues/new)!

---

**Grazie per aver scelto SwissKnife!** ??

Se trovi utile questo progetto, considera di lasciare una ? su GitHub!
