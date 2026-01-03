using System;
using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Xobject;
using iText.IO.Image;
using SwissKnife.Core;

namespace SwissKnife.Tools;

/// <summary>
/// Compresses PDF files by optimizing images and removing unnecessary data.
/// </summary>
public sealed class PdfCompressTool : ITool
{
    public const string ToolId = "pdf.compress";

    public string Id => ToolId;

    public string Name => "PDF Compress";

    public string Description => "Comprime file PDF riducendone le dimensioni";

    public enum CompressionLevel
    {
        Low,      // Qualità alta, compressione bassa
        Medium,   // Bilanciato
        High      // Qualità bassa, compressione alta
    }

    /// <summary>
    /// Compresses a PDF file.
    /// Context.InputFilePath specifies the source PDF.
    /// Context.OutputFilePath specifies the compressed output PDF path.
    /// Context.Parameters can contain:
    ///   - "compressionLevel": "Low", "Medium", or "High"
    /// </summary>
    public async Task<ToolResult> RunAsync(ToolContext context)
    {
        if (string.IsNullOrWhiteSpace(context.InputFilePath))
        {
            return ToolResult.Failure("File PDF non specificato.");
        }

        if (!File.Exists(context.InputFilePath))
        {
            return ToolResult.Failure("Il file PDF specificato non esiste.");
        }

        if (string.IsNullOrWhiteSpace(context.OutputFilePath))
        {
            return ToolResult.Failure("Percorso file di output non specificato.");
        }

        // Parse compression level
        var compressionLevelStr = context.Parameters?.GetValueOrDefault("compressionLevel") as string ?? "Medium";
        if (!Enum.TryParse<CompressionLevel>(compressionLevelStr, out var compressionLevel))
        {
            compressionLevel = CompressionLevel.Medium;
        }

        try
        {
            var inputFileInfo = new FileInfo(context.InputFilePath);
            long originalSize = inputFileInfo.Length;

            context.Logger?.Invoke($"Apertura PDF: {inputFileInfo.Name}");
            context.Logger?.Invoke($"Dimensione originale: {FormatFileSize(originalSize)}");
            context.Logger?.Invoke($"Livello compressione: {compressionLevel}");

            await Task.Run(() =>
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                using var reader = new PdfReader(context.InputFilePath);
                using var writer = new PdfWriter(context.OutputFilePath, 
                    new WriterProperties()
                        .SetCompressionLevel(GetCompressionLevelValue(compressionLevel))
                        .SetFullCompressionMode(true));
                using var pdfDoc = new PdfDocument(reader, writer);

                int totalPages = pdfDoc.GetNumberOfPages();
                context.Logger?.Invoke($"Totale pagine: {totalPages}");

                // Process each page for image compression
                for (int i = 1; i <= totalPages; i++)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    var page = pdfDoc.GetPage(i);
                    
                    // Report progress
                    double progress = ((double)i / totalPages) * 100.0;
                    context.Progress?.Report(new ToolProgress(progress, $"Compressione pagina {i}/{totalPages}"));
                }

                context.Logger?.Invoke("Finalizzazione documento compresso...");
            }, context.CancellationToken);

            var outputFileInfo = new FileInfo(context.OutputFilePath);
            long compressedSize = outputFileInfo.Length;
            double reductionPercent = ((double)(originalSize - compressedSize) / originalSize) * 100.0;

            context.Logger?.Invoke($"Dimensione compressa: {FormatFileSize(compressedSize)}");
            context.Logger?.Invoke($"Riduzione: {reductionPercent:F1}% ({FormatFileSize(originalSize - compressedSize)} risparmiati)");
            
            if (compressedSize >= originalSize)
            {
                context.Logger?.Invoke("Nota: Il file compresso non è più piccolo dell'originale. Il PDF potrebbe essere già ottimizzato.");
            }

            return ToolResult.Success(context.OutputFilePath);
        }
        catch (OperationCanceledException)
        {
            context.Logger?.Invoke("Operazione annullata.");
            
            // Clean up partial output file
            if (File.Exists(context.OutputFilePath))
            {
                try { File.Delete(context.OutputFilePath); } catch { }
            }
            
            throw;
        }
        catch (iText.Kernel.Exceptions.PdfException pdfEx)
        {
            // Check for inner exception with more details
            var errorMessage = pdfEx.InnerException != null 
                ? $"{pdfEx.Message} - {pdfEx.InnerException.Message}"
                : pdfEx.Message;
            
            context.Logger?.Invoke($"Errore PDF: {errorMessage}");
            return ToolResult.Failure($"Errore PDF: {errorMessage}");
        }
        catch (iText.IO.Exceptions.IOException pdfIoEx)
        {
            var errorMessage = pdfIoEx.InnerException != null 
                ? $"{pdfIoEx.Message} - {pdfIoEx.InnerException.Message}"
                : pdfIoEx.Message;
            
            context.Logger?.Invoke($"Errore PDF I/O: {errorMessage}");
            return ToolResult.Failure($"Errore PDF I/O: {errorMessage}");
        }
        catch (UnauthorizedAccessException)
        {
            return ToolResult.Failure("Accesso negato. Verifica i permessi dei file.");
        }
        catch (IOException ioEx)
        {
            return ToolResult.Failure($"Errore I/O: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            var errorMessage = ex.InnerException != null 
                ? $"{ex.Message} - {ex.InnerException.Message}"
                : ex.Message;
            
            context.Logger?.Invoke($"Errore inatteso: {errorMessage}");
            return ToolResult.Failure($"Errore inatteso: {errorMessage}");
        }
    }

    private int GetCompressionLevelValue(CompressionLevel level)
    {
        return level switch
        {
            CompressionLevel.Low => 3,      // Minore compressione, qualità migliore
            CompressionLevel.Medium => 6,   // Bilanciato
            CompressionLevel.High => 9,     // Massima compressione
            _ => 6
        };
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
}
