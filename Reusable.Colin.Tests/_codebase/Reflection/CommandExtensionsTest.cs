using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Shelly.Reflection;

namespace Reusable.Shelly.Tests.Reflection
{
    [TestClass]
    public class CommandExtensionsTest
    {
        [TestMethod]
        public void GetCommandNames_WithoutNamespace()
        {
            typeof(Test1Command).GetCommandNames().Verify("CommandNames").SequenceEqual(new[] { "Test1", "t1" });
        }

        [TestMethod]
        public void GetCommandNames_WithNamespace()
        {
            typeof(Test2Command).GetCommandNames().Verify("CommandNames").SequenceEqual(new[]
            {
                "Foo.Bar.Test2",
                "Foo.Bar.t2",
                "Foo.Bar.two",
                "Test2",
                "t2",
                "two"
            });
        }

        [TestMethod]
        public void ValidateCommandPropertyNames_Unique()
        {
            typeof(Test1Command).ValidateCommandPropertyNames();
        }

        [TestMethod]
        public void ValidateCommandPropertyNames_NonUnique()
        {
            new Action(() =>
            {
                typeof(Test3Command).ValidateCommandPropertyNames();
            })
            .Verify().Throws<ArgumentException>();
        }

        [TestMethod]
        public void GetCommandProperties_WithAndWitoutAlias()
        {
            var commandProperties = typeof(Test1Command).GetCommandProperties().ToList();
            commandProperties.Count.Verify().IsEqual(2);
            var foo = commandProperties.ElementAt(0);
            foo.Names.Verify().SequenceEqual(new[] { "Foo", "fu" });
            var baz = commandProperties.ElementAt(1);
            baz.Names.Verify().SequenceEqual(new[] { "Baz" });
        }

        [TestMethod]
        public void GetDescription_String()
        {
            typeof(Test1Command).GetDescription().Verify().IsEqual("Abc");
        }

        [TestMethod]
        public void GetDescription_NullOrEmpty()
        {
            typeof(Test2Command).GetDescription().Verify().IsNullOrEmpty();
        }

        [Alias("t1")]
        [System.ComponentModel.Description("Abc")]
        private class Test1Command : Command
        {
            [Parameter]
            [Alias("fu")]
            public string Foo { get; set; }

            public int Bar { get; set; }

            [Parameter]
            public DateTime Baz { get; set; }

            public override void Execute()
            {
                throw new NotImplementedException();
            }
        }

        [Namespace("Foo.Bar")]
        [Alias("t2", "two")]
        private class Test2Command : Command
        {
            public override void Execute()
            {
                throw new NotImplementedException();
            }
        }

        private class Test3Command : Command
        {
            [Parameter]
            public string Foo { get; set; }

            [Parameter]
            [Alias("Foo")]
            public string Bar { get; set; }

            public override void Execute()
            {
                throw new NotImplementedException();
            }
        }
    }
}
