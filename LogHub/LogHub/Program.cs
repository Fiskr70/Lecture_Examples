using LogHub;
using Microsoft.Extensions.DependencyInjection;

if (args.Length != 1)
{
    Console.Error.WriteLine("Использование: LogHub <путь к файлу логов>");
    return;
}
var settings = Configuration.Load("loghub.config");
var services = new ServiceCollection();
var config = new ConfigService();
foreach (var (key, value) in settings) config.SetValue(key, value);
services.AddSingleton<IConfigService>(config);
services.AddSingleton<PipeLogParser>();
services.AddSingleton(new SeverityFilter(config.GetValue("minSeverity") ?? "Info"));
IRecordFormatter formatter = config.GetValue("format") switch
{
    "json" => new JsonFormatter(), "csv" => new CsvFormatter(),
    "plain" or null => new PlainFormatter(), _ => new MessageFormatter()
};
services.AddSingleton(formatter);
services.AddSingleton<ILogSink>(_ => config.GetValue("sink") switch
{
    "file" => new ManagedFileSink(), "console" or null => new ManagedConsoleSink(),
    _ => throw new InvalidOperationException("Неизвестный профиль приёмника")
});
services.AddTransient<LogProcessor>();
services.AddTransient<LogApplication>();
using var container = services.BuildServiceProvider();
var application = container.GetRequiredService<LogApplication>();
var result = application.Run(args[0]);
Console.WriteLine($"Записано: {result.Written}, пропущено: {result.Skipped}");

