using System.Reflection;

namespace LogHub;

public interface IManagedLogSink : ILogSink, IDisposable;

public interface ISinkFactory
{
    string Name { get; }
    IManagedLogSink Create(IReadOnlyDictionary<string, string> settings);
}

public static class SinkRegistry
{
    public static IManagedLogSink Create(string name, IReadOnlyDictionary<string, string> settings)
    {
        var factories = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => !t.IsAbstract && typeof(ISinkFactory).IsAssignableFrom(t))
            .Select(t => (ISinkFactory)Activator.CreateInstance(t)!)
            .ToDictionary(f => f.Name, StringComparer.Ordinal);
        if (!factories.TryGetValue(name, out var factory))
            throw new NotSupportedException($"Приёмник '{name}' не поддерживается");
        return factory.Create(settings);
    }
}

public sealed class ConsoleSinkFactory : ISinkFactory
{
    public string Name => "console";
    public IManagedLogSink Create(IReadOnlyDictionary<string, string> settings) => new ManagedConsoleSink();
}

public sealed class ManagedConsoleSink : IManagedLogSink
{
    private readonly ConsoleSink _sink = new(Console.Out);
    public void Write(LogRecord record, string formatted) => _sink.Write(record, formatted);
    public void Dispose() { }
}

public sealed class FileSinkFactory : ISinkFactory
{
    public string Name => "file";
    public IManagedLogSink Create(IReadOnlyDictionary<string, string> settings) => new ManagedFileSink();
}

public sealed class ManagedFileSink : IManagedLogSink
{
    private readonly StreamWriter _writer = new($"logs-{DateTime.Now:yyyyMMdd}.txt", append: true);
    public void Write(LogRecord record, string formatted) { _writer.WriteLine(formatted); _writer.Flush(); }
    public void Dispose() => _writer.Dispose();
}

public sealed class DatabaseSinkFactory : ISinkFactory
{
    public string Name => "database";
    public IManagedLogSink Create(IReadOnlyDictionary<string, string> settings) =>
        new DemonstrationDatabaseSink(settings.GetValueOrDefault("connection", "local"));
}

// Имитация сообщений демонстрационного соединения, а не доступ к реальной БД.
public sealed class DemonstrationDatabaseSink : IManagedLogSink
{
    public DemonstrationDatabaseSink(string connection) => Console.WriteLine($"[БД] соединение открыто: {connection}");
    public void Write(LogRecord r, string text) => Console.WriteLine(
        $"[БД] INSERT INTO logs(date, severity, message) VALUES(@d, @s, @m) -> {r.Date:O}, {r.Severity}, {text}");
    public void Dispose() => Console.WriteLine("[БД] соединение закрыто");
}
