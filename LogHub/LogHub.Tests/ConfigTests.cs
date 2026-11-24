using LogHub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace LogHub.Tests;

[TestClass]
public class ConfigTests
{
    [TestMethod]
    public void ClassicInstanceIsShared()
    {
        var first = ClassicConfigService.Instance();
        first.SetValue("shared-test", "written");
        Assert.AreSame(first, ClassicConfigService.Instance());
        Assert.AreEqual("written", ClassicConfigService.Instance().GetValue("shared-test"));
    }
    [TestMethod]
    public void LazyInstanceIsShared()
    {
        var first = LazyConfigService.Instance();
        first.SetValue("lazy-test", "written");
        Assert.AreSame(first, LazyConfigService.Instance());
        Assert.AreEqual("written", LazyConfigService.Instance().GetValue("lazy-test"));
    }
    [TestMethod]
    public void ClassicWritesStateThatCanLeak()
    {
        ClassicConfigService.Instance().SetValue("order-dependent", "present");
        Assert.AreEqual("present", ClassicConfigService.Instance().GetValue("order-dependent"));
    }
    [TestMethod]
    [Ignore("��室�� �⤥�쭮, �� ������ ��᫥ ClassicWritesStateThatCanLeak: ������쭮� ���ﭨ� ��࠭���� ����� ��⠬�.")]
    public void ClassicExpectedEmptyState() =>
        Assert.IsNull(ClassicConfigService.Instance().GetValue("order-dependent"));
    [TestMethod]
    public void SeparateContainersHaveSeparateSingletons()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfigService, ConfigService>();
        using var firstContainer = services.BuildServiceProvider();
        using var secondContainer = services.BuildServiceProvider();
        var first = firstContainer.GetRequiredService<IConfigService>();
        var same = firstContainer.GetRequiredService<IConfigService>();
        var independent = secondContainer.GetRequiredService<IConfigService>();
        first.SetValue("key", "first");
        Assert.AreSame(first, same);
        Assert.AreNotSame(first, independent);
        Assert.IsNull(independent.GetValue("key"));
    }
}
