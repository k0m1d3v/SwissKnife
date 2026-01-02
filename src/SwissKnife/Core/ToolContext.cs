using System;
using System.Threading;

namespace SwissKnife.Core;

/// <summary>
/// Provides execution context for tools, including progress, logging and cancellation.
/// </summary>
public sealed class ToolContext
{
    /// <summary>
    /// Optional input file path for file-based tools.
    /// </summary>
    public string? InputFilePath { get; init; }

    /// <summary>
    /// Reports progress updates to the caller.
    /// </summary>
    public IProgress<ToolProgress>? Progress { get; init; }

    /// <summary>
    /// Logs informational or error messages.
    /// </summary>
    public Action<string>? Logger { get; init; }

    /// <summary>
    /// Cancellation token used to cancel long-running operations.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}
