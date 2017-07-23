using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine.Tests.Commnads;

namespace Reusable.CommandLine.Tests
{
    [TestClass]
    public class CommandContainerTest
    {
        [TestMethod]
        public void Add_SameCommandTwice_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => CommandContainer.Empty.Add<FooCommand>().Add<FooCommand>());
        }
    }
}
