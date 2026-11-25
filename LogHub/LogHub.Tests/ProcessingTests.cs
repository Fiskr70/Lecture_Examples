using LogHub;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogHub.Tests;

[TestClass]
public sealed class ProcessingTests
{
    [TestMethod]
    public void Parser_ValidLine_ReturnsTrimmedFields()
    {
        var record = new PipeLogParser().Parse("2026-01-01| Warning | hello ");
        Assert.IsNotNull(record);
        Assert.AreEqual("Warning", record.Severity);
        Assert.AreEqual("hello", record.Message);
    }

    [TestMethod]
    public void Parser_MissingField_ReturnsNull() =>
        Assert.IsNull(new PipeLogParser().Parse("2026-01-01|Info"));

    [TestMethod]
    public void Parser_InvalidDate_ReturnsNull() =>
        Assert.IsNull(new PipeLogParser().Parse("invalid|Info|hello"));

    [TestMethod]
    public void Parser_MessageMayContainSeparator()
    {
        var record = new PipeLogParser().Parse("2026-01-01|Info|a|b");
        Assert.IsNotNull(record);
        Assert.AreEqual("a|b", record.Message);
    }

    [TestMethod]
    public void Filter_RejectsLowerSeverity()
    {
        var record = new LogRecord(new DateTime(2026, 1, 1), "Info", "x");
        Assert.IsFalse(new SeverityFilter("Warning").Allows(record));
    }

    [TestMethod]
    public void Filter_UnknownSeverityUsesInfoLevel()
    {
        var record = new LogRecord(new DateTime(2026, 1, 1), "unknown", "x");
        Assert.IsTrue(new SeverityFilter("Info").Allows(record));
    }

    [TestMethod]
    public void Processor_CountsWrittenAndSkipped()
    {
        var sink = new RecordingSink();
        var processor = new LogProcessor(new PipeLogParser(), new SeverityFilter("Warning"),
            new PlainFormatter(), sink);
        var result = processor.Process(["2026-01-01|Info|skip", "invalid", "2026-01-01|Error|write"]);
        Assert.AreEqual(1, result.Written);
        Assert.AreEqual(2, result.Skipped);
        Assert.AreEqual(1, sink.Lines.Count);
        Assert.IsTrue(sink.Lines[0].Contains("[Error] write"));
    }

    [TestMethod]
    public void Processor_RoutesToInjectedSink()
    {
        var first = new RecordingSink();
        var second = new RecordingSink();
        var processor = new LogProcessor(new PipeLogParser(), new SeverityFilter("Info"),
            new PlainFormatter(), second);
        processor.Process(["2026-01-01|Info|hello"]);
        Assert.AreEqual(0, first.Lines.Count);
        Assert.AreEqual(1, second.Lines.Count);
    }

    private sealed class RecordingSink : ILogSink
    {
        public List<string> Lines { get; } = [];
        public void Write(LogRecord record, string formatted) => Lines.Add(formatted);
    }
}
