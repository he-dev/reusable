using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;
using Reusable.OmniLog;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
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
    public partial class IntegrationTest
    {
        private abstract class TestCommand<TBag> : ConsoleCommand<TBag> where TBag : ICommandBag, new()
        {
            private readonly IDictionary<Type, ICommandBag> _bags;

            protected TestCommand(ILogger logger, ICommandLineMapper mapper, IDictionary<Type, ICommandBag> bags)
                : base(logger, mapper)
            {
                _bags = bags;
            }

            protected override Task ExecuteAsync(TBag parameter, CancellationToken cancellationToken)
            {
                _bags.Add(typeof(TBag), parameter);
                return Task.CompletedTask;
            }
        }

        [Alias("cmd1")]
        private class Command1 : TestCommand<Bag1>
        {
            public Command1(ILogger<Command1> logger, ICommandLineMapper mapper, IDictionary<Type, ICommandBag> bags)
                : base(logger, mapper, bags)
            {
            }
        }

        private abstract class TestBag<TBag> : SimpleBag
        {
            [NotMapped]
            public Func<TBag, Task> Assert { get; set; }
        }

        private class Bag1 : TestBag<Bag1>
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
        private class Command2 : TestCommand<Bag2>
        {
            public Command2(ILogger<Command2> logger, ICommandLineMapper mapper, IDictionary<Type, ICommandBag> bags)
                : base(logger, mapper, bags)
            {
            }
        }

        private class Bag2 : SimpleBag
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