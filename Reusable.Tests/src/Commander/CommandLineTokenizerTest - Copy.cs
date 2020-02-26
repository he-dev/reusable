using System.Linq;
using Reusable.Lexing;
using Xunit;

namespace Reusable.Commander
{
    // public class CommandLineTokenizerTest
    // {
    //     private static readonly ICommandLineTokenizer Tokenizer = new CommandLineTokenizer();
    //
    //     [Fact]
    //     public void Tokenize_Empty_Empty()
    //     {
    //         var tokens = Tokenizer.Tokenize(string.Empty).ToList();
    //         //Assert.That.IsEmpty(tokens);
    //     }
    //
    //     [Fact]
    //     public void Tokenize_Quoted_SingleToken()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"""foo bar""").ToList();
    //         Assert.Equal(@"foo bar", tokens.Single());
    //     }
    //
    //     [Fact]
    //     public void Tokenize_SpaceSeparated_SpaceSeparated()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"foo bar baz").ToList();
    //         Assert.Equal(3, tokens.Count);
    //         Assert.Equal(new[] { "foo", "bar", "baz" }, tokens);
    //     }
    //
    //     [Fact]
    //     public void Tokenize_ColonSeparated_ColonSeparated()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"-foo:bar -baz:qux").ToList();
    //         Assert.Equal(4, tokens.Count);
    //         Assert.Equal(new[] { "-foo", "bar", "-baz", "qux" }, tokens);
    //     }
    //
    //     [Fact]
    //     public void Tokenize_EqualSignSeparated_EqualSignSeparated()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"-foo=bar -baz=qux").ToList();
    //         Assert.Equal(4, tokens.Count);
    //         Assert.Equal(new[] { "-foo", "bar", "-baz", "qux" }, tokens);
    //     }
    //
    //     [Fact]
    //     public void Tokenize_MixedSeparated_MixedSignSeparated()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"-foo=bar -baz:qux").ToList();
    //         Assert.Equal(4, tokens.Count);
    //         Assert.Equal(new[] { "-foo", "bar", "-baz", "qux" }, tokens);
    //     }
    //
    //     [Fact]
    //     public void Tokenize_EscapedSeparators_NotSeparated()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"-foo\=bar -baz\:qux").ToList();
    //         Assert.Equal(2, tokens.Count);
    //         Assert.Equal(new[] { "-foo=bar", "-baz:qux" }, tokens);
    //     }
    //
    //     [Fact]
    //     public void Tokenize_EscapedEscapeChar_NotSeparated()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"foo\\bar").ToList();
    //         Assert.Equal(1, tokens.Count);
    //         Assert.Equal(new[] { "foo\\bar" }, tokens);
    //     }
    //
    //     [Fact]
    //     public void Tokenize_QuotedPath_NotSeparated()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"""C:foo\bar\baz.qux""").ToList();
    //         Assert.Equal(1, tokens.Count);
    //         Assert.Equal(new[] { @"C:foo\bar\baz.qux" }, tokens);
    //     }
    //
    //     [Fact]
    //     public void Tokenize_RelativePath_NotSeparated()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"\bar\baz.qux").ToList();
    //         Assert.Equal(1, tokens.Count);
    //         Assert.Equal(new[] { @"\bar\baz.qux" }, tokens);
    //     }
    //
    //     [Fact]
    //     public void Tokenize_PipeSeparatedWithoutSpace_PipeCollected()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"foo -fooo|bar -baar").ToList();
    //         Assert.Equal(5, tokens.Count);
    //         Assert.Equal(new[] { "foo", "-fooo", "|", "bar", "-baar" }, tokens);
    //     }
    //
    //     [Fact]
    //     public void Tokenize_PipeSeparatedWithSpace_PipeCollected()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"foo -fooo | bar -baar").ToList();
    //         Assert.Equal(5, tokens.Count);
    //         Assert.Equal(new[] { "foo", "-fooo", "|", "bar", "-baar" }, tokens);
    //     }
    //
    //     [Fact]
    //     public void Tokenize_CommaSeparated_CommaSeparated()
    //     {
    //         var tokens = Tokenizer.Tokenize(@"-foo:1, 2, 3").ToList();
    //         Assert.Equal(4, tokens.Count);
    //         Assert.Equal(new[] { "-foo", "1", "2", "3" }, tokens);
    //     }
    //
    //     //[Fact]
    //     //public void Tokenize_DashArgument_Separated()
    //     //{
    //     //    var tokens = Tokenizer.Tokenize(@"-foo -bar-baz").ToList();
    //     //    Assert.Equal(2, tokens.Count);
    //     //    CollectionAssert.Equal(new[] { "-foo", "bar-baz" }, tokens);
    //     //}
    // }

    public class CommandLineTokenizerTest2
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
        [InlineData("cmd -f -f --arg  -fl", "cmd", "f", "f", "arg", "fl")]
        [InlineData("cmd --arg \"arg:val\"    \"val-arg\" -f --arg", "cmd", "arg", "arg:val", "val-arg", "f", "arg")]
        public void Can_tokenize_command_lines(string value, params string[] expected)
        {
            var tokens = Tokenizer.Tokenize(value).ToList();
            Assert.Equal(expected, tokens.Select(t => t.Value).ToArray());
        }
    }
}
