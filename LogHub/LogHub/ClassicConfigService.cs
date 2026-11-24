namespace LogHub;

public sealed class ClassicConfigService : IConfigService
{
    private static readonly object sync = new();
    private static ClassicConfigService? instance;
    private readonly ConfigService values = new();
    private ClassicConfigService() { }
    public static ClassicConfigService Instance()
    {
        lock (sync) return instance ??= new ClassicConfigService();
    }
    public string? GetValue(string key) => values.GetValue(key);
    public void SetValue(string key, string value) => values.SetValue(key, value);
}
