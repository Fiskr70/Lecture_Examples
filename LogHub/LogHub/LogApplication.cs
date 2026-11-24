namespace LogHub;

public sealed class LogApplication(LogProcessor processor)
{
    public ProcessingResult Run(string path) => processor.Process(File.ReadLines(path));
}
