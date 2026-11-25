using System.Reflection;

namespace LogHub;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SinkNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

public static class SinkRegistry
{
    private static readonly IReadOnlyDictionary<string, Type> SinkTypes = typeof(SinkRegistry)
        .Assembly.GetTypes()
        .Where(type => !type.IsAbstract && typeof(ILogSink).IsAssignableFrom(type))
        .Select(type => (Type: type, Name: type.GetCustomAttribute<SinkNameAttribute>()))
        .Where(sink => sink.Name is not null)
        .ToDictionary(sink => sink.Name!.Name, sink => sink.Type, StringComparer.Ordinal);

    public static Type ResolveType(string name) =>
        SinkTypes.TryGetValue(name, out var sinkType)
            ? sinkType
            : throw new NotSupportedException($"Приёмник '{name}' не поддерживается");
}

[SinkName("file")]
public sealed class FileSink : ILogSink, IDisposable
{
    private readonly StreamWriter writer = new($"logs-{DateTime.Now:yyyyMMdd}.txt", append: true);

    public void Write(LogRecord record, string formatted)
    {
        writer.WriteLine(formatted);
        writer.Flush();
    }

    public void Dispose() => writer.Dispose();
}

// Имитирует вывод учебного примера без подключения к базе данных.
[SinkName("database")]
public sealed class DemonstrationDatabaseSink : ILogSink, IDisposable
{
    public DemonstrationDatabaseSink(IConfigService config) =>
        Console.WriteLine($"[БД] соединение открыто: {config.GetValue("connection") ?? "local"}");

    public void Write(LogRecord record, string formatted) => Console.WriteLine(
        $"[БД] INSERT INTO logs(date, severity, message) VALUES(@d, @s, @m) -> {record.Date:O}, {record.Severity}, {formatted}");

    public void Dispose() => Console.WriteLine("[БД] соединение закрыто");
}
