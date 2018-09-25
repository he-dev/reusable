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
        private readonly IDictionary<Type, ICommandBag> _bags;

        public Command1(ILogger<Command1> logger, ICommandLineMapper mapper, IDictionary<Type, ICommandBag> bags)
            : base(logger, mapper, bags)
        {
            _bags = bags;
        }       
    }

    // Default values.
    internal class Bag1 : SimpleBag
    {        
        public bool Bool1 { get; set; }

        [DefaultValue(false)]
        public bool Bool2 { get; set; }

        [DefaultValue(true)]
        public bool Bool3 { get; set; }

        public string String1 { get; set; }
        
        [DefaultValue("foo")]
        public string String2 { get; set; }

        public int Int1 { get; set; }

        public int? Int2 { get; set; }

        [DefaultValue(3)]
        public int Int3 { get; set; }

        public DateTime DateTime1 { get; set; }
        
        public DateTime? DateTime2 { get; set; }

        [DefaultValue("2018/01/01")]
        public DateTime DateTime3 { get; set; }
            
        public IList<int> List1 { get; set; }        
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

    #region Invalid bags

    // Contains duplicate parameters.
    internal class InvalidBag1 : SimpleBag
    {
        
        public string A { get; set; }

        [Alias("A")]
        public string B { get; set; }
    }
    
    // Contains invalid parameter positions.
    internal class InvalidBag2 : SimpleBag
    {        
        [Position(1)]
        public string A { get; set; }

        [Position(3)]
        public string B { get; set; }
    }
    
    // Contains unsupported parameter type.
    internal class InvalidBag3 : SimpleBag
    {        
        public AppDomain A { get; set; }
    }

    #endregion
}