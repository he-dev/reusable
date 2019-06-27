using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Diagnostics;
using Reusable.Quickey;

namespace Reusable.Commander
{
    // foo -bar -baz qux
    public interface ICommandLine : IEnumerable<CommandArgument>
    {
        string Name { get; }
        
        bool Async { get; }
    }

    [UseMember]
    [PlainSelectorFormatter]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class CommandLine : ICommandLine
    {
        protected CommandLine(CommandLineDictionary arguments)
        {
            Reader = new CommandLineReader(arguments);
        }

        private string DebuggerDisplay => ToString();
        
        private ICommandLineReader Reader { get; }

        protected T GetArgument<T>(Expression<Func<T>> selector) => Reader.GetItem(selector);

        [Position(0)]
        public string Name => GetArgument(() => Name);
        
        public bool Async => GetArgument(() => Async);

        #region IEnumerable

        public IEnumerator<CommandArgument> GetEnumerator() => Reader.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
        
        public override string ToString() => this.Join(" ");

        public static implicit operator string(CommandLine commandLine) => commandLine?.ToString();
    }

    internal class DefaultCommandLine : CommandLine
    {
        public DefaultCommandLine(CommandLineDictionary arguments) : base(arguments) { }
    }
}