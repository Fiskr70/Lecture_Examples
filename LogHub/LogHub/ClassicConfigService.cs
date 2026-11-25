using System.Collections.Concurrent;

namespace LogHub;

public sealed class ClassicConfigService : IConfigService
{
    private static readonly object sync = new();
    private static ClassicConfigService? instance;
    private readonly ConcurrentDictionary<string, string> values = new(StringComparer.Ordinal);
    private ClassicConfigService() { }
    public static ClassicConfigService Instance()
    {
        lock (sync) return instance ??= new ClassicConfigService();
    }
    public string? GetValue(string key) => values.TryGetValue(key, out var value) ? value : null;
    public void SetValue(string key, string value) => values[key] = value;
}
