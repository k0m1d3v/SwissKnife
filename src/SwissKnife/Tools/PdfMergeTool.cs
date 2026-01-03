using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using SwissKnife.Core;

namespace SwissKnife.Tools;

/// <summary>
/// Merges multiple PDF files into a single PDF document.
/// </summary>
public sealed class PdfMergeTool : ITool
{
    public const string ToolId = "pdf.merge";

    public string Id => ToolId;

    public string Name => "PDF Merge";

    public string Description => "Unisce più file PDF in un unico documento";

    /// <summary>
    /// Merges multiple PDF files.
    /// Context.InputFilePath should contain semicolon-separated file paths.
    /// Context.OutputFilePath specifies the output merged PDF path.
    /// </summary>
    public async Task<ToolResult> RunAsync(ToolContext context)
    {
        if (string.IsNullOrWhiteSpace(context.InputFilePath))
        {
            return ToolResult.Failure("Nessun file PDF specificato per il merge.");
        }

        if (string.IsNullOrWhiteSpace(context.OutputFilePath))
        {
            return ToolResult.Failure("Percorso file di output non specificato.");
        }

        // Parse input paths (semicolon-separated)
        var inputPaths = context.InputFilePath
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToList();

        if (inputPaths.Count < 2)
        {
            return ToolResult.Failure("Sono necessari almeno 2 file PDF per eseguire il merge.");
        }

        // Validate all input files exist
        foreach (var path in inputPaths)
        {
            if (!File.Exists(path))
            {
                return ToolResult.Failure($"File non trovato: {path}");
            }
        }

        try
        {
            context.Logger?.Invoke($"Inizio merge di {inputPaths.Count} file PDF...");

            await Task.Run(() =>
            {
                using var outputPdf = new PdfDocument(new PdfWriter(context.OutputFilePath));
                var merger = new PdfMerger(outputPdf);

                for (int i = 0; i < inputPaths.Count; i++)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    var inputPath = inputPaths[i];
                    context.Logger?.Invoke($"Unione file {i + 1}/{inputPaths.Count}: {Path.GetFileName(inputPath)}");

                    using var sourcePdf = new PdfDocument(new PdfReader(inputPath));
                    int pageCount = sourcePdf.GetNumberOfPages();
                    merger.Merge(sourcePdf, 1, pageCount);

                    double progress = ((double)(i + 1) / inputPaths.Count) * 100.0;
                    context.Progress?.Report(new ToolProgress(progress, $"Unito {i + 1}/{inputPaths.Count} file"));
                }

                context.Logger?.Invoke("Finalizzazione documento...");
            }, context.CancellationToken);

            var outputFileInfo = new FileInfo(context.OutputFilePath);
            context.Logger?.Invoke($"Merge completato: {outputFileInfo.Name} ({outputFileInfo.Length:n0} byte)");
            
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
        catch (iText.IO.Exceptions.IOException pdfEx)
        {
            return ToolResult.Failure($"Errore PDF: {pdfEx.Message}");
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
            return ToolResult.Failure($"Errore inatteso: {ex.Message}");
        }
    }
}
