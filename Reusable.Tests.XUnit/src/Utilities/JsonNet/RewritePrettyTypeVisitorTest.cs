using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
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
    public class RewritePrettyTypeVisitorTest
    {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        private static readonly IJsonVisitor RewritePrettyTypeVisitor =
            new RewritePrettyTypeVisitor(
                TypeDictionary
                    .BuiltInTypes
                    .AddRange(TypeDictionary.From(
                        typeof(JsonTestClass0),
                        typeof(JsonTestClass1<>),
                        typeof(JsonTestClass2<,>)
                    )));

        private static readonly IResourceProvider Files =
            EmbeddedFileProvider<RewritePrettyTypeVisitorTest>
                .Default
                .DecorateWith(RelativeProvider.Factory(@"res\Utilities\JsonNet"));

        [Fact]
        public void Disallows_types_with_explicit_generic_arguments()
        {
            Assert.ThrowsAny<DynamicException>(() => new RewritePrettyTypeVisitor(TypeDictionary.From(typeof(List<int>))));
        }

        [Fact]
        public void Can_rewrite_non_generic_type()
        {
            var json = Files.ReadTextFile("PrettyType0.json");
            var testClass = RewritePrettyTypeVisitor.Visit(json).ToObject<JsonTestClass>(JsonSerializer);

            Assert.NotNull(testClass);
            Assert.NotNull(testClass.Test0);
            Assert.Null(testClass.Test1);
            Assert.Null(testClass.Test2);
        }

        [Fact]
        public void Can_rewrite_generic_type_with_one_argument()
        {
            var json = Files.ReadTextFile("PrettyType1.json");
            var testClass = RewritePrettyTypeVisitor.Visit(json).ToObject<JsonTestClass>(JsonSerializer);

            Assert.NotNull(testClass);
            Assert.Null(testClass.Test0);
            Assert.NotNull(testClass.Test1);
            Assert.Null(testClass.Test2);
        }

        [Fact]
        public void Can_rewrite_generic_type_with_two_arguments()
        {
            var json = Files.ReadTextFile("PrettyType2.json");
            var testClass = RewritePrettyTypeVisitor.Visit(json).ToObject<JsonTestClass>(JsonSerializer);

            Assert.NotNull(testClass);
            Assert.Null(testClass.Test0);
            Assert.Null(testClass.Test1);
            Assert.NotNull(testClass.Test2);
        }
    }

    [UsedImplicitly]
    [PublicAPI]
    internal class JsonTestClass
    {
        public IJsonTestInterface Test0 { get; set; }

        public IJsonTestInterface Test1 { get; set; }

        public IJsonTestInterface Test2 { get; set; }
    }

    internal interface IJsonTestInterface { }

    internal class JsonTestClass0 : IJsonTestInterface { }

    internal class JsonTestClass1<T1> : IJsonTestInterface { }

    internal class JsonTestClass2<T1, T2> : IJsonTestInterface { }
}