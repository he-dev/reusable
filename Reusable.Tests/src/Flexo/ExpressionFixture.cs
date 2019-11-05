using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.Translucent;
using Reusable.Utilities.JsonNet.DependencyInjection;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    public class ExpressionFixture : IDisposable
    {
        private readonly ILifetimeScope _scope;
        private readonly IDisposable _disposer;
        private readonly ConcurrentDictionary<string, IList<IExpression>> _expressions = new ConcurrentDictionary<string, IList<IExpression>>();

        public ExpressionFixture()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<JsonContractResolverModule>();
            builder.RegisterModule(new LoggerModule(new LoggerFactory()));
            builder.RegisterModule(new ExpressionSerializerPackage(Enumerable.Empty<Type>()));

            var container = builder.Build();
            _scope = container.BeginLifetimeScope();
            _disposer = Disposable.Create(() =>
            {
                _scope.Dispose();
                container.Dispose();
            });

            Serializer = _scope.Resolve<ExpressionSerializer>();
        }

        public IExpressionSerializer Serializer { get; }

        public T Resolve<T>(Action<T> configure = default)
        {
            var t = _scope.Resolve<T>();
            configure?.Invoke(t);
            return t;
        }

        public IList<IExpression> References => _expressions.GetOrAdd("ExpressionReferences.json", ReadExpressionFile());

        public IList<IExpression> Expressions => _expressions.GetOrAdd("ExpressionCollection.json", ReadExpressionFile());

        private Func<string, IList<IExpression>> ReadExpressionFile()
        {
            return n =>
            {
                var json = TestHelper.Resources.ReadTextFile(n);
                return Serializer.Deserialize<IList<IExpression>>(json);
            };
        }

        public void Dispose()
        {
            _disposer.Dispose();
        }
    }
}