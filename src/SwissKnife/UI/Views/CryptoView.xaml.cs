using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SwissKnife.Core;
using SwissKnife.Tools;
using SwissKnife.UI.ViewModels;

namespace SwissKnife.UI.Views;

public partial class CryptoView : UserControl
{
    private readonly HashTool _hashTool = new();
    private CancellationTokenSource? _cancellationTokenSource;

    public CryptoView()
    {
        InitializeComponent();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Multiselect = false,
            Title = "Seleziona un file",
        };

        if (dialog.ShowDialog(GetParentWindow()) == true)
        {
            FilePathTextBox.Text = dialog.FileName;
            AppendLog($"Selezionato: {dialog.FileName}");
            SetStatus("File selezionato");
        }
    }

    private async void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_cancellationTokenSource != null)
        {
            return;
        }

        string filePath = FilePathTextBox.Text.Trim();
        ClearResult();
        SetStatus("Elaborazione in corso...");
        SetRunningState(true, isIndeterminate: true);

        _cancellationTokenSource = new CancellationTokenSource();

        var progress = new Progress<ToolProgress>(p =>
        {
            if (p.Percentage.HasValue)
            {
                OperationProgressBar.IsIndeterminate = false;
                OperationProgressBar.Value = p.Percentage.Value;
                OperationProgressBar.Visibility = Visibility.Visible;
            }
            else
            {
                OperationProgressBar.IsIndeterminate = true;
                OperationProgressBar.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(p.Message))
            {
                SetStatus(p.Message);
            }
        });

        var context = new ToolContext
        {
            InputFilePath = filePath,
            CancellationToken = _cancellationTokenSource.Token,
            Logger = AppendLog,
            Progress = progress,
        };

        try
        {
            var result = await _hashTool.RunAsync(context);

            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Output))
            {
                HashOutputTextBox.Text = result.Output;
                CopyButton.IsEnabled = true;
                SetStatus("Hash calcolato");
            }
            else
            {
                AppendLog(result.Error ?? "Errore sconosciuto");
                SetStatus("Elaborazione fallita");
            }
        }
        catch (OperationCanceledException)
        {
            AppendLog("Operazione annullata dall'utente.");
            SetStatus("Annullato");
        }
        finally
        {
            OperationProgressBar.Visibility = Visibility.Collapsed;
            SetRunningState(false, isIndeterminate: false);
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(HashOutputTextBox.Text))
        {
            Clipboard.SetText(HashOutputTextBox.Text);
            SetStatus("Hash copiato negli appunti");
        }
    }

    private void SetRunningState(bool isRunning, bool isIndeterminate)
    {
        CalculateButton.IsEnabled = !isRunning;
        CancelButton.IsEnabled = isRunning;
        OperationProgressBar.IsIndeterminate = isIndeterminate;
        OperationProgressBar.Visibility = isRunning ? Visibility.Visible : Visibility.Collapsed;
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

    private void ClearResult()
    {
        HashOutputTextBox.Clear();
        CopyButton.IsEnabled = false;
    }

    private Window? GetParentWindow() => Window.GetWindow(this);
}
