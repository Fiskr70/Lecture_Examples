using LogHub;

if (args.Length != 1)
{
    Console.Error.WriteLine("Использование: LogHub <путь к файлу логов>");
    return;
}

var config = Configuration.Load("loghub.config");
var formatters = new Dictionary<string, IRecordFormatter>(StringComparer.Ordinal)
{
    ["plain"] = new PlainFormatter(), ["json"] = new JsonFormatter(), ["csv"] = new CsvFormatter()
};
var formatter = formatters.GetValueOrDefault(config["format"], new MessageFormatter());
ProcessingResult result;
using (var sink = SinkRegistry.Create(config["sink"], config))
{
    var processor = new LogProcessor(new PipeLogParser(), new SeverityFilter(config["minSeverity"]),
        formatter, sink);
    result = processor.Process(File.ReadLines(args[0]));
}
Console.WriteLine($"Записано: {result.Written}, пропущено: {result.Skipped}");
