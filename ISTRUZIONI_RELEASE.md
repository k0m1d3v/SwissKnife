# ?? Istruzioni per il Rilascio v1.0.0

Congratulazioni! Il tuo progetto SwissKnife è pronto per la prima release ufficiale.

## ? File Preparati

I seguenti file sono stati creati/aggiornati per la release:

1. **CHANGELOG.md** - Documentazione completa delle modifiche
2. **RELEASE_NOTES.md** - Note di rilascio per GitHub
3. **src/SwissKnife/SwissKnife.csproj** - Aggiornato con versione 1.0.0 e metadati
4. **release-v1.0.0.ps1** - Script automatico per il rilascio
5. **build-release.ps1** - Script per creare il pacchetto di distribuzione
6. **.github/workflows/release.yml** - GitHub Action per rilasci automatici (opzionale)

## ?? Procedura di Rilascio

### Opzione A: Rilascio Automatico (Consigliato)

Esegui lo script PowerShell che gestisce tutto automaticamente:

```powershell
.\release-v1.0.0.ps1
```

Lo script eseguirà:
- ? Verifica modifiche non committate
- ? Build del progetto in modalità Release
- ? Creazione del tag v1.0.0
- ? Push su GitHub (con conferma)

### Opzione B: Rilascio Manuale

Se preferisci fare tutto manualmente:

1. **Committa le modifiche**
   ```bash
   git add .
   git commit -m "chore: prepare v1.0.0 release"
   ```

2. **Verifica che compili**
   ```bash
   dotnet build -c Release
   ```

3. **Crea il tag**
   ```bash
   git tag -a v1.0.0 -m "Release v1.0.0 - First stable release"
   ```

4. **Push su GitHub**
   ```bash
   git push origin main
   git push origin v1.0.0
   ```

## ?? Creazione del Pacchetto di Distribuzione

Per creare il file .zip da caricare su GitHub:

```powershell
.\build-release.ps1
```

Lo script creerà:
- Cartella `release-package/` con i file compilati
- File `SwissKnife-v1.0.0.zip` (o `-win-x64.zip` se self-contained)

**Opzioni disponibili:**
- **Framework-dependent**: Richiede .NET 8 Runtime installato (più leggero)
- **Self-contained**: Include .NET Runtime (più grande ma funziona ovunque)

## ?? Creazione della Release su GitHub

1. **Vai su GitHub**
   Apri: https://github.com/k0m1d3v/SwissKnife/releases/new

2. **Seleziona il tag**
   - Scegli il tag `v1.0.0` dal dropdown

3. **Compila i campi**
   - **Titolo**: `SwissKnife v1.0.0 - First Stable Release`
   - **Descrizione**: Copia il contenuto da `RELEASE_NOTES.md`

4. **Carica l'asset**
   - Trascina il file `SwissKnife-v1.0.0.zip` nella sezione "Attach binaries"

5. **Pubblica**
   - Assicurati che "Set as the latest release" sia selezionato
   - Clicca su "Publish release"

## ?? Rilasci Futuri Automatici (Opzionale)

Il file `.github/workflows/release.yml` è già configurato per automatizzare i futuri rilasci.

**Come funziona:**
- Quando crei un nuovo tag (es: `v1.1.0`), GitHub Actions:
  1. Compila il progetto
  2. Crea il pacchetto self-contained
  3. Crea automaticamente la release con l'asset allegato

**Per usarlo:**
```bash
git tag -a v1.1.0 -m "Release v1.1.0"
git push origin v1.1.0
# GitHub Actions farà il resto automaticamente!
```

## ?? Checklist Pre-Release

Prima di pubblicare, verifica che:

- [ ] Il progetto compila senza errori: `dotnet build -c Release`
- [ ] La versione nel .csproj è corretta (1.0.0)
- [ ] CHANGELOG.md è aggiornato
- [ ] README.md è completo e aggiornato
- [ ] LICENSE è presente
- [ ] Tutte le modifiche sono committate
- [ ] I test (se presenti) passano
- [ ] L'applicazione funziona correttamente in modalità Release

## ?? Dopo la Release

Una volta pubblicata la release:

1. **Annuncia la release**
   - Condividi il link sui social
   - Invia newsletter (se presente)

2. **Monitora i feedback**
   - Controlla le issues su GitHub
   - Rispondi alle domande degli utenti

3. **Pianifica la prossima release**
   - Aggiorna la roadmap
   - Raccogli feature requests

## ?? Troubleshooting

### Build fallisce
```bash
dotnet clean
dotnet restore
dotnet build -c Release
```

### Tag già esistente
```bash
git tag -d v1.0.0              # Elimina localmente
git push origin :refs/tags/v1.0.0  # Elimina su GitHub
```

### Modifiche non committate
```bash
git status
git add .
git commit -m "tuo messaggio"
```

## ?? Supporto

Per problemi o domande:
- GitHub Issues: https://github.com/k0m1d3v/SwissKnife/issues
- Documentazione: https://github.com/k0m1d3v/SwissKnife#readme

---

**Buona fortuna con la tua prima release! ??**
