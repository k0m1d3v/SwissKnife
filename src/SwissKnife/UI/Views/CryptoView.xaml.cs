using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WpfUserControl = System.Windows.Controls.UserControl;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;
using WpfClipboard = System.Windows.Clipboard;
using WpfColor = System.Windows.Media.Color;
using SwissKnife.Core;
using SwissKnife.Tools;
using SwissKnife.UI.ViewModels;

namespace SwissKnife.UI.Views;

public partial class CryptoView : WpfUserControl
{
    private readonly HashTool _hashTool = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private CancellationTokenSource? _verifyCancellationTokenSource;

    public CryptoView()
    {
        InitializeComponent();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new WpfOpenFileDialog
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
            WpfClipboard.SetText(HashOutputTextBox.Text);
            SetStatus("Hash copiato negli appunti");
        }
    }

    private void VerifyBrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new WpfOpenFileDialog
        {
            CheckFileExists = true,
            Multiselect = false,
            Title = "Seleziona un file da verificare",
        };

        if (dialog.ShowDialog(GetParentWindow()) == true)
        {
            VerifyFilePathTextBox.Text = dialog.FileName;
            AppendLog($"File da verificare: {dialog.FileName}");
            SetStatus("File selezionato per verifica");
        }
    }

    private async void VerifyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_verifyCancellationTokenSource != null)
        {
            return;
        }

        string filePath = VerifyFilePathTextBox.Text.Trim();
        string expectedHash = ExpectedHashTextBox.Text.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            AppendLog("Errore: Seleziona un file da verificare");
            SetStatus("File non selezionato");
            return;
        }

        if (string.IsNullOrWhiteSpace(expectedHash))
        {
            AppendLog("Errore: Inserisci l'hash da verificare");
            SetStatus("Hash non inserito");
            return;
        }

        ClearVerifyResult();
        SetStatus("Verifica in corso...");
        SetVerifyRunningState(true, isIndeterminate: true);

        _verifyCancellationTokenSource = new CancellationTokenSource();

        var progress = new Progress<ToolProgress>(p =>
        {
            if (p.Percentage.HasValue)
            {
                VerifyProgressBar.IsIndeterminate = false;
                VerifyProgressBar.Value = p.Percentage.Value;
                VerifyProgressBar.Visibility = Visibility.Visible;
            }
            else
            {
                VerifyProgressBar.IsIndeterminate = true;
                VerifyProgressBar.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(p.Message))
            {
                SetStatus(p.Message);
            }
        });

        var context = new ToolContext
        {
            InputFilePath = filePath,
            CancellationToken = _verifyCancellationTokenSource.Token,
            Logger = AppendLog,
            Progress = progress,
        };

        try
        {
            var result = await _hashTool.RunAsync(context);

            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Output))
            {
                string actualHash = result.Output.ToUpperInvariant();
                bool isMatch = actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);

                ShowVerifyResult(isMatch, actualHash, expectedHash);

                if (isMatch)
                {
                    AppendLog("? Hash verificato: CORRISPONDE");
                    SetStatus("Hash verificato: CORRISPONDE");
                }
                else
                {
                    AppendLog("? Hash verificato: NON CORRISPONDE");
                    SetStatus("Hash verificato: NON CORRISPONDE");
                }
            }
            else
            {
                AppendLog(result.Error ?? "Errore durante il calcolo dell'hash");
                SetStatus("Verifica fallita");
            }
        }
        catch (OperationCanceledException)
        {
            AppendLog("Verifica annullata dall'utente.");
            SetStatus("Verifica annullata");
        }
        finally
        {
            VerifyProgressBar.Visibility = Visibility.Collapsed;
            SetVerifyRunningState(false, isIndeterminate: false);
            _verifyCancellationTokenSource?.Dispose();
            _verifyCancellationTokenSource = null;
        }
    }

    private void VerifyCancelButton_Click(object sender, RoutedEventArgs e)
    {
        _verifyCancellationTokenSource?.Cancel();
    }

    private void SetRunningState(bool isRunning, bool isIndeterminate)
    {
        CalculateButton.IsEnabled = !isRunning;
        CancelButton.IsEnabled = isRunning;
        OperationProgressBar.IsIndeterminate = isIndeterminate;
        OperationProgressBar.Visibility = isRunning ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetVerifyRunningState(bool isRunning, bool isIndeterminate)
    {
        VerifyButton.IsEnabled = !isRunning;
        VerifyCancelButton.IsEnabled = isRunning;
        VerifyProgressBar.IsIndeterminate = isIndeterminate;
        VerifyProgressBar.Visibility = isRunning ? Visibility.Visible : Visibility.Collapsed;
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

    private void ClearVerifyResult()
    {
        VerifyResultBorder.Visibility = Visibility.Collapsed;
    }

    private void ShowVerifyResult(bool isMatch, string actualHash, string expectedHash)
    {
        VerifyResultBorder.Visibility = Visibility.Visible;

        if (isMatch)
        {
            VerifyResultTextBlock.Text = "? HASH CORRISPONDE";
            VerifyResultTextBlock.Foreground = new SolidColorBrush(WpfColor.FromRgb(76, 175, 80));
            VerifyDetailTextBlock.Text = $"Il file ha l'hash corretto: {actualHash}";
            VerifyResultBorder.BorderBrush = new SolidColorBrush(WpfColor.FromRgb(76, 175, 80));
        }
        else
        {
            VerifyResultTextBlock.Text = "? HASH NON CORRISPONDE";
            VerifyResultTextBlock.Foreground = new SolidColorBrush(WpfColor.FromRgb(244, 67, 54));
            VerifyDetailTextBlock.Text = $"Atteso: {expectedHash}\nCalcolato: {actualHash}";
            VerifyResultBorder.BorderBrush = new SolidColorBrush(WpfColor.FromRgb(244, 67, 54));
        }
    }

    private Window? GetParentWindow() => Window.GetWindow(this);
}
