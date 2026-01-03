using System.Threading;
using System.Threading.Tasks;

namespace SwissKnife.Core;

/// <summary>
/// Defines a contract for toolbox commands.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Stable identifier used internally.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Display name shown in UI.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Short description of the tool.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the tool using the provided context.
    /// </summary>
    /// <param name="context">Execution context containing inputs, logging and cancellation.</param>
    /// <returns>Result containing success flag and payload.</returns>
    Task<ToolResult> RunAsync(ToolContext context);
}
