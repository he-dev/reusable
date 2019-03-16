using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.Reflection;
using Reusable.Utilities.JsonNet;
using Xunit;

namespace Reusable.Tests.Utilities.JsonNet
{
    public class PrettyTypeResolverTest
    {
        private static readonly IResourceProvider Resources = new RelativeProvider(EmbeddedFileProvider<PrettyTypeResolverTest>.Default, @"res\Utilities\JsonNet");

        [Fact]
        public void AllowsTypesWithoutGenericArguments()
        {
            new RewritePrettyTypeVisitor(TypeDictionary.From(typeof(List<>)));
        }

        [Fact]
        public void DisallowsTypesWithExplicitGenericArguments()
        {
            Assert.ThrowsAny<DynamicException>(() => new RewritePrettyTypeVisitor(TypeDictionary.From(typeof(List<int>))));
        }

        [Fact]
        public void Deserialize_CanNonGenericType()
        {
            using (var jsonStream = Resources.ReadTextFile("PrettyTypeReaderT0.json").ToStreamReader().BaseStream)
            using (var streamReader = new StreamReader(jsonStream))
            {
                var json = streamReader.ReadToEnd();
                var visit = new RewritePrettyTypeVisitor(TypeDictionary.From(typeof(JsonTestClass0)));

                var jsonSerializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto };
                var testClass0 = visit.Visit(json).ToObject<JsonTestClass0>(jsonSerializer);

                Assert.NotNull(testClass0);
                Assert.NotNull(testClass0.Test0);
                Assert.Null(testClass0.Test1);
                Assert.Null(testClass0.Test2);
            }
        }

        [Fact]
        public void Deserialize_CanGenericType1()
        {
            using (var jsonStream = Resources.ReadTextFile("PrettyTypeReaderT1.json").ToStreamReader().BaseStream)
            using (var streamReader = new StreamReader(jsonStream))
                //using (var jsonTextReader = new PrettyTypeReader(streamReader, "$t", PrettyTypeResolver.Create(new[] { typeof(JsonTestClass0), typeof(JsonTestClass1<>) })))
            {
                var json = streamReader.ReadToEnd();
                var prettyTypeResolver = new RewritePrettyTypeVisitor(TypeDictionary.BuiltInTypes.AddRange(TypeDictionary.From(typeof(JsonTestClass1<>))));

                var jsonSerializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto };
                var testClass0 = prettyTypeResolver.Visit(json).ToObject<JsonTestClass0>(jsonSerializer);

                Assert.NotNull(testClass0);
                Assert.Null(testClass0.Test0);
                Assert.NotNull(testClass0.Test1);
                Assert.Null(testClass0.Test2);
            }
        }

        [Fact]
        public void Deserialize_CanGenericType2()
        {
            using (var jsonStream = Resources.ReadTextFile("PrettyTypeReaderT2.json").ToStreamReader().BaseStream)
            using (var streamReader = new StreamReader(jsonStream))
                //using (var jsonTextReader = new PrettyTypeReader(streamReader, "$t", PrettyTypeResolver.Create(new[] { typeof(JsonTestClass0) })))
            {
                var json = streamReader.ReadToEnd();
                var visit = new RewritePrettyTypeVisitor(TypeDictionary.BuiltInTypes.AddRange(TypeDictionary.From(typeof(JsonTestClass2<,>))));

                var jsonSerializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto };
                var testClass0 = visit.Visit(json).ToObject<JsonTestClass0>(jsonSerializer);

                Assert.NotNull(testClass0);
                Assert.Null(testClass0.Test0);
                Assert.Null(testClass0.Test1);
                Assert.NotNull(testClass0.Test2);
            }
        }
    }

    internal class JsonTestClass0
    {
        public JsonTestClass0 Test0 { get; set; }

        public TestInterface Test1 { get; set; }

        public TestInterface Test2 { get; set; }
    }

    internal interface TestInterface { }

    internal class JsonTestClass1<T1> : TestInterface { }

    internal class JsonTestClass2<T1, T2> : TestInterface { }
}