using Reusable.Essentials;
using Reusable.Essentials.Data;
using Xunit;

namespace Reusable
{
    public class SemanticVersionTest
    {
        [Fact]
        public void CanParse()
        {
            Assert.Equal(new SemanticVersion(3, 2, 1), SemanticVersion.Parse("v3.2.1"));
        }
        
        [Fact]
        public void CanDetectPrefix()
        {
            Assert.True(SemanticVersion.Parse("v3.2.1").Prefix);
        }
        
        [Fact]
        public void CanUsePrefix()
        {
            Assert.Equal("v3.2.1",  SemanticVersion.Parse("v3.2.1").ToString());
        }
        
        [Fact]
        public void CanIgnorePrefix()
        {
            Assert.Equal("3.2.1",  SemanticVersion.Parse("v3.2.1").Also(x => { x.Prefix = false;}).ToString());
        }
    }
}