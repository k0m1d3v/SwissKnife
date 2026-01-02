# SwissKnife

MVP di una toolbox WPF modulare (.NET 8) pensata per essere portabile e facilmente estendibile.

## Struttura
- `SwissKnife.sln` – soluzione Visual Studio.
- `src/SwissKnife/` – progetto WPF:
  - `Core/` – interfacce e infrastruttura (ITool, ToolContext, ToolRegistry).
  - `Tools/` – implementazioni dei tool (HashTool per SHA-256).
  - `UI/` – XAML e code-behind della finestra principale.

## Requisiti di build
- .NET 8 SDK su Windows.

## Esecuzione
1. Ripristina e compila: `dotnet restore` quindi `dotnet build`.
2. Avvia: `dotnet run --project src/SwissKnife/SwissKnife.csproj`.

L'app consente di selezionare un file, calcolare l'hash SHA-256 in streaming e copiare il risultato.
