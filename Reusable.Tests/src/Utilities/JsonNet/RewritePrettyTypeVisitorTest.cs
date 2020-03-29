using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Exceptionize;
using Reusable.Translucent;
using Reusable.Utilities.JsonNet.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;
using Reusable.Utilities.JsonNet.Services;
using Reusable.Utilities.JsonNet.TypeDictionaries;
using Reusable.Utilities.JsonNet.Visitors;
using Xunit;

namespace Reusable.Utilities.JsonNet
{
    public class RewritePrettyTypeVisitorTest : IClassFixture<TestHelperFixture>
    {
        private readonly TestHelperFixture _testHelper;

        private static readonly JsonSerializer JsonSerializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        private static readonly IJsonVisitor RewritePrettyTypeVisitor = new NormalizeTypeProperty(BuildInTypeDictionary.Default.Add(new CustomTypeDictionary
        (
            typeof(JsonTestClass0),
            typeof(JsonTestClass1<>),
            typeof(JsonTestClass2<,>)
        )));

        public RewritePrettyTypeVisitorTest(TestHelperFixture testHelper)
        {
            _testHelper = testHelper;
        }

        [Fact]
        public void Disallows_types_with_explicit_generic_arguments()
        {
            Assert.ThrowsAny<DynamicException>(() => new NormalizeTypeProperty(new CustomTypeDictionary(typeof(List<int>))));
        }

        [Fact]
        public void Can_rewrite_non_generic_type()
        {
            var json = _testHelper.Resource.ReadTextFile("PrettyType0.json");
            var testClass = RewritePrettyTypeVisitor.Visit(json).ToObject<JsonTestClass>(JsonSerializer);

            Assert.NotNull(testClass);
            Assert.NotNull(testClass.Test0);
            Assert.Null(testClass.Test1);
            Assert.Null(testClass.Test2);
        }

        [Fact]
        public void Can_rewrite_generic_type_with_one_argument()
        {
            var json = _testHelper.Resource.ReadTextFile("PrettyType1.json");
            var testClass = RewritePrettyTypeVisitor.Visit(json).ToObject<JsonTestClass>(JsonSerializer);

            Assert.NotNull(testClass);
            Assert.Null(testClass.Test0);
            Assert.NotNull(testClass.Test1);
            Assert.Null(testClass.Test2);
        }

        [Fact]
        public void Can_rewrite_generic_type_with_two_arguments()
        {
            var json = _testHelper.Resource.ReadTextFile("PrettyType2.json");
            var testClass = RewritePrettyTypeVisitor.Visit(json).ToObject<JsonTestClass>(JsonSerializer);

            Assert.NotNull(testClass);
            Assert.Null(testClass.Test0);
            Assert.Null(testClass.Test1);
            Assert.NotNull(testClass.Test2);
        }
    }

    [UsedImplicitly]
    [PublicAPI]
    [JsonTypeSchema("test")]
    internal class JsonTestClass
    {
        public IJsonTestInterface Test0 { get; set; }

        public IJsonTestInterface Test1 { get; set; }

        public IJsonTestInterface Test2 { get; set; }
    }

    internal interface IJsonTestInterface { }

    [JsonTypeSchema("test")]
    internal class JsonTestClass0 : IJsonTestInterface { }

    [JsonTypeSchema("test")]
    internal class JsonTestClass1<T1> : IJsonTestInterface { }

    [JsonTypeSchema("test")]
    internal class JsonTestClass2<T1, T2> : IJsonTestInterface { }
}