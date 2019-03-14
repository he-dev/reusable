using System.Linq;
using Reusable.Commander;
using Xunit;

namespace Reusable.Tests.Commander
{
    public class CommandLineTokenizerTest
    {
        private static readonly ICommandLineTokenizer Tokenizer = new CommandLineTokenizer();

        [Fact]
        public void Tokenize_Empty_Empty()
        {
            var tokens = Tokenizer.Tokenize(string.Empty).ToList();
            //Assert.That.IsEmpty(tokens);
        }

        [Fact]
        public void Tokenize_Quoted_SingleToken()
        {
            var tokens = Tokenizer.Tokenize(@"""foo bar""").ToList();
            Assert.Equal(@"foo bar", tokens.Single());
        }

        [Fact]
        public void Tokenize_SpaceSeparated_SpaceSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"foo bar baz").ToList();
            Assert.Equal(3, tokens.Count);
            Assert.Equal(new[] { "foo", "bar", "baz" }, tokens);
        }

        [Fact]
        public void Tokenize_ColonSeparated_ColonSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"-foo:bar -baz:qux").ToList();
            Assert.Equal(4, tokens.Count);
            Assert.Equal(new[] { "-foo", "bar", "-baz", "qux" }, tokens);
        }

        [Fact]
        public void Tokenize_EqualSignSeparated_EqualSignSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"-foo=bar -baz=qux").ToList();
            Assert.Equal(4, tokens.Count);
            Assert.Equal(new[] { "-foo", "bar", "-baz", "qux" }, tokens);
        }

        [Fact]
        public void Tokenize_MixedSeparated_MixedSignSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"-foo=bar -baz:qux").ToList();
            Assert.Equal(4, tokens.Count);
            Assert.Equal(new[] { "-foo", "bar", "-baz", "qux" }, tokens);
        }

        [Fact]
        public void Tokenize_EscapedSeparators_NotSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"-foo\=bar -baz\:qux").ToList();
            Assert.Equal(2, tokens.Count);
            Assert.Equal(new[] { "-foo=bar", "-baz:qux" }, tokens);
        }

        [Fact]
        public void Tokenize_EscapedEscapeChar_NotSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"foo\\bar").ToList();
            Assert.Equal(1, tokens.Count);
            Assert.Equal(new[] { "foo\\bar" }, tokens);
        }

        [Fact]
        public void Tokenize_QuotedPath_NotSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"""C:foo\bar\baz.qux""").ToList();
            Assert.Equal(1, tokens.Count);
            Assert.Equal(new[] { @"C:foo\bar\baz.qux" }, tokens);
        }

        [Fact]
        public void Tokenize_RelativePath_NotSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"\bar\baz.qux").ToList();
            Assert.Equal(1, tokens.Count);
            Assert.Equal(new[] { @"\bar\baz.qux" }, tokens);
        }

        [Fact]
        public void Tokenize_PipeSeparatedWithoutSpace_PipeCollected()
        {
            var tokens = Tokenizer.Tokenize(@"foo -fooo|bar -baar").ToList();
            Assert.Equal(5, tokens.Count);
            Assert.Equal(new[] { "foo", "-fooo", "|", "bar", "-baar" }, tokens);
        }

        [Fact]
        public void Tokenize_PipeSeparatedWithSpace_PipeCollected()
        {
            var tokens = Tokenizer.Tokenize(@"foo -fooo | bar -baar").ToList();
            Assert.Equal(5, tokens.Count);
            Assert.Equal(new[] { "foo", "-fooo", "|", "bar", "-baar" }, tokens);
        }

        [Fact]
        public void Tokenize_CommaSeparated_CommaSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"-foo:1, 2, 3").ToList();
            Assert.Equal(4, tokens.Count);
            Assert.Equal(new[] { "-foo", "1", "2", "3" }, tokens);
        }

        //[Fact]
        //public void Tokenize_DashArgument_Separated()
        //{
        //    var tokens = Tokenizer.Tokenize(@"-foo -bar-baz").ToList();
        //    Assert.Equal(2, tokens.Count);
        //    CollectionAssert.Equal(new[] { "-foo", "bar-baz" }, tokens);
        //}
    }
}
