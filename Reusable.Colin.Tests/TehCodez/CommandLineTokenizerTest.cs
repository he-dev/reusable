using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Colin.Services;
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
            var tokens = string.Empty.Tokenize().ToList();
            tokens.Count.Verify().IsEqual(0);
        }

        [TestMethod]
        public void Tokenize_FileNameOnly_SingleToken()
        {
            var tokens = @"""C:\foo\bar baz\qux.baar""".Tokenize().ToList();
            tokens.Count.Verify().IsEqual(1);
            tokens[0].Verify().IsEqual(@"C:\foo\bar baz\qux.baar");
        }

        [TestMethod]
        public void Tokenize_FileNameWithArguments_MultipleTokens()
        {
            var tokens = @"""C:\foo\bar baz\qux.baar"" -foo -bar:baz -qux:""quux baar""".Tokenize(':').ToList();
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
            var tokens = @"foo.bar -baz -qux:""quux baar""|bar.baz -foo".Tokenize(':').ToList();
            tokens.Verify().SequenceEqual(new[] { "foo.bar", "-baz", "-qux", "quux baar", "|", "bar.baz", "-foo" });
        }
    }
}
