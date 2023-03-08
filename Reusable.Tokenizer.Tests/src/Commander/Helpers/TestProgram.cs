using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Reusable.DoubleDash;
using Reusable.DoubleDash.DependencyInjection;
using Reusable.OmniLog;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Commander.Helpers
{
    internal class TestProgram : IDisposable
    {
        private readonly ICommandExecutor _executor;
        private readonly IDisposable _disposer;

        private TestProgram(ICommandExecutor executor, IDisposable disposer)
        {
            _executor = executor;
            _disposer = disposer;
        }

        public static TestProgram Create(Action<ICommandRegistrationBuilder> build)
        {
            var container = InitializeContainer(build);
            var scope = container.BeginLifetimeScope();

            return new TestProgram(scope.Resolve<ICommandExecutor>(), Disposable.Create(() =>
            {
                scope.Dispose();
                container.Dispose();
            }));
        }

        public Task RunAsync(params string[] args) => _executor.ExecuteAsync<object>(args);
        
        public Task RunAsync<T>(IEnumerable<string> args, T context = default) => _executor.ExecuteAsync<object>(args, context);

        private static IContainer InitializeContainer(Action<ICommandRegistrationBuilder> build)
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(new LoggerFactory())
                .As<ILoggerFactory>();

            builder
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>));

            builder
                .RegisterModule(new CommandModule(build));

            //builder
            //    .RegisterInstance((ExecuteExceptionCallback)(ex =>
            //    {
            //        Fail(ex.Message);
            //    }));

            return builder.Build();
        }

        public void Dispose() => _disposer.Dispose();
    }
}