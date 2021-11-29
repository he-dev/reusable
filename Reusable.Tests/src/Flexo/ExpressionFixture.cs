using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Reusable.Flexo.Abstractions;
using Reusable.OmniLog;
using Reusable.Translucent;
using Reusable.Utilities.JsonNet.DependencyInjection;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    public class ExpressionFixture : IDisposable
    {
        private readonly ILifetimeScope _scope;
        private readonly IDisposable _disposer;

        public ExpressionFixture()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<ExpressionUseCase>().AsSelf();
            builder.RegisterModule<JsonContractResolverModule>();
            builder.RegisterModule(new LoggerModule(new LoggerFactory(Enumerable.Empty<ILoggerNode>())));
            builder.RegisterModule(new ExpressionSerializerModule(Enumerable.Empty<Type>()));

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

        //public IList<IExpression> Expressions => _expressions.GetOrAdd("ExpressionCollection.json", ReadExpressionFile<IList<IExpression>>);
        
        //public IList<IExpression> Packages => _expressions.GetOrAdd("Packages.IsPositive.json", fileName => new List<IExpression> { ReadExpressionFile<IExpression>(fileName) });

        public T ReadExpressionFile<T>(IResource resources, string fileName)
        {
            var json = resources.ReadTextFile(fileName);
            return Serializer.Deserialize<T>(json);
        }

        public void Dispose()
        {
            _disposer.Dispose();
        }
    }
}