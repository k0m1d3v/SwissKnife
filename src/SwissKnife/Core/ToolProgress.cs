namespace SwissKnife.Core;

/// <summary>
/// Represents progress updates from a tool.
/// </summary>
public sealed class ToolProgress
{
    public ToolProgress(double? percentage = null, string? message = null)
    {
        Percentage = percentage;
        Message = message;
    }

    /// <summary>
    /// Completion percentage from 0..100. Null when indeterminate.
    /// </summary>
    public double? Percentage { get; }

    /// <summary>
    /// Status message to surface to the UI.
    /// </summary>
    public string? Message { get; }
}
