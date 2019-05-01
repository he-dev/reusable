using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flexo;
using Reusable.IOnymous;
using Reusable.OmniLog;
using Reusable.Utilities.JsonNet;
using Reusable.Utilities.JsonNet.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Double = Reusable.Flexo.Double;
using ExpressionSerializer = Reusable.Flexo.ExpressionSerializer;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    public class ExpressionSerializerTest : IClassFixture<ExpressionFixture>
    {
        private readonly ExpressionFixture _helper;
        private readonly ITestOutputHelper _output;

        private static readonly IResourceProvider Flexo =
            EmbeddedFileProvider<ExpressionSerializerTest>
                .Default
                .DecorateWith(RelativeProvider.Factory(@"res\Flexo"));


        public ExpressionSerializerTest(ExpressionFixture helper, ITestOutputHelper output)
        {
            _helper = helper;
            _output = output;
        }

        [Theory]
        [SmartMemberData(nameof(GetData))]
        public void Can_evaluate_supported_expressions(string useCaseName, object expected, bool throws)
        {
            var useCase = _helper.GetExpressions().Single(e => e.Name == useCaseName);
            ExpressionAssert.Equal(expected, useCase, ctx => ctx.WithReferences(_helper.GetReferences()), _output, throws);
        }

        [Fact]
        public async Task Can_deserialize_single_expression()
        {
            var jsonFile = await Flexo.GetFileAsync(@"ExpressionObject.json", MimeType.Json);
            using (var jsonStream = await jsonFile.CopyToMemoryStreamAsync())
            {
                var expression = await _helper.Serializer.DeserializeExpressionAsync(jsonStream.Rewind());
                Assert.NotNull(expression);
            }
        }

        public static IEnumerable<object> GetData() => new (string UseCaseName, object Expected, bool Throws)[]
        {
            ("Any", true, false),
            ("Any{Predicate=false}", true, false),
            ("All{Predicate=true}", false, false),
            ("Sum", 3.0, false),
            ("ToDouble", 1.0, false),
            ("Matches", true, false),
            ("True.Not", false, false),
            ("True.ToDouble", 1.0, false),
            ("Double.ToDouble", 1.0, true),
            ("Double.IsEqual", true, false),
            ("Double.ref_IsLessThan3", true, false),
            ("CollectionOfDouble", new double[] { 1, 2, 3 }, false),
            ("Collection.Sum", 3.0, false),
            ("Collection.Max", 4.0, false),
            ("Collection.Contains{Comparer=Regex}", true, false),
            ("Collection.Contains{Comparer=Regex}", true, false),
            ("Collection.Any{Predicate=Matches}", true, false),
            ("Collection.Contains", true, false),
            ("Collection.All", true, false),
            ("Collection.Overlaps{Comparer=SoftString}", true, false),
            ("Collection.Select.ToString", new[] { "1", "True" }, false),
        }.Select(uc => new { uc.UseCaseName, uc.Expected, uc.Throws });
    }

    [DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SmartMemberDataAttribute : MemberDataAttributeBase
    {
        public SmartMemberDataAttribute(string memberName, params object[] parameters) : base(memberName, parameters) { }

        protected override object[] ConvertDataItem(MethodInfo testMethod, object item)
        {
            var itemProperties = item.GetType().GetProperties().ToDictionary(p => p.Name, p => p, SoftString.Comparer);
            var testMethodParameters = testMethod.GetParameters();
            var dataItem = new object[testMethodParameters.Length];
            for (var i = 0; i < testMethodParameters.Length; i++)
            {
                var tmp = testMethodParameters[i];
                if (itemProperties.TryGetValue(tmp.Name, out var ip))
                {
                    if (tmp.ParameterType.IsAssignableFrom(ip.PropertyType))
                    {
                        dataItem[i] = ip.GetValue(item);
                    }
                    else
                    {
                        throw DynamicException.Create
                        (
                            $"DataItemParameterTypeMismatch",
                            $"Data item for '{GetTestMethodInfo()}' " +
                            $"cannot assign value of type '{ip.PropertyType.ToPrettyString()}' " +
                            $"to the parameter '{tmp.Name}' of type '{tmp.ParameterType.ToPrettyString()}'."
                        );
                    }
                }
                else
                {
                    if (tmp.IsOptional)
                    {
                        dataItem[i] = tmp.DefaultValue;
                    }
                    else
                    {
                        throw DynamicException.Create
                        (
                            $"DataItemParameterNotOptional",
                            $"Data item for '{GetTestMethodInfo()}' does not specify the required parameter '{tmp.Name}'"
                        );
                    }
                }
            }

            return dataItem;

            string GetTestMethodInfo() => $"{testMethod.DeclaringType.ToPrettyString()}.{testMethod.Name}";
        }
    }
}