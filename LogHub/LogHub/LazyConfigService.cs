using System.Collections.Concurrent;

namespace LogHub;

public sealed class LazyConfigService : IConfigService
{
    private static readonly Lazy<LazyConfigService> instance = new(() => new LazyConfigService());
    private readonly ConcurrentDictionary<string, string> values = new(StringComparer.Ordinal);
    private LazyConfigService() { }
    public static LazyConfigService Instance() => instance.Value;
    public string? GetValue(string key) => values.TryGetValue(key, out var value) ? value : null;
    public void SetValue(string key, string value) => values[key] = value;
}
