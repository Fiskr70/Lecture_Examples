using LogHub;
using Microsoft.Extensions.DependencyInjection;

if (args.Length != 1)
{
    Console.Error.WriteLine("Использование: LogHub <путь к файлу логов>");
    return;
}
var settings = Configuration.Load("loghub.config");
var services = new ServiceCollection();
services.AddSingleton<IConfigService>(_ =>
{
    var config = new ConfigService();
    foreach (var (key, value) in settings) config.SetValue(key, value);
    return config;
});
services.AddSingleton<PipeLogParser>();
services.AddSingleton(sp => new SeverityFilter(sp.GetRequiredService<IConfigService>().GetValue("minSeverity") ?? "Info"));
services.AddSingleton<IRecordFormatter>(sp => sp.GetRequiredService<IConfigService>().GetValue("format") switch
{
    "json" => new JsonFormatter(), "csv" => new CsvFormatter(),
    "plain" or null => new PlainFormatter(), _ => new MessageFormatter()
});
services.AddSingleton<ILogSink>(sp => sp.GetRequiredService<IConfigService>().GetValue("sink") switch
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
