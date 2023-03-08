using Reusable;
using Xunit;

namespace Reusable.Tests;

public class SemanticVersionTest
{
    [Fact]
    public void CanParse()
    {
        Assert.Equal(new SemanticVersion(3, 2, 1), SemanticVersion.Parse("3.2.1"));
    }
        
    // [Fact]
    // public void CanDetectPrefix()
    // {
    //     Assert.True(SemanticVersion.Parse("v3.2.1").Prefix);
    // }
        
    [Fact]
    public void CanUsePrefix()
    {
        Assert.Equal((string?)"3.2.1",  (string?)SemanticVersion.Parse("3.2.1").ToString());
    }
        
    [Fact]
    public void CanIgnorePrefix()
    {
        Assert.Equal("3.2.1",  SemanticVersion.Parse("3.2.1")); //.Also(x => { x.Prefix = false;}).ToString());
    }
}