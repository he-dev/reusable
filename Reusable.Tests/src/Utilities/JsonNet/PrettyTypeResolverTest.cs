using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;
using Reusable.Utilities.JsonNet;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Utilities.JsonNet
{
    [TestClass]
    public class PrettyTypeResolverTest
    {
        [TestMethod]
        public void CanValidateHasNoGenericArguments()
        {
            PrettyTypeResolver.Create(new[] { typeof(List<>) });
            Assert.That.Throws<DynamicException>(() => PrettyTypeResolver.Create(new[] { typeof(List<int>) }));
        }
    }
}