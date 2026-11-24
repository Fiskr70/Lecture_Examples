namespace LogHub;

public sealed class LazyConfigService : IConfigService
{
    private static readonly Lazy<LazyConfigService> instance = new(() => new LazyConfigService());
    private readonly ConfigService values = new();
    private LazyConfigService() { }
    public static LazyConfigService Instance() => instance.Value;
    public string? GetValue(string key) => values.GetValue(key);
    public void SetValue(string key, string value) => values.SetValue(key, value);
}
