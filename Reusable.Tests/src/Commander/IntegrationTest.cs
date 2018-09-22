using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;
using Reusable.OmniLog;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Reusable.Commander.Annotations;
using Reusable.IO;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;
using IContainer = Autofac.IContainer;

namespace Reusable.Tests.Commander
{
    [TestClass]
    public class IntegrationTest
    {
        private static Action<Bag1> _cmd1Assert;
        private static Action<Bag2> _cmd2Assert;

        [TestMethod]
        public async Task ExecuteAsync()
        {
            using (var container = InitializeContainer())
            using (var scope = container.BeginLifetimeScope())
            {
                var executor = scope.Resolve<ICommandLineExecutor>();

                //Assert.That.Throws<DynamicException>(
                //    () =>
                //    {
                //        executor.ExecuteAsync("cmd3").GetAwaiter().GetResult();
                //    },
                //    filter => filter.WhenName("CommandNotFoundException")
                //);

                _cmd1Assert = bag =>
                {
                    Assert.IsFalse(bag.Property01);
                    Assert.IsTrue(bag.Property02);
                    Assert.IsTrue(bag.Property03);
                    Assert.AreEqual("bar", bag.Property04);
                    Assert.That.Collection().AreEqual(new[] { 1, 2, 3 }, bag.Property05);
                };

                _cmd2Assert = bag =>
                {
                    Assert.AreEqual("baz", bag.Property04);
                };

                await executor.ExecuteAsync("cmd1 -p03:true -p04:bar -p05 1 2 3 | cmd2 -p04:baz");
            }
        }

        private static IContainer InitializeContainer()
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(new LoggerFactory())
                .As<ILoggerFactory>();

            builder
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>));

            builder
                .RegisterModule(new CommanderModule(new[]
                {
                    typeof(Command1),
                    typeof(Command2)
                }));

            return builder.Build();
        }

        [Alias("cmd1")]
        private class Command1 : ConsoleCommand<Bag1>
        {
            public Command1(ILogger<Command1> logger, ICommandLineMapper mapper)
                : base(logger, mapper)
            { }

            protected override Task ExecuteAsync(Bag1 parameter, CancellationToken cancellationToken)
            {
                _cmd1Assert(parameter);
                return Task.CompletedTask;
            }
        }

        private class Bag1 : CommandBag
        {
            [DefaultValue(false)]
            public bool Property01 { get; set; }

            [DefaultValue(true)]
            public bool Property02 { get; set; }

            [Alias("p03")]
            [DefaultValue(false)]
            public bool Property03 { get; set; }

            [Alias("p04")]
            [DefaultValue("foo")]
            public string Property04 { get; set; }

            [Alias("p05")]
            public IList<int> Property05 { get; set; }
        }


        [Alias("cmd2")]
        private class Command2 : ConsoleCommand<Bag2>
        {
            public Command2(ILogger<Command2> logger, ICommandLineMapper mapper)
                : base(logger, mapper)
            { }

            protected override Task ExecuteAsync(Bag2 parameter, CancellationToken cancellationToken)
            {
                _cmd2Assert(parameter);
                return Task.CompletedTask;
            }
        }

        private class Bag2 : CommandBag
        {
            [DefaultValue(false)]
            public bool Property01 { get; set; }

            [DefaultValue(true)]
            public bool Property02 { get; set; }

            [Alias("p03")]
            [DefaultValue(false)]
            public bool Property03 { get; set; }

            [Alias("p04")]
            [DefaultValue("foo")]
            public string Property04 { get; set; }

            [Alias("p05")]
            public IList<int> Property05 { get; set; }
        }
    }
}
