using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;

namespace Reusable.Tests.Commander
{
    [TestClass]
    public class CommandLineTokenizerTest
    {
        private static readonly ICommandLineTokenizer Tokenizer = new CommandLineTokenizer();

        [TestMethod]
        public void Tokenize_Empty_Empty()
        {
            var tokens = Tokenizer.Tokenize(string.Empty).ToList();
            //Assert.That.IsEmpty(tokens);
        }

        [TestMethod]
        public void Tokenize_Quoted_SingleToken()
        {
            var tokens = Tokenizer.Tokenize(@"""foo bar""").ToList();
            Assert.AreEqual(@"foo bar", tokens.Single());
        }

        [TestMethod]
        public void Tokenize_SpaceSeparated_SpaceSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"foo bar baz").ToList();
            Assert.AreEqual(3, tokens.Count);
            CollectionAssert.AreEqual(new[] { "foo", "bar", "baz" }, tokens);
        }

        [TestMethod]
        public void Tokenize_ColonSeparated_ColonSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"-foo:bar -baz:qux").ToList();
            Assert.AreEqual(4, tokens.Count);
            CollectionAssert.AreEqual(new[] { "-foo", "bar", "-baz", "qux" }, tokens);
        }

        [TestMethod]
        public void Tokenize_EqualSignSeparated_EqualSignSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"-foo=bar -baz=qux").ToList();
            Assert.AreEqual(4, tokens.Count);
            CollectionAssert.AreEqual(new[] { "-foo", "bar", "-baz", "qux" }, tokens);
        }

        [TestMethod]
        public void Tokenize_MixedSeparated_MixedSignSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"-foo=bar -baz:qux").ToList();
            Assert.AreEqual(4, tokens.Count);
            CollectionAssert.AreEqual(new[] { "-foo", "bar", "-baz", "qux" }, tokens);
        }

        [TestMethod]
        public void Tokenize_EscapedSeparators_NotSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"-foo\=bar -baz\:qux").ToList();
            Assert.AreEqual(2, tokens.Count);
            CollectionAssert.AreEqual(new[] { "-foo=bar", "-baz:qux" }, tokens);
        }

        [TestMethod]
        public void Tokenize_EscapedEscapeChar_NotSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"foo\\bar").ToList();
            Assert.AreEqual(1, tokens.Count);
            CollectionAssert.AreEqual(new[] { "foo\\bar" }, tokens);
        }

        [TestMethod]
        public void Tokenize_QuotedPath_NotSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"""C:foo\bar\baz.qux""").ToList();
            Assert.AreEqual(1, tokens.Count);
            CollectionAssert.AreEqual(new[] { @"C:foo\bar\baz.qux" }, tokens);
        }

        [TestMethod]
        public void Tokenize_RelativePath_NotSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"\bar\baz.qux").ToList();
            Assert.AreEqual(1, tokens.Count);
            CollectionAssert.AreEqual(new[] { @"\bar\baz.qux" }, tokens);
        }

        [TestMethod]
        public void Tokenize_PipeSeparatedWithoutSpace_PipeCollected()
        {
            var tokens = Tokenizer.Tokenize(@"foo -fooo|bar -baar").ToList();
            Assert.AreEqual(5, tokens.Count);
            CollectionAssert.AreEqual(new[] { "foo", "-fooo", "|", "bar", "-baar" }, tokens);
        }

        [TestMethod]
        public void Tokenize_PipeSeparatedWithSpace_PipeCollected()
        {
            var tokens = Tokenizer.Tokenize(@"foo -fooo | bar -baar").ToList();
            Assert.AreEqual(5, tokens.Count);
            CollectionAssert.AreEqual(new[] { "foo", "-fooo", "|", "bar", "-baar" }, tokens);
        }

        [TestMethod]
        public void Tokenize_CommaSeparated_CommaSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"-foo:1, 2, 3").ToList();
            Assert.AreEqual(4, tokens.Count);
            CollectionAssert.AreEqual(new[] { "-foo", "1", "2", "3" }, tokens);
        }

        //[TestMethod]
        //public void Tokenize_DashArgument_Separated()
        //{
        //    var tokens = Tokenizer.Tokenize(@"-foo -bar-baz").ToList();
        //    Assert.AreEqual(2, tokens.Count);
        //    CollectionAssert.AreEqual(new[] { "-foo", "bar-baz" }, tokens);
        //}
    }
}
