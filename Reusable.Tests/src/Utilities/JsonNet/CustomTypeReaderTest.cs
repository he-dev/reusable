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
    ""Test0"": {
        ""$t"": ""JsonTestClass0""
    },
    ""Test1"": {
        ""$t"": ""JsonTestClass1<int>""
    },
    ""Test2"": {
        ""$t"": ""JsonTestClass2<int, string>""
    }
}
";
            using (var streamReader = json.ToStreamReader())
            using (var jsonTextReader = new PrettyTypeReader(streamReader, typeof(CustomTypeReaderTest)))
            {
                var jsonSerializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto };
                var testClass0 = jsonSerializer.Deserialize<JsonTestClass0>(jsonTextReader);

                Assert.IsNotNull(testClass0);
                Assert.IsNotNull(testClass0.Test0);
                Assert.IsNotNull(testClass0.Test1);
                Assert.IsNotNull(testClass0.Test2);
            }
        }
    }

    internal class JsonTestClass0
    {
        public JsonTestClass0 Test0 { get; set; }

        public TestInterface Test1 { get; set; }

        public TestInterface Test2 { get; set; }
    }

    internal interface TestInterface
    {
    }

    internal class JsonTestClass1<T1> : TestInterface
    {
    }

    internal class JsonTestClass2<T1, T2> : TestInterface
    {
    }
}