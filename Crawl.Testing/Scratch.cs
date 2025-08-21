using Crawl.ECS;
using Xunit.Abstractions;

namespace Crawl.Testing;

public class TestSystem : ECS.System.System
{
    public override void Execute(World world)
    {
        throw new NotImplementedException();
    }
}

public class TestSystem2 : ECS.System.System
{
    public override void Execute(World world)
    {
        throw new NotImplementedException();
    }
}

public class Scratch
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Scratch(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Testing()
    {
        var pc = new TestSystem();
        var pc2 = new TestSystem2();

        _testOutputHelper.WriteLine($"""
                                     1:
                                        FQ {pc.GetType().FullName}
                                        Hash {pc.GetHashCode()}
                                     2:
                                        FQ {pc2.GetType().FullName}
                                        Hash {pc2.GetHashCode()}
                                     """);
    }
}