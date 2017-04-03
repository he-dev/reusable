using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse.Testing;
using Reusable.Fuse;

namespace Reusable.Colin.Tests
{
    [TestClass]
    public class CommandLineTokenizerTest
    {
        [TestMethod]
        public void Tokenize_Empty_EmptyTokens()
        {
            var tokens = CommandLineTokenizer.Tokenize(string.Empty);
            tokens.Count.Verify().IsEqual(0);
        }

        [TestMethod]
        public void Tokenize_FileNameOnly_SingleToken()
        {
            var tokens = CommandLineTokenizer.Tokenize(@"""C:\foo\bar baz\qux.baar""");
            tokens.Count.Verify().IsEqual(1);
            tokens[0].Verify().IsEqual(@"C:\foo\bar baz\qux.baar");
        }

        [TestMethod]
        public void Tokenize_FileNameWithArguments_MultipleTokens()
        {
            var tokens = CommandLineTokenizer.Tokenize(@"""C:\foo\bar baz\qux.baar"" -foo -bar:baz -qux:""quux baar""", ':');
            tokens.Count.Verify().IsEqual(6);
            tokens[0].Verify().IsEqual(@"C:\foo\bar baz\qux.baar");
            tokens[1].Verify().IsEqual(@"-foo");
            tokens[2].Verify().IsEqual(@"-bar");
            tokens[3].Verify().IsEqual(@"baz");
            tokens[4].Verify().IsEqual(@"-qux");
            tokens[5].Verify().IsEqual(@"quux baar");
        }

        [TestMethod]
        public void Tokenize_CommandLineWithPipe_MultipleTokensSplittedOnPipe()
        {
            var tokens = CommandLineTokenizer.Tokenize(@"foo.bar -baz -qux:""quux baar""|bar.baz -foo", ':');
            tokens.Verify().SequenceEqual(new[] { "foo.bar", "-baz", "-qux", "quux baar", "|", "bar.baz", "-foo" });
        }
    }
}
