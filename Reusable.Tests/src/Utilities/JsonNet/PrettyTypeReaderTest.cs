using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Reusable.Extensions;
using Reusable.IO;
using Reusable.Utilities.JsonNet;

namespace Reusable.Tests.Utilities.JsonNet
{
    [TestClass]
    public class PrettyTypeReaderTest
    {
        private static readonly IFileProvider Resources = new RelativeFileProvider(EmbeddedFileProvider<PrettyTypeReaderTest>.Default, @"res\Utilities\JsonNet");

        [TestMethod]
        public void Deserialize_CanNonGenericType()
        {
            using (var jsonStream = Resources.GetFileInfo("PrettyTypeReaderT0.json").CreateReadStream())
            using (var streamReader = new StreamReader(jsonStream))
            using (var jsonTextReader = new PrettyTypeReader(streamReader, "$t", PrettyTypeResolver.Create(new[] { typeof(JsonTestClass0) })))
            {
                var jsonSerializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto };
                var testClass0 = jsonSerializer.Deserialize<JsonTestClass0>(jsonTextReader);

                Assert.IsNotNull(testClass0);
                Assert.IsNotNull(testClass0.Test0);
                Assert.IsNull(testClass0.Test1);
                Assert.IsNull(testClass0.Test2);
            }
        }

        [TestMethod]
        public void Deserialize_CanGenericType1()
        {
            using (var jsonStream = Resources.GetFileInfo("PrettyTypeReaderT1.json").CreateReadStream())
            using (var streamReader = new StreamReader(jsonStream))
            using (var jsonTextReader = new PrettyTypeReader(streamReader, "$t", PrettyTypeResolver.Create(new[] { typeof(JsonTestClass0), typeof(JsonTestClass1<>) })))
            {
                var jsonSerializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto };
                var testClass0 = jsonSerializer.Deserialize<JsonTestClass0>(jsonTextReader);

                Assert.IsNotNull(testClass0);
                Assert.IsNull(testClass0.Test0);
                Assert.IsNotNull(testClass0.Test1);
                Assert.IsNull(testClass0.Test2);
            }
        }

        [TestMethod]
        public void Deserialize_CanGenericType2()
        {
            using (var jsonStream = Resources.GetFileInfo("PrettyTypeReaderT2.json").CreateReadStream())
            using (var streamReader = new StreamReader(jsonStream))
            using (var jsonTextReader = new PrettyTypeReader(streamReader, "$t", PrettyTypeResolver.Create(new[] { typeof(JsonTestClass0) })))
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