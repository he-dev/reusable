using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine.Services;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.TestTools.UnitTesting.AssertExtensions.TehCodez;

namespace Reusable.CommandLine.Tests.Services
{
    [TestClass]
    public class CommandLineTokenizerTest
    {
        private static readonly ICommandLineTokenizer Tokenizer = new CommandLineTokenizer();

        [TestMethod]
        public void Tokenize_Empty_Empty()
        {
            var tokens = Tokenizer.Tokenize(string.Empty).ToList();
            Assert.That.IsEmpty(tokens);
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
        public void Tokenize_PipeSeparated_PipeCollected()
        {
            var tokens = Tokenizer.Tokenize(@"-foo|-baz").ToList();
            Assert.AreEqual(3, tokens.Count);
            CollectionAssert.AreEqual(new[] { "-foo", "|", "-baz" }, tokens);
        }

        [TestMethod]
        public void Tokenize_CommaSeparated_CommaSeparated()
        {
            var tokens = Tokenizer.Tokenize(@"-foo:1, 2, 3").ToList();
            Assert.AreEqual(4, tokens.Count);
            CollectionAssert.AreEqual(new[] { "-foo", "1", "2", "3" }, tokens);
        }
    }
}
