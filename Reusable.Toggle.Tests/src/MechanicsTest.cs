using System.Linq;
using Reusable.Toggle.Mechanics;
using Xunit;

namespace Reusable.Toggle.Tests;

public class MechanicsTest
{
    private static readonly IFeatureService Features = new FeatureService
    {
        { "foo", new AlwaysOn() },
        { "bar", new AlwaysOff() },
        { "baz", new CountdownFeature(2) }
    };

    [Fact]
    public void AlwaysOn_executes()
    {
        var foo = false;
        if (Features.TryUse("foo", out var f))
        {
            using (f)
            {
                foo = true;
            }
        }

        Assert.True(foo);
    }

    [Fact]
    public void AlwaysOn_does_not_execute()
    {
        var bar = false;
        if (Features.TryUse("bar", out var b))
        {
            using (b)
            {
                bar = true;
            }
        }

        Assert.False(bar);
    }

    [Fact ]
    public void Countdown_executes_n_times()
    {
        var baz = 0;
        foreach (var x in Enumerable.Range(0, 10))
        {
            if (Features.TryUse("baz", out var b1))
            {
                using (b1)
                {
                    baz++;
                }
            }
        }

        Assert.Equal(2, baz);
    }
}