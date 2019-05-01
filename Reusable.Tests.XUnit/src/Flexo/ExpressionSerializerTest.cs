using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.PeerToPeer;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
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
    [UsedImplicitly]
    public class ExpressionFixture : IDisposable
    {
        private static readonly IResourceProvider Flexo =
            EmbeddedFileProvider<ExpressionSerializerTest>
                .Default
                .DecorateWith(RelativeProvider.Factory(@"res\Flexo"));

        private readonly ILifetimeScope _scope;
        private readonly IDisposable _disposer;
        private readonly IExpressionSerializer _serializer;
        private readonly ConcurrentDictionary<string, IList<IExpression>> _expressions = new ConcurrentDictionary<string, IList<IExpression>>();

        public ExpressionFixture()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<JsonContractResolverModule>();
            builder.RegisterModule(new LoggerModule(new LoggerFactory()));
            builder.RegisterModule(new ExpressionSerializerModule(Expression.Types));

            var container = builder.Build();
            _scope = container.BeginLifetimeScope();
            _disposer = Disposable.Create(() =>
            {
                _scope.Dispose();
                container.Dispose();
            });

            _serializer = _scope.Resolve<ExpressionSerializer.Factory>()(TypeDictionary.From(Expression.Types));
        }


        public T Resolve<T>(Action<T> configure = default)
        {
            var t = _scope.Resolve<T>();
            configure?.Invoke(t);
            return t;
        }

        public IList<IExpression> GetReferences()
        {
            return _expressions.GetOrAdd("ExpressionReferences.json", ReadExpressionFile());
        }


        public IList<IExpression> GetExpressions()
        {
            return _expressions.GetOrAdd("ExpressionCollection.json", ReadExpressionFile());
        }

        private Func<string, IList<IExpression>> ReadExpressionFile()
        {
            return n =>
            {
                var json = Flexo.ReadTextFile(n);
                return _serializer.Deserialize<IList<IExpression>>(json);
            };
        }

        public void Dispose()
        {
            _disposer.Dispose();
        }
    }

    public class ExpressionSerializerTest : IDisposable, IClassFixture<ExpressionFixture>
    {
        private readonly ExpressionFixture _helper;
        private readonly ITestOutputHelper _output;

        private static readonly IResourceProvider Flexo =
            EmbeddedFileProvider<ExpressionSerializerTest>
                .Default
                .DecorateWith(RelativeProvider.Factory(@"res\Flexo"));

        private readonly IExpressionSerializer _serializer;

        private readonly IDisposable _container;

        public ExpressionSerializerTest(ExpressionFixture helper, ITestOutputHelper output)
        {
            _helper = helper;
            _output = output;
            var builder = new ContainerBuilder();

            builder.RegisterModule<JsonContractResolverModule>();
            builder.RegisterModule(new LoggerModule(new LoggerFactory()));
            builder.RegisterModule(new ExpressionSerializerModule(Expression.Types));

            var container = builder.Build();
            var scope = container.BeginLifetimeScope();

            _serializer = scope.Resolve<ExpressionSerializer.Factory>()(TypeDictionary.From(Expression.Types));
            _container = Disposable.Create(() =>
            {
                scope.Dispose();
                container.Dispose();
            });
        }

        [Theory]
        [SmartMemberData(nameof(GetData))]
        public void Can_evaluate_supported_expressions(string useCaseName, object expected, bool throws)
        {
            var useCase = _helper.GetExpressions().Get(useCaseName);
            ExpressionAssert.Equal(expected, useCase, ctx => ctx.WithReferences(_helper.GetReferences()), _output, throws);
        }

        [Fact]
        public async Task Can_deserialize_single_expression()
        {
            var jsonFile = await Flexo.GetFileAsync(@"ExpressionObject.json", MimeType.Json);
            using (var jsonStream = await jsonFile.CopyToMemoryStreamAsync())
            {
                var expression = await _serializer.DeserializeExpressionAsync(jsonStream.Rewind());
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

        public void Dispose()
        {
            _container.Dispose();
        }
    }

    internal static class EnumerableExtensions
    {
        public static IExpression Get(this IEnumerable<IExpression> expressions, SoftString name)
        {
            return expressions.Single(e => e.Name == name);
        }
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
                    if (!tmp.ParameterType.IsAssignableFrom(ip.PropertyType))
                    {
                        throw DynamicException.Create
                        (
                            $"DataItemParameterTypeMismatch",
                            $"Data item for '{GetTestMethodInfo()}' " +
                            $"cannot assign value of type '{ip.PropertyType.ToPrettyString()}' " +
                            $"to the parameter '{tmp.Name}' of type '{tmp.ParameterType.ToPrettyString()}'."
                        );
                    }
                    dataItem[i] = ip.GetValue(item);
                }
                else
                {                    
                    if (!tmp.IsOptional)
                    {
                        throw DynamicException.Create
                        (
                            $"DataItemParameterNotOptional",
                            $"Data item for '{GetTestMethodInfo()}' does not specify the required parameter '{tmp.Name}'"
                        );
                    }
                    else
                    {
                        dataItem[i] = tmp.DefaultValue;
                    }
                }
            }

            return dataItem;

            string GetTestMethodInfo() => $"{testMethod.DeclaringType.ToPrettyString()}.{testMethod.Name}";
        }
    }
}