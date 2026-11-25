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
    foreach (var (key, value) in settings)
        config.SetValue(key, value);
    return config;
});

services.AddSingleton<PipeLogParser>();
services.AddSingleton<SeverityFilter>();
var formatterType = settings.GetValueOrDefault("format") switch
{
    "json" => typeof(JsonFormatter),
    "csv" => typeof(CsvFormatter),
    "plain" or null => typeof(PlainFormatter),
    _ => typeof(MessageFormatter)
};
var sinkType = SinkRegistry.ResolveType(settings.GetValueOrDefault("sink", "console"));
services.AddSingleton(typeof(IRecordFormatter), formatterType);
services.AddSingleton(typeof(ILogSink), sinkType);
services.AddTransient<LogProcessor>();
services.AddTransient<LogApplication>();

ProcessingResult result;
using (var container = services.BuildServiceProvider(new ServiceProviderOptions
       { ValidateOnBuild = true, ValidateScopes = true }))
{
    var application = container.GetRequiredService<LogApplication>();
    result = application.Run(args[0]);
}

Console.WriteLine($"Записано: {result.Written}, пропущено: {result.Skipped}");
