using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Tester;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander.Tests
{
    [TestClass]
    public class CommandLineTest
    {
        [TestMethod]
        public void Add_ArgumentName_ArgumentName()
        {
            var commandLine = new CommandLine
            {
                "foo"
            };

            Assert.That.Collection().CountEquals(1, commandLine);

            var commandArgument = commandLine.Single();

            Assert.AreEqual(SoftKeySet.Create("foo"), commandArgument.Key);
            Assert.That.Collection().CountEquals(0, commandArgument);
        }

        [TestMethod]
        public void Add_ArgumentWithValues_ArgumentWithValues()
        {
            var commandLine = new CommandLine
            {
                { "foo", "bar" },
                { "foo", "baz" },
            };

            Assert.That.Collection().CountEquals(1, commandLine);

            var commandArgument = commandLine.Single();

            Assert.AreEqual(SoftKeySet.Create("foo"), commandArgument.Key);
            Assert.That.Collection().CountEquals(2, commandArgument);
            Assert.AreEqual("bar", commandArgument.ElementAt(0));
            Assert.AreEqual("baz", commandArgument.ElementAt(1));
        }
    }
}