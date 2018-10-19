using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Reusable.Extensions;
using Reusable.Utilities.JsonNet;

namespace Reusable.Tests.Utilities.JsonNet
{
    [TestClass]
    public class CustomTypeReaderTest
    {
        [TestMethod]
        public void Deserialize_CanResolveTypeByAlias()
        {
            var json =
                @"
{
    ""MyClass"": {
        ""-t"": ""MyClass2""
    }
}
";
            var types = Assembly.GetExecutingAssembly().GetTypes();
            using (var streamReader = json.ToStreamReader())
            using (var jsonTextReader = new CustomTypeReader(streamReader, "-t", TypeResolver.Create(typeof(CustomTypeReaderTest))))
            {
                var myClass1 = new JsonSerializer().Deserialize<MyClass1>(jsonTextReader);

                Assert.IsNotNull(myClass1);
                Assert.IsNotNull(myClass1.MyClass);
            }
        }
    }

    internal class MyClass1
    {
        public MyClass2 MyClass { get; set; }
    }

    internal class MyClass2
    {
    }
}