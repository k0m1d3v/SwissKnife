using System;
using System.Collections.Generic;
using System.Linq;

namespace SwissKnife.Core;

/// <summary>
/// Holds available tools and allows lookup by identifier.
/// </summary>
public sealed class ToolRegistry
{
    private readonly Dictionary<string, ITool> _tools = new(StringComparer.OrdinalIgnoreCase);

    public void Register(ITool tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        _tools[tool.Id] = tool;
    }

    public ITool? GetById(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        return _tools.TryGetValue(id, out var tool) ? tool : null;
    }

    public IReadOnlyCollection<ITool> GetAll() => _tools.Values.ToList();
}
