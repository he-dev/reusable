using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.OmniLog;

namespace Reusable.Tests.Commander.IntegrationTests
{
    internal abstract class TestCommand<TBag> : ConsoleCommand<TBag> where TBag : ICommandBag, new()
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
    internal class Command1 : TestCommand<Bag1>
    {
        public Command1(ILogger<Command1> logger, ICommandLineMapper mapper, IDictionary<Type, ICommandBag> bags)
            : base(logger, mapper, bags)
        {
        }
    }

    internal abstract class TestBag<TBag> : SimpleBag
    {
        [NotMapped]
        public Func<TBag, Task> Assert { get; set; }
    }

    internal class Bag1 : TestBag<Bag1>
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
    internal class Command2 : TestCommand<Bag2>
    {
        public Command2(ILogger<Command2> logger, ICommandLineMapper mapper, IDictionary<Type, ICommandBag> bags)
            : base(logger, mapper, bags)
        {
        }
    }

    internal class Bag2 : SimpleBag
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