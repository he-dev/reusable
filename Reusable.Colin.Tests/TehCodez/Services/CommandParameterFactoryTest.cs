using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine.Annotations;
using Reusable.CommandLine.Services;

namespace Reusable.CommandLine.Tests.Services
{
    [TestClass]
    public class CommandParameterFactoryTest
    {
        [TestMethod]
        public void ctor_ParameterWithDefaultConstructor_CreatesInstance()
        {
            new CommandParameterFactory(typeof(Foo));
        }

        [TestMethod]
        public void ctor_UniqueParameter_CreatesInstance()
        {
            new CommandParameterFactory(typeof(UniqueParameters));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ctor_DuplicateParameters_Throws()
        {
            new CommandParameterFactory(typeof(DuplicateParameters));
        }

        private class Foo { }

        private class UniqueParameters
        {
            [Parameter]
            public string Foo { get; set; }

            [Parameter]
            public string Bar { get; set; }
        }

        private class DuplicateParameters
        {
            [Parameter]
            public string Foo { get; set; }

            [Parameter("Foo")]
            public string Bar { get; set; }
        }

        
    }
}