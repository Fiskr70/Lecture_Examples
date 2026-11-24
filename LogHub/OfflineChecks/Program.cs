using LogHub;

var tests = new List<(string Name, Action Check)>
{
    ("valid line", () => { var r = new PipeLogParser().Parse("2026-01-01| Warning | hello "); Require(r is not null && r.Severity == "Warning" && r.Message == "hello"); }),
    ("missing field", () => Require(new PipeLogParser().Parse("2026-01-01|Info") is null)),
    ("bad date", () => Require(new PipeLogParser().Parse("bad|Info|hello") is null)),
    ("separator in message", () => Require(new PipeLogParser().Parse("2026-01-01|Info|a|b")?.Message == "a|b")),
    ("severity below minimum", () => Require(!new SeverityFilter("Warning").Allows(new(DateTime.MinValue, "Info", "x")))),
    ("unknown severity uses Info", () => Require(new SeverityFilter("Info").Allows(new(DateTime.MinValue, "unknown", "x")))),
    ("written and skipped", () => { var sink = new MemorySink(); var p = new LogProcessor(new(), new("Warning"), new PlainFormatter(), sink); var result = p.Process(["2026-01-01|Info|skip", "invalid", "2026-01-01|Error|write"]); Require(result.Written == 1 && result.Skipped == 2 && sink.Lines.Count == 1); }),
    ("injected sink", () => { var first = new MemorySink(); var second = new MemorySink(); var p = new LogProcessor(new(), new("Info"), new PlainFormatter(), second); p.Process(["2026-01-01|Info|hello"]); Require(first.Lines.Count == 0 && second.Lines.Count == 1); }),
    ("format plain", () => Require(new PlainFormatter().Format(new(new DateTime(2026,1,1), "Error", "text")) == "2026-01-01 00:00:00 [Error] text")),
    ("unsupported sink", () => { try { SinkRegistry.Create("unknown", new Dictionary<string,string>()); throw new Exception("Expected NotSupportedException"); } catch (NotSupportedException) { } }),
    ("classic singleton", () => { var c = ClassicConfigService.Instance(); c.SetValue("verify", "yes"); Require(ReferenceEquals(c, ClassicConfigService.Instance()) && ClassicConfigService.Instance().GetValue("verify") == "yes"); }),
    ("lazy singleton", () => { var c = LazyConfigService.Instance(); c.SetValue("verify", "yes"); Require(ReferenceEquals(c, LazyConfigService.Instance()) && LazyConfigService.Instance().GetValue("verify") == "yes"); }),
    ("independent configs", () => { var first = new ConfigService(); var second = new ConfigService(); first.SetValue("verify", "yes"); Require(second.GetValue("verify") is null); })
};
var failed = 0;
foreach (var (name, check) in tests)
{
    try { check(); Console.WriteLine($"PASS {name}"); }
    catch (Exception e) { failed++; Console.WriteLine($"FAIL {name}: {e.Message}"); }
}
Console.WriteLine($"Checks: {tests.Count}, passed: {tests.Count - failed}, failed: {failed}");
return failed == 0 ? 0 : 1;

static void Require(bool value) { if (!value) throw new InvalidOperationException("Assertion failed"); }

sealed class MemorySink : ILogSink
{
    public List<string> Lines { get; } = [];
    public void Write(LogRecord record, string formatted) => Lines.Add(formatted);
}
