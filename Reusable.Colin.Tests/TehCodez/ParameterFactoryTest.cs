using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Colin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Colin.Annotations;

namespace Reusable.Colin.Tests
{
    [TestClass]
    public class ParameterFactoryTest
    {
        [TestMethod]
        public void ctor_ParameterWithDefaultConstructor_CreatesInstance()
        {
            new ParameterFactory(typeof(Foo));
        }

        [TestMethod]
        public void ctor_UniqueParameter_CreatesInstance()
        {
            new ParameterFactory(typeof(UniqueParameters));
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicateParameterNameException))]
        public void ctor_DuplicateParameters_Throws()
        {
            new ParameterFactory(typeof(DuplicateParameters));
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