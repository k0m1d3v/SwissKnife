namespace SwissKnife.Core;

/// <summary>
/// Represents the outcome of a tool execution.
/// </summary>
public sealed class ToolResult
{
    private ToolResult(bool isSuccess, string? output, string? error)
    {
        IsSuccess = isSuccess;
        Output = output;
        Error = error;
    }

    public bool IsSuccess { get; }

    public string? Output { get; }

    public string? Error { get; }

    public static ToolResult Success(string output) => new(true, output, null);

    public static ToolResult Failure(string error) => new(false, null, error);
}
