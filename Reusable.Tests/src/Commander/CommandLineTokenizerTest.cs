using System.Linq;
using Reusable.Exceptionize;
using Reusable.Lexing;
using Xunit;

namespace Reusable.Commander
{
    public class CommandLineTokenizerTest
    {
        private static readonly ITokenizer<CommandLineToken> Tokenizer = new CommandLineTokenizer();

        [Theory]
        [InlineData("cmd", "cmd")]
        [InlineData("cmd --arg", "cmd", "arg")]
        [InlineData("cmd --arg --arg", "cmd", "arg", "arg")]
        [InlineData("cmd --arg  val", "cmd", "arg", "val")]
        [InlineData("cmd --arg --arg val val", "cmd", "arg", "arg", "val", "val")]
        [InlineData("cmd --arg val val", "cmd", "arg", "val", "val")]
        [InlineData("cmd val val --arg  --arg val    val", "cmd", "val", "val", "arg", "arg", "val", "val")]
        [InlineData("cmd -f -f --arg  -f -l", "cmd", "f", "f", "arg", "f", "l")]
        [InlineData("cmd --arg \"arg:val\"    \"val-arg\" -f --arg", "cmd", "arg", "arg:val", "val-arg", "f", "arg")]
        [InlineData("cmd|cmd", "cmd", "|", "cmd")]
        [InlineData("cmd --arg   ", "cmd", "arg")]
        [InlineData("   cmd   --arg ", "cmd", "arg")]
        [InlineData("cmd -arg", null)]
        [InlineData("cmd -arg -arg -args -arg val:val val:val", null)]
        [InlineData("CMD --arg --arg val:val val:val", "CMD", "arg", "arg", "val:val", "val:val")]
        public void Can_tokenize_command_lines(string value, params string[] expected)
        {
            if (expected is {})
            {
                var tokens = Tokenizer.Tokenize(value).ToList();
                Assert.Equal(expected, tokens.Select(t => t.Value).ToArray());
            }
            else
            {
                Assert.ThrowsAny<DynamicException>(() => Tokenizer.Tokenize(value).ToList());
            }
        }
    }
}