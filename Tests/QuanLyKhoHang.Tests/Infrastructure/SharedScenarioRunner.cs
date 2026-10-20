namespace QuanLyKhoHang.Tests.Infrastructure;

/// <summary>Dispatches data-driven plans to reusable handlers without one test method per workbook row.</summary>
public sealed class SharedScenarioRunner
{
    private readonly IReadOnlyDictionary<string, Func<ScenarioPlan, Task>> handlers;

    public SharedScenarioRunner(IEnumerable<KeyValuePair<string, Func<ScenarioPlan, Task>>> handlers)
    {
        this.handlers = handlers.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
    }

    public Task ExecuteAsync(ScenarioPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (!handlers.TryGetValue(plan.Action, out var handler))
        {
            throw new InvalidOperationException(
                $"No shared handler is registered for action '{plan.Action}' ({plan.TestCase.Id}).");
        }

        return handler(plan);
    }
}
