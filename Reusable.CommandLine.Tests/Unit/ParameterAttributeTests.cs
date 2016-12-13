using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Candle.Tests.Unit
{
    [TestClass]
    public class ParameterAttributeTests
    {
        [TestMethod]
        public void ctor_ValidatesPosition()
        {
            new Action(() => new ParameterAttribute { Position = 0 }).Verify().Throws<ArgumentOutOfRangeException>();
            new Action(() => new ParameterAttribute { Position = 1 }).Verify().DoesNotThrow();
        }        
    }
}
