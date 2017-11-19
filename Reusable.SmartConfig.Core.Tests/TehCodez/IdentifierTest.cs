using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Tests.Common;
using Reusable.SmartConfig.Tests.Common.Configurations;

namespace Reusable.SmartConfig.Tests
{
    [TestClass]
    public class IdentifierTest
    {
        [TestMethod]
        public void Create_Container_Consumer()
        {
            Assert.AreEqual("Empty", Identifier.Create<EmptyConfiguration>().ToString());
        }

        [TestMethod]
        public void Create_ConsumerContainer_ConsumerContainer()
        {
            Assert.AreEqual("TestConsumer.Empty", Identifier.Create<TestConsumer, EmptyConfiguration>().ToString());
        }

        [TestMethod]
        public void Load_ConsumerInstanceContainer_ConsumerInstanceContainer()
        {
            var consumer = new TestConsumer { Name = "qux" };
            Assert.AreEqual("TestConsumer[qux].Empty", Identifier.Create<TestConsumer, EmptyConfiguration>(consumer, c => c.Name).ToString());
        }

        [TestMethod]
        public void Parse_Container_Identifier()
        {
            Assert.AreEqual(
                Identifier.Create(Token.Literal("foo")), 
                Identifier.Parse("foo"));
        }

        [TestMethod]
        public void Parse_ConsumerContainerElement_Identifier()
        {
            Assert.AreEqual(
                Identifier.Create(Token.Literal("foo"), Token.Literal("bar"), Token.Element("baz")),
                Identifier.Parse("Foo.Bar[Baz]"));
        }

        [TestMethod]
        public void Parse_ConsumerInstanceContainerElement_Identifier()
        {
            Assert.AreEqual(
                Identifier.Create(Token.Literal("foo"), Token.Element("qux"), Token.Literal("bar"), Token.Element("baz")),
                Identifier.Parse("Foo[qux].Bar[Baz]"));
        }

        [TestMethod]
        public void ToString_Container_String()
        {
            Assert.AreEqual("bar", Identifier.Create(Token.Literal("bar")).ToString());
        }

        [TestMethod]
        public void ToString_ContainerElement_String()
        {
            Assert.AreEqual("bar[baz]", Identifier.Create(Token.Literal("bar"), Token.Element("baz")).ToString());
        }

        [TestMethod]
        public void ToString_ConsumerContainerElement_String()
        {
            Assert.AreEqual("foo.bar[baz]", Identifier.Create(Token.Literal("foo"), Token.Literal("bar"), Token.Element("baz")).ToString());
        }

        [TestMethod]
        public void ToString_ConsumerInstanceContainerElement_String()
        {
            Assert.AreEqual("foo[qux].bar[baz]", Identifier.Create(Token.Literal("foo"), Token.Element("qux"), Token.Literal("bar"), Token.Element("baz")).ToString());
        }

        [TestMethod]
        public void ToString_SameConsequtiveLiterals_Collapsed()
        {
            Assert.AreEqual(
                "foo.bar[baz]", 
                Identifier.Create(
                    Token.Literal("foo"), 
                    Token.Literal("foo"),
                    Token.Literal("bar"), 
                    Token.Element("baz")
                ).ToString()
            );
        }

        [TestMethod]
        public void Equals_SameProperties_True()
        {
            var identifier1 = Identifier.Parse("foo.bar[baz]");
            var identifier2 = Identifier.Parse("foo.bar[baz]");
            Assert.AreEqual(identifier1, identifier2);
            Assert.AreNotSame(identifier1, identifier2);
        }

        [TestMethod]
        public void Equals_DifferentProperties_False()
        {
            var identifier1 = Identifier.Parse("foo.bar[baz]");
            var identifier2 = Identifier.Parse("foo[qux].bar[baz]");
            Assert.AreNotEqual(identifier1, identifier2);
            Assert.AreNotSame(identifier1, identifier2);
        }

    }
}
