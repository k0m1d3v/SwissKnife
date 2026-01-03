using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using WpfUserControl = System.Windows.Controls.UserControl;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;
using SwissKnife.Core;
using SwissKnife.Tools;
using SwissKnife.UI.ViewModels;

namespace SwissKnife.UI.Views;

public partial class PdfView : WpfUserControl
{
    private readonly PdfMergeTool _mergeTool = new();
    private readonly PdfSplitTool _splitTool = new();
    private readonly PdfCompressTool _compressTool = new();
    private System.Threading.CancellationTokenSource? _mergeCancellationTokenSource;
    private System.Threading.CancellationTokenSource? _splitCancellationTokenSource;
    private System.Threading.CancellationTokenSource? _compressCancellationTokenSource;
    private readonly List<string> _mergeFiles = new();

    public PdfView()
    {
        InitializeComponent();
    }

    // ===== MERGE METHODS =====

    private void AddMergeFilesButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new WpfOpenFileDialog
        {
            CheckFileExists = true,
            Multiselect = true,
            Title = "Seleziona file PDF da unire",
            Filter = "PDF Files (*.pdf)|*.pdf"
        };

        if (dialog.ShowDialog(GetParentWindow()) == true)
        {
            foreach (var file in dialog.FileNames)
            {
                if (!_mergeFiles.Contains(file))
                {
                    _mergeFiles.Add(file);
                }
            }

            UpdateMergeFilesList();
            AppendLog($"Aggiunti {dialog.FileNames.Length} file alla lista merge");
            SetStatus($"{_mergeFiles.Count} file pronti per il merge");
        }
    }

    private void ClearMergeFilesButton_Click(object sender, RoutedEventArgs e)
    {
        _mergeFiles.Clear();
        UpdateMergeFilesList();
        AppendLog("Lista file merge cancellata");
        SetStatus("Lista merge vuota");
    }

    private void MoveUpButton_Click(object sender, RoutedEventArgs e)
    {
        if (MergeFilesListBox.SelectedIndex > 0)
        {
            int index = MergeFilesListBox.SelectedIndex;
            var item = _mergeFiles[index];
            _mergeFiles.RemoveAt(index);
            _mergeFiles.Insert(index - 1, item);
            UpdateMergeFilesList();
            MergeFilesListBox.SelectedIndex = index - 1;
        }
    }

    private void MoveDownButton_Click(object sender, RoutedEventArgs e)
    {
        if (MergeFilesListBox.SelectedIndex >= 0 && MergeFilesListBox.SelectedIndex < _mergeFiles.Count - 1)
        {
            int index = MergeFilesListBox.SelectedIndex;
            var item = _mergeFiles[index];
            _mergeFiles.RemoveAt(index);
            _mergeFiles.Insert(index + 1, item);
            UpdateMergeFilesList();
            MergeFilesListBox.SelectedIndex = index + 1;
        }
    }

    private void RemoveMergeFileButton_Click(object sender, RoutedEventArgs e)
    {
        if (MergeFilesListBox.SelectedIndex >= 0)
        {
            int index = MergeFilesListBox.SelectedIndex;
            _mergeFiles.RemoveAt(index);
            UpdateMergeFilesList();
            AppendLog("File rimosso dalla lista");
        }
    }

    private async void MergePdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (_mergeCancellationTokenSource != null)
        {
            return;
        }

        if (_mergeFiles.Count < 2)
        {
            AppendLog("Errore: Seleziona almeno 2 file PDF");
            SetStatus("Errore: minimo 2 file richiesti");
            return;
        }

        var saveDialog = new WpfSaveFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf",
            Title = "Salva PDF unito",
            FileName = "merged.pdf"
        };

        if (saveDialog.ShowDialog(GetParentWindow()) != true)
        {
            return;
        }

        SetStatus("Merge in corso...");
        SetMergeRunningState(true);

        _mergeCancellationTokenSource = new System.Threading.CancellationTokenSource();

        var progress = new Progress<ToolProgress>(p =>
        {
            if (p.Percentage.HasValue)
            {
                MergeProgressBar.IsIndeterminate = false;
                MergeProgressBar.Value = p.Percentage.Value;
                MergeProgressBar.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(p.Message))
            {
                SetStatus(p.Message);
            }
        });

        var context = new ToolContext
        {
            InputFilePath = string.Join(";", _mergeFiles),
            OutputFilePath = saveDialog.FileName,
            CancellationToken = _mergeCancellationTokenSource.Token,
            Logger = AppendLog,
            Progress = progress
        };

        try
        {
            var result = await _mergeTool.RunAsync(context);

            if (result.IsSuccess)
            {
                AppendLog($"? Merge completato: {Path.GetFileName(saveDialog.FileName)}");
                SetStatus("Merge completato con successo");
            }
            else
            {
                AppendLog($"? Errore merge: {result.Error}");
                SetStatus("Merge fallito");
            }
        }
        catch (OperationCanceledException)
        {
            AppendLog("Merge annullato dall'utente");
            SetStatus("Merge annullato");
        }
        finally
        {
            MergeProgressBar.Visibility = Visibility.Collapsed;
            SetMergeRunningState(false);
            _mergeCancellationTokenSource?.Dispose();
            _mergeCancellationTokenSource = null;
        }
    }

    private void MergeCancelButton_Click(object sender, RoutedEventArgs e)
    {
        _mergeCancellationTokenSource?.Cancel();
    }

    // ===== SPLIT METHODS =====

    private void BrowseSplitFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new WpfOpenFileDialog
        {
            CheckFileExists = true,
            Multiselect = false,
            Title = "Seleziona PDF da dividere",
            Filter = "PDF Files (*.pdf)|*.pdf"
        };

        if (dialog.ShowDialog(GetParentWindow()) == true)
        {
            SplitFilePathTextBox.Text = dialog.FileName;
            AppendLog($"File selezionato: {dialog.FileName}");
            SetStatus("File PDF selezionato per split");
        }
    }

    private void BrowseSplitOutputButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Seleziona cartella di destinazione",
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SplitOutputPathTextBox.Text = dialog.SelectedPath;
            AppendLog($"Cartella output: {dialog.SelectedPath}");
        }
    }

    private async void SplitPdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (_splitCancellationTokenSource != null)
        {
            return;
        }

        string inputFile = SplitFilePathTextBox.Text.Trim();
        string outputFolder = SplitOutputPathTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(inputFile))
        {
            AppendLog("Errore: Seleziona un file PDF");
            SetStatus("File non selezionato");
            return;
        }

        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            AppendLog("Errore: Seleziona una cartella di output");
            SetStatus("Cartella output non selezionata");
            return;
        }

        SetStatus("Split in corso...");
        SetSplitRunningState(true);

        _splitCancellationTokenSource = new System.Threading.CancellationTokenSource();

        var progress = new Progress<ToolProgress>(p =>
        {
            if (p.Percentage.HasValue)
            {
                SplitProgressBar.IsIndeterminate = false;
                SplitProgressBar.Value = p.Percentage.Value;
                SplitProgressBar.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(p.Message))
            {
                SetStatus(p.Message);
            }
        });

        // Get split mode
        string mode = SplitByPagesRadio.IsChecked == true ? "pages" : "range";
        string pagesPerFile = PagesPerFileTextBox.Text.Trim();
        string range = SplitRangeTextBox.Text.Trim();

        var parameters = new Dictionary<string, object>
        {
            ["mode"] = mode,
            ["pagesPerFile"] = string.IsNullOrWhiteSpace(pagesPerFile) ? "1" : pagesPerFile,
            ["range"] = range
        };

        var context = new ToolContext
        {
            InputFilePath = inputFile,
            OutputFilePath = outputFolder,
            CancellationToken = _splitCancellationTokenSource.Token,
            Logger = AppendLog,
            Progress = progress,
            Parameters = parameters
        };

        try
        {
            var result = await _splitTool.RunAsync(context);

            if (result.IsSuccess)
            {
                AppendLog($"? Split completato nella cartella: {outputFolder}");
                SetStatus("Split completato con successo");
            }
            else
            {
                AppendLog($"? Errore split: {result.Error}");
                SetStatus("Split fallito");
            }
        }
        catch (OperationCanceledException)
        {
            AppendLog("Split annullato dall'utente");
            SetStatus("Split annullato");
        }
        finally
        {
            SplitProgressBar.Visibility = Visibility.Collapsed;
            SetSplitRunningState(false);
            _splitCancellationTokenSource?.Dispose();
            _splitCancellationTokenSource = null;
        }
    }

    private void SplitCancelButton_Click(object sender, RoutedEventArgs e)
    {
        _splitCancellationTokenSource?.Cancel();
    }

    // ===== COMPRESS METHODS =====

    private void BrowseCompressFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new WpfOpenFileDialog
        {
            CheckFileExists = true,
            Multiselect = false,
            Title = "Seleziona PDF da comprimere",
            Filter = "PDF Files (*.pdf)|*.pdf"
        };

        if (dialog.ShowDialog(GetParentWindow()) == true)
        {
            CompressFilePathTextBox.Text = dialog.FileName;
            AppendLog($"File selezionato: {dialog.FileName}");
            SetStatus("File PDF selezionato per compressione");
        }
    }

    private async void CompressPdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (_compressCancellationTokenSource != null)
        {
            return;
        }

        string inputFile = CompressFilePathTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(inputFile))
        {
            AppendLog("Errore: Seleziona un file PDF");
            SetStatus("File non selezionato");
            return;
        }

        var saveDialog = new WpfSaveFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf",
            Title = "Salva PDF compresso",
            FileName = Path.GetFileNameWithoutExtension(inputFile) + "_compressed.pdf",
            InitialDirectory = Path.GetDirectoryName(inputFile)
        };

        if (saveDialog.ShowDialog(GetParentWindow()) != true)
        {
            return;
        }

        CompressResultBorder.Visibility = Visibility.Collapsed;
        SetStatus("Compressione in corso...");
        SetCompressRunningState(true);

        _compressCancellationTokenSource = new System.Threading.CancellationTokenSource();

        var progress = new Progress<ToolProgress>(p =>
        {
            if (p.Percentage.HasValue)
            {
                CompressProgressBar.IsIndeterminate = false;
                CompressProgressBar.Value = p.Percentage.Value;
                CompressProgressBar.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(p.Message))
            {
                SetStatus(p.Message);
            }
        });

        // Get compression level
        string compressionLevel = "Medium";
        if (CompressLowRadio.IsChecked == true) compressionLevel = "Low";
        else if (CompressHighRadio.IsChecked == true) compressionLevel = "High";

        var parameters = new Dictionary<string, object>
        {
            ["compressionLevel"] = compressionLevel
        };

        var context = new ToolContext
        {
            InputFilePath = inputFile,
            OutputFilePath = saveDialog.FileName,
            CancellationToken = _compressCancellationTokenSource.Token,
            Logger = AppendLog,
            Progress = progress,
            Parameters = parameters
        };

        try
        {
            var result = await _compressTool.RunAsync(context);

            if (result.IsSuccess)
            {
                AppendLog($"? Compressione completata: {Path.GetFileName(saveDialog.FileName)}");
                SetStatus("Compressione completata con successo");
                ShowCompressResult(inputFile, saveDialog.FileName);
            }
            else
            {
                AppendLog($"? Errore compressione: {result.Error}");
                SetStatus("Compressione fallita");
            }
        }
        catch (OperationCanceledException)
        {
            AppendLog("Compressione annullata dall'utente");
            SetStatus("Compressione annullata");
        }
        finally
        {
            CompressProgressBar.Visibility = Visibility.Collapsed;
            SetCompressRunningState(false);
            _compressCancellationTokenSource?.Dispose();
            _compressCancellationTokenSource = null;
        }
    }

    private void CompressCancelButton_Click(object sender, RoutedEventArgs e)
    {
        _compressCancellationTokenSource?.Cancel();
    }

    private void ShowCompressResult(string originalPath, string compressedPath)
    {
        var originalInfo = new FileInfo(originalPath);
        var compressedInfo = new FileInfo(compressedPath);

        long originalSize = originalInfo.Length;
        long compressedSize = compressedInfo.Length;
        double reductionPercent = ((double)(originalSize - compressedSize) / originalSize) * 100.0;

        CompressResultBorder.Visibility = Visibility.Visible;

        if (compressedSize < originalSize)
        {
            CompressResultTextBlock.Text = $"? Compressione riuscita: {reductionPercent:F1}% ridotto";
            CompressDetailTextBlock.Text = $"Originale: {FormatFileSize(originalSize)}\n" +
                                          $"Compresso: {FormatFileSize(compressedSize)}\n" +
                                          $"Risparmiati: {FormatFileSize(originalSize - compressedSize)}";
        }
        else
        {
            CompressResultTextBlock.Text = "! Nessuna riduzione";
            CompressDetailTextBlock.Text = $"Il file compresso ({FormatFileSize(compressedSize)}) non e piu piccolo dell'originale ({FormatFileSize(originalSize)}).\n" +
                                          "Il PDF potrebbe essere gia ottimizzato.";
        }
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    // ===== HELPER METHODS =====

    private void UpdateMergeFilesList()
    {
        MergeFilesListBox.ItemsSource = null;
        MergeFilesListBox.ItemsSource = _mergeFiles.Select(f => Path.GetFileName(f)).ToList();
    }

    private void SetMergeRunningState(bool isRunning)
    {
        AddMergeFilesButton.IsEnabled = !isRunning;
        ClearMergeFilesButton.IsEnabled = !isRunning;
        MergePdfButton.IsEnabled = !isRunning;
        MergeCancelButton.IsEnabled = isRunning;
        MergeProgressBar.IsIndeterminate = true;
        MergeProgressBar.Visibility = isRunning ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetSplitRunningState(bool isRunning)
    {
        BrowseSplitFileButton.IsEnabled = !isRunning;
        BrowseSplitOutputButton.IsEnabled = !isRunning;
        SplitPdfButton.IsEnabled = !isRunning;
        SplitCancelButton.IsEnabled = isRunning;
        SplitProgressBar.IsIndeterminate = true;
        SplitProgressBar.Visibility = isRunning ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetCompressRunningState(bool isRunning)
    {
        BrowseCompressFileButton.IsEnabled = !isRunning;
        CompressPdfButton.IsEnabled = !isRunning;
        CompressCancelButton.IsEnabled = isRunning;
        CompressProgressBar.IsIndeterminate = true;
        CompressProgressBar.Visibility = isRunning ? Visibility.Visible : Visibility.Collapsed;
        CompressLowRadio.IsEnabled = !isRunning;
        CompressMediumRadio.IsEnabled = !isRunning;
        CompressHighRadio.IsEnabled = !isRunning;
    }

    private void AppendLog(string message)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => AppendLog(message));
            return;
        }

        var builder = new StringBuilder();
        builder.Append('[')
               .Append(DateTime.Now.ToString("HH:mm:ss"))
               .Append("] ")
               .AppendLine(message);

        LogTextBox.AppendText(builder.ToString());
        LogTextBox.ScrollToEnd();
    }

    private void SetStatus(string message)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.StatusText = message;
        }
    }

    private Window? GetParentWindow() => Window.GetWindow(this);
}
