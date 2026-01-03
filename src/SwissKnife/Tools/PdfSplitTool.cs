using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using SwissKnife.Core;

namespace SwissKnife.Tools;

/// <summary>
/// Splits a PDF file into multiple documents based on page ranges.
/// </summary>
public sealed class PdfSplitTool : ITool
{
    public const string ToolId = "pdf.split";

    public string Id => ToolId;

    public string Name => "PDF Split";

    public string Description => "Divide un file PDF in più documenti";

    /// <summary>
    /// Splits a PDF file.
    /// Context.InputFilePath specifies the source PDF.
    /// Context.OutputFilePath specifies the output directory.
    /// Context.Parameters can contain:
    ///   - "mode": "pages" (split each page) or "range" (split by range)
    ///   - "range": comma-separated ranges like "1-3,5-7" (for range mode)
    ///   - "pagesPerFile": number of pages per split file (for pages mode)
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
            return ToolResult.Failure("Cartella di output non specificata.");
        }

        // Get split mode and parameters
        var mode = context.Parameters?.GetValueOrDefault("mode") as string ?? "pages";
        var pagesPerFileStr = context.Parameters?.GetValueOrDefault("pagesPerFile") as string ?? "1";
        var rangeStr = context.Parameters?.GetValueOrDefault("range") as string;

        if (!int.TryParse(pagesPerFileStr, out int pagesPerFile) || pagesPerFile < 1)
        {
            pagesPerFile = 1;
        }

        try
        {
            // Ensure output directory exists
            Directory.CreateDirectory(context.OutputFilePath);

            context.Logger?.Invoke($"Apertura file PDF: {Path.GetFileName(context.InputFilePath)}");

            await Task.Run(() =>
            {
                using var sourcePdf = new PdfDocument(new PdfReader(context.InputFilePath));
                int totalPages = sourcePdf.GetNumberOfPages();
                
                context.Logger?.Invoke($"Totale pagine: {totalPages}");

                if (mode == "range" && !string.IsNullOrWhiteSpace(rangeStr))
                {
                    // Split by custom ranges
                    SplitByRanges(sourcePdf, rangeStr, context);
                }
                else
                {
                    // Split by pages per file
                    SplitByPagesPerFile(sourcePdf, pagesPerFile, context);
                }
            }, context.CancellationToken);

            context.Logger?.Invoke("Split completato con successo.");
            return ToolResult.Success(context.OutputFilePath);
        }
        catch (OperationCanceledException)
        {
            context.Logger?.Invoke("Operazione annullata.");
            throw;
        }
        catch (iText.IO.Exceptions.IOException pdfEx)
        {
            return ToolResult.Failure($"Errore PDF: {pdfEx.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            return ToolResult.Failure("Accesso negato. Verifica i permessi.");
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

    private void SplitByPagesPerFile(PdfDocument sourcePdf, int pagesPerFile, ToolContext context)
    {
        int totalPages = sourcePdf.GetNumberOfPages();
        int fileCount = 0;
        string baseFileName = Path.GetFileNameWithoutExtension(context.InputFilePath);

        for (int startPage = 1; startPage <= totalPages; startPage += pagesPerFile)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            int endPage = Math.Min(startPage + pagesPerFile - 1, totalPages);
            fileCount++;

            string outputFileName = $"{baseFileName}_part{fileCount:D3}_p{startPage}-{endPage}.pdf";
            string outputPath = Path.Combine(context.OutputFilePath, outputFileName);

            context.Logger?.Invoke($"Creazione {outputFileName} (pagine {startPage}-{endPage})");

            ExtractPages(sourcePdf, startPage, endPage, outputPath);

            double progress = ((double)endPage / totalPages) * 100.0;
            context.Progress?.Report(new ToolProgress(progress, $"Estratte pagine {startPage}-{endPage}"));
        }

        context.Logger?.Invoke($"Creati {fileCount} file PDF.");
    }

    private void SplitByRanges(PdfDocument sourcePdf, string rangeStr, ToolContext context)
    {
        var ranges = ParseRanges(rangeStr);
        int totalPages = sourcePdf.GetNumberOfPages();
        string baseFileName = Path.GetFileNameWithoutExtension(context.InputFilePath);
        int fileCount = 0;

        foreach (var (startPage, endPage) in ranges)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (startPage < 1 || endPage > totalPages || startPage > endPage)
            {
                context.Logger?.Invoke($"Range non valido ignorato: {startPage}-{endPage}");
                continue;
            }

            fileCount++;
            string outputFileName = $"{baseFileName}_range{fileCount:D2}_p{startPage}-{endPage}.pdf";
            string outputPath = Path.Combine(context.OutputFilePath, outputFileName);

            context.Logger?.Invoke($"Creazione {outputFileName} (pagine {startPage}-{endPage})");

            ExtractPages(sourcePdf, startPage, endPage, outputPath);

            double progress = ((double)fileCount / ranges.Count) * 100.0;
            context.Progress?.Report(new ToolProgress(progress, $"Estratto range {fileCount}/{ranges.Count}"));
        }

        context.Logger?.Invoke($"Creati {fileCount} file PDF da {ranges.Count} range.");
    }

    private void ExtractPages(PdfDocument sourcePdf, int startPage, int endPage, string outputPath)
    {
        using var outputPdf = new PdfDocument(new PdfWriter(outputPath));
        sourcePdf.CopyPagesTo(startPage, endPage, outputPdf);
    }

    private List<(int start, int end)> ParseRanges(string rangeStr)
    {
        var ranges = new List<(int, int)>();
        var parts = rangeStr.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.Contains('-'))
            {
                var nums = trimmed.Split('-');
                if (nums.Length == 2 && 
                    int.TryParse(nums[0].Trim(), out int start) && 
                    int.TryParse(nums[1].Trim(), out int end))
                {
                    ranges.Add((start, end));
                }
            }
            else if (int.TryParse(trimmed, out int singlePage))
            {
                ranges.Add((singlePage, singlePage));
            }
        }

        return ranges;
    }
}
