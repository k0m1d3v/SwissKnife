using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SwissKnife.Core;

namespace SwissKnife.Tools;

/// <summary>
/// Computes a SHA-256 hash for the provided file path.
/// </summary>
public sealed class HashTool : ITool
{
    public const string ToolId = "hash.sha256";

    public string Id => ToolId;

    public string Name => "Hash File";

    public string Description => "Calcola l'hash SHA-256 di un file usando streaming";

    public async Task<ToolResult> RunAsync(ToolContext context)
    {
        if (string.IsNullOrWhiteSpace(context.InputFilePath))
        {
            return ToolResult.Failure("Percorso file non specificato.");
        }

        var filePath = context.InputFilePath;

        if (!File.Exists(filePath))
        {
            return ToolResult.Failure("Il file specificato non esiste.");
        }

        try
        {
            context.Logger?.Invoke($"Apertura file: {filePath}");

            var fileInfo = new FileInfo(filePath);
            long totalLength = fileInfo.Length;
            long processed = 0;

            using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);

            using var sha256 = SHA256.Create();

            var buffer = new byte[81920];
            int bytesRead;
            while ((bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, buffer.Length), context.CancellationToken)) > 0)
            {
                sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
                processed += bytesRead;

                if (totalLength > 0)
                {
                    double percentage = (double)processed / totalLength * 100d;
                    context.Progress?.Report(new ToolProgress(percentage, $"Letti {processed:n0} byte su {totalLength:n0}"));
                }
                else
                {
                    context.Progress?.Report(new ToolProgress(null, $"Letti {processed:n0} byte"));
                }
            }

            sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            string hash = Convert.ToHexString(sha256.Hash ?? Array.Empty<byte>());

            context.Logger?.Invoke("Hash completato.");
            return ToolResult.Success(hash);
        }
        catch (OperationCanceledException)
        {
            context.Logger?.Invoke("Operazione annullata.");
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            return ToolResult.Failure("Accesso al file negato.");
        }
        catch (IOException ioEx)
        {
            return ToolResult.Failure($"Errore di I/O: {ioEx.Message}");
        }
        catch (CryptographicException cryptoEx)
        {
            return ToolResult.Failure($"Errore crittografico: {cryptoEx.Message}");
        }
        catch (Exception ex)
        {
            return ToolResult.Failure($"Errore inatteso: {ex.Message}");
        }
    }
}
