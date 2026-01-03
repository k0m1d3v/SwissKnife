# SwissKnife

MVP di una toolbox WPF modulare (.NET 8) pensata per essere portabile e facilmente estendibile. Un'applicazione multi-tool con interfaccia moderna e tema scuro personalizzato.

## 🎯 Caratteristiche

- **Architettura modulare**: facilmente estendibile con nuovi tool
- **UI moderna**: tema scuro con accent viola (#8A7CFF)
- **Design responsivo**: scrolling automatico per finestre ridimensionate
- **Operazioni asincrone**: con progress bar e cancellazione

## 📁 Struttura del Progetto

```
SwissKnife/
├── SwissKnife.sln              # Soluzione Visual Studio
└── src/SwissKnife/             # Progetto WPF principale
    ├── Core/                   # Interfacce e infrastruttura
    │   ├── ITool.cs            # Interfaccia base per i tool
    │   ├── ToolContext.cs      # Contesto di esecuzione
    │   ├── ToolProgress.cs     # Report di avanzamento
    │   └── ToolResult.cs       # Risultato dell'esecuzione
    ├── Tools/                  # Implementazioni dei tool
    │   └── HashTool.cs         # Calcolo hash SHA-256
    ├── UI/                     # Interfaccia utente
    │   ├── ViewModels/         # ViewModel MVVM
    │   ├── Views/              # View XAML
    │   │   └── CryptoView.xaml # Vista strumenti crittografici
    │   ├── Models/             # Modelli dati
    │   └── Helpers/            # Utility e selector
    └── App.xaml                # Risorse e stili globali
```

## 🛠️ Tool Disponibili

### 🔐 Crypto (Crittografia)

#### Hash SHA-256
- **Calcolo hash**: Genera l'hash SHA-256 di qualsiasi file
  - Streaming con buffer ottimizzato (81920 byte)
  - Progress bar in tempo reale
  - Supporto per file di grandi dimensioni
  - Cancellazione operazione
  - Log dettagliato delle operazioni
  - Copia hash negli appunti

#### Verifica Hash SHA-256
- **Verifica integrità**: Confronta un hash SHA-256 con un file
  - Selezione file da verificare
  - Input hash atteso
  - Risultato visivo chiaro (✓ verde / ✗ rosso)
  - Mostra entrambi gli hash per confronto
  - Utile per verificare download e integrità file

### 📄 PDF (Manipolazione PDF)

#### Merge PDF (Unisci PDF)
- **Unione documenti**: Combina più file PDF in un unico documento
  - Selezione multipla di file
  - Riordino file con pulsanti Up/Down
  - Rimozione file dalla lista
  - Mantiene l'ordine selezionato
  - Progress bar durante l'unione
  - Cancellazione operazione
  - Log operazioni dettagliato

#### Split PDF (Dividi PDF)
- **Divisione documenti**: Divide un PDF in più file
  - **Modalità per pagine**: Dividi ogni N pagine
    - Es: 1 pagina per file, 3 pagine per file, etc.
  - **Modalità range personalizzati**: Specifica range di pagine
    - Es: "1-3,5-7,10" crea 3 file (pagine 1-3, 5-7, pagina 10)
  - Selezione cartella output
  - Nomi file automatici con indicatore pagine
  - Progress bar durante la divisione
  - Cancellazione operazione

#### Compress PDF (Comprimi PDF)
- **Ottimizzazione dimensioni**: Riduce le dimensioni dei file PDF
  - **3 livelli di compressione**:
    - **Bassa**: Qualità alta, compressione minima
    - **Media**: Bilanciato (predefinito)
    - **Alta**: Qualità ridotta, compressione massima
  - Ottimizzazione struttura PDF
  - Full compression mode iText7
  - **Statistiche dettagliate**:
    - Dimensione originale e compressa
    - Percentuale di riduzione
    - Spazio risparmiato
  - Progress bar per pagina
  - Cancellazione operazione
  - Avviso se il file è già ottimizzato

### 🔄 Convert (In sviluppo)
Strumenti di conversione file

### 🔧 Utility (In sviluppo)
Strumenti di utilità vari

### ⚙️ Settings (In sviluppo)
Impostazioni applicazione

## 🎨 Design UI

### Tema
- **Background primario**: `#151515`
- **Background secondario**: `#1E1E1E`
- **Accent color**: `#8A7CFF` (viola)
- **Testo primario**: `#F5F5F5`
- **Testo secondario**: `#B3B3B3`
- **Bordi**: `#2A2A2A`

### Componenti Stilizzati
- **Card**: Container con ombra e bordi arrotondati
- **Button**: Stili Primary (accent) e Ghost (outline)
- **TextBox**: Bordo arrotondato con focus accent
- **ProgressBar**: Design minimale (8px altezza)
- **ScrollBar**: Discreta (8px) con tema dark
- **Sidebar**: Navigazione con indicatore accent

## 🚀 Requisiti

- **.NET 8 SDK** o superiore
- **Windows** (WPF è Windows-only)
- **Visual Studio 2022** (consigliato) o VS Code con estensioni C#

## 📦 Installazione e Build

### 1. Clona il repository
```bash
git clone https://github.com/k0m1d3v/SwissKnife.git
cd SwissKnife
```

### 2. Ripristina le dipendenze
```bash
dotnet restore
```

### 3. Compila il progetto
```bash
dotnet build
```

### 4. Esegui l'applicazione
```bash
dotnet run --project src/SwissKnife/SwissKnife.csproj
```

Oppure apri `SwissKnife.sln` in Visual Studio e premi F5.

## 🔧 Sviluppo

### Aggiungere un nuovo tool

1. **Crea la classe del tool** in `src/SwissKnife/Tools/`:
```csharp
public sealed class MyTool : ITool
{
    public string Id => "mytool.id";
    public string Name => "My Tool";
    public string Description => "Tool description";
    
    public async Task<ToolResult> RunAsync(ToolContext context)
    {
        // Implementazione
        return ToolResult.Success("output");
    }
}
```

2. **Crea la View** in `src/SwissKnife/UI/Views/`:
```xaml
<UserControl x:Class="SwissKnife.UI.Views.MyToolView">
    <!-- UI del tool -->
</UserControl>
```

3. **Registra il DataTemplate** in `App.xaml`:
```xaml
<DataTemplate x:Key="MyToolTemplate">
    <views:MyToolView />
</DataTemplate>
```

4. **Aggiungi alla categoria** in `MainViewModel.cs`

### Struttura ITool

Ogni tool implementa `ITool` con:
- **Id**: Identificatore unico
- **Name**: Nome visualizzato
- **Description**: Descrizione funzionalità
- **RunAsync**: Metodo di esecuzione asincrona

### ToolContext

Fornisce al tool:
- `InputFilePath`: Percorso file input
- `OutputFilePath`: Percorso file output (opzionale)
- `CancellationToken`: Cancellazione operazione
- `Logger`: Callback per logging
- `Progress`: Report avanzamento (IProgress<ToolProgress>)

## 📝 Roadmap

- [x] Architettura modulare base
- [x] Tema dark UI
- [x] Tool Hash SHA-256 (calcolo)
- [x] Tool Hash SHA-256 (verifica)
- [x] Scrolling UI responsivo
- [x] Progress bar e cancellazione
- [x] Tool PDF Merge (unisci PDF)
- [x] Tool PDF Split (dividi PDF)
- [x] Tool PDF Compress (comprimi PDF)
- [ ] Tool Convert (immagini, documenti)
- [ ] Tool Utility (vari)
- [ ] Impostazioni persistenti
- [x] Supporto temi multipli
- [ ] Export/Import configurazioni
- [ ] PDF: Estrazione testo/immagini
- [ ] PDF: Protezione con password
- [ ] PDF: Rotazione pagine

## 🤝 Contribuire

Contributi benvenuti! Per contribuire:

1. Fork il progetto
2. Crea un branch per la feature (`git checkout -b feature/AmazingFeature`)
3. Commit le modifiche (`git commit -m 'Add some AmazingFeature'`)
4. Push al branch (`git push origin feature/AmazingFeature`)
5. Apri una Pull Request

## 📄 Licenza

Questo progetto è distribuito sotto licenza MIT. Vedi il file `LICENSE` per i dettagli.

## 🐛 Bug e Suggerimenti

Usa la sezione [Issues](https://github.com/k0m1d3v/SwissKnife/issues) di GitHub per:
- Segnalare bug
- Richiedere nuove funzionalità
- Discutere miglioramenti

## 👥 Autori

- **k0m1d3v** - *Sviluppo iniziale*

## 🙏 Ringraziamenti

- .NET Team per l'eccellente framework
- Community WPF per guide e risorse
- Contributors che migliorano il progetto
