using System.Globalization;

namespace LogHub;

public sealed record LogRecord(DateTime Date, string Severity, string Message);

public sealed class PipeLogParser
{
    public LogRecord? Parse(string line)
    {
        var parts = line.Split('|', 3);
        if (parts.Length != 3 || !DateTime.TryParse(parts[0], CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date)) return null;
        return new LogRecord(date, parts[1].Trim(), parts[2].Trim());
    }
}

public sealed class SeverityFilter(string minimum)
{
    private static readonly Dictionary<string, int> Levels = new(StringComparer.Ordinal)
    {
        ["Debug"] = 0, ["Info"] = 1, ["Warning"] = 2, ["Error"] = 3, ["Critical"] = 4
    };

    public bool Allows(LogRecord record) => Level(record.Severity) >= Level(minimum);
    private static int Level(string name) => Levels.GetValueOrDefault(name, 1);
}

public interface IRecordFormatter
{
    string Format(LogRecord record);
}

public sealed class PlainFormatter : IRecordFormatter
{
    public string Format(LogRecord r) => $"{r.Date:yyyy-MM-dd HH:mm:ss} [{r.Severity}] {r.Message}";
}

public sealed class JsonFormatter : IRecordFormatter
{
    public string Format(LogRecord r) => "{\"date\":\"" + r.Date.ToString("O") + "\"," +
        "\"level\":\"" + r.Severity.ToLowerInvariant() + "\"," +
        "\"message\":\"" + r.Message.Replace("\"", "\\\"") + "\"}";
}

public sealed class CsvFormatter : IRecordFormatter
{
    public string Format(LogRecord r) => $"{r.Date:O};{r.Severity};{r.Message}";
}

public sealed class MessageFormatter : IRecordFormatter
{
    public string Format(LogRecord r) => r.Message;
}

public interface ILogSink
{
    void Write(LogRecord record, string formatted);
}

public sealed class ConsoleSink(TextWriter output) : ILogSink
{
    public void Write(LogRecord record, string formatted)
    {
        if (!ReferenceEquals(output, Console.Out)) { output.WriteLine(formatted); return; }
        var previous = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = record.Severity is "Error" or "Critical" ? ConsoleColor.Red : ConsoleColor.Gray;
            output.WriteLine(formatted);
        }
        finally { Console.ForegroundColor = previous; }
    }
}

public sealed class FileSink(TextWriter output) : ILogSink
{
    public void Write(LogRecord record, string formatted)
    {
        output.WriteLine(formatted);
        output.Flush();
    }
}

public sealed record ProcessingResult(int Written, int Skipped);

public sealed class LogProcessor(PipeLogParser parser, SeverityFilter filter,
    IRecordFormatter formatter, ILogSink sink)
{
    public ProcessingResult Process(IEnumerable<string> lines)
    {
        var written = 0;
        var skipped = 0;
        foreach (var line in lines)
        {
            var record = parser.Parse(line);
            if (record is null || !filter.Allows(record)) { skipped++; continue; }
            sink.Write(record, formatter.Format(record));
            written++;
        }
        return new ProcessingResult(written, skipped);
    }
}
