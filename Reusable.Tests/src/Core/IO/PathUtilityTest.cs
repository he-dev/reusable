using Reusable.Marbles.Extensions;
using Xunit;

namespace Reusable.Core.IO
{
    public class PathUtilityTest
    {
        [Theory]
        [InlineData(@"c:\where\is\my\note.txt", @"*.txt", true)]
        [InlineData(@"c:\where\is\my\note.tst", @"*.t?t", true)]
        [InlineData(@"c:\where\is\my\note.pdf", @"*.txt", false)]
        [InlineData(@"c:\where\is\my\note.pdf", @"*\my\*", true)]
        [InlineData(@"c:\where\is\your\note.pdf", @"*\my\*", false)]
        [InlineData(@"c:\where\is\your\note.pdf", @"c:\*", true)]
        [InlineData(@"e:\where\is\your\note.pdf", @"c:\*", false)]
        public void Can_match(string path, string pattern, bool matches)
        {
            Assert.Equal(matches, path.WildcardMatches(pattern));
        }
    }
}