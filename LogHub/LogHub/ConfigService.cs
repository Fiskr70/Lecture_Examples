using System.Collections.Concurrent;

namespace LogHub;

public sealed class ConfigService : IConfigService
{
    private readonly ConcurrentDictionary<string, string> values = new(StringComparer.Ordinal);
    public string? GetValue(string key) => values.TryGetValue(key, out var value) ? value : null;
    public void SetValue(string key, string value) => values[key] = value;
}
