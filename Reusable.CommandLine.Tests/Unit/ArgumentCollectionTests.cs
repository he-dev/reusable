using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable RedundantAssignment
// ReSharper disable PossibleMultipleEnumeration

namespace Candle.Tests.Unit
{
    [TestClass]
    public class ArgumentCollectionTests
    {
        [TestMethod]
        public void ctor_CreatesEmptyCollection()
        {
            var arguments = new ArgumentCollection();

            arguments.Count.Verify().IsEqual(0);
            arguments.Anonymous.Verify().IsNull();
            arguments.CommandName.Verify().IsNullOrEmpty();

            var values = Enumerable.Empty<string>();
            arguments.TryGetValues(new[] { "foo" }, 1, out values).Verify().IsFalse();
            values.Verify().IsNotNull();
        }

        [TestMethod]
        public void Add_AnonymousArguments()
        {
            var arguments = new ArgumentCollection();
            arguments.Add(Argument.Anonymous, "foo");
            arguments.Add(Argument.Anonymous, "bar");
            arguments.Add(Argument.Anonymous, "baz");

            arguments.Count.Verify().IsEqual(1);

            arguments.Anonymous.Verify().IsNotNull();
            arguments.Anonymous.Count.Verify().IsEqual(3);

            arguments.CommandName.Verify().IsNotNullOrEmpty().IsEqual("foo");

            var values = Enumerable.Empty<string>();
            arguments.TryGetValues(new[] { "qux" }, 1, out values).Verify().IsTrue();
            values.Verify().IsNotNull();
            values.Single().Verify().IsEqual("bar");
        }

        [TestMethod]
        public void Add_NamedArgumentsWithSingleValue()
        {
            var arguments = new ArgumentCollection();
            arguments.Add(Argument.Anonymous, "foo");
            arguments.Add("bar", "a");
            arguments.Add("baz");

            arguments.Count.Verify().IsEqual(3);
            arguments.Anonymous.Verify().IsNotNull();
            arguments.Anonymous.Count.Verify().IsEqual(1);
            arguments.CommandName.Verify().IsNotNullOrEmpty().IsEqual("foo");

            var values = Enumerable.Empty<string>();
            arguments.TryGetValues(new[] { "qux" }, 1, out values).Verify().IsFalse();
            values.Verify().IsNotNull();
        }

        [TestMethod]
        public void Add_NamedArgumentWithMultipleValues()
        {
            var arguments = new ArgumentCollection();
            arguments.Add(Argument.Anonymous, "foo");
            arguments.Add("bar", "a");
            arguments.Add("bar", "b");
            arguments.Add("baz");

            arguments.Count.Verify().IsEqual(3);
            arguments.Anonymous.Verify().IsNotNull();
            arguments.Anonymous.Count.Verify().IsEqual(1);
            arguments.CommandName.Verify().IsNotNullOrEmpty().IsEqual("foo");

            var values = Enumerable.Empty<string>();
            arguments.TryGetValues(new[] { "qux" }, 1, out values).Verify().IsFalse();
            values.Verify().IsNotNull();

            arguments["baz"].Verify().IsNotNull();
            arguments["baz"].Count.Verify().IsEqual(0);
        }
    }
}


