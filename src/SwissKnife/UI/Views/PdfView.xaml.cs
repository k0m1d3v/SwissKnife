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
    private System.Threading.CancellationTokenSource? _mergeCancellationTokenSource;
    private System.Threading.CancellationTokenSource? _splitCancellationTokenSource;
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

    private void AppendLog(string message)
    {
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
