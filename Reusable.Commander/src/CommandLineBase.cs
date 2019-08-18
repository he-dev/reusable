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
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.Commander
{
    // foo -bar -baz qux
    public interface ICommandLine : IEnumerable<CommandArgument>
    {
        string Name { get; }

        bool Async { get; }
    }

    [UsedImplicitly]
    [UseMember]
    [PlainSelectorFormatter]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    // Using 'Base' suffix here so that implementers can use 'CommandLine'.
    public class CommandLineBase : ICommandLine
    {
        public CommandLineBase(CommandLineDictionary arguments)
        {
            Reader = new CommandLineReader(arguments);
        }

        private string DebuggerDisplay => ToString();

        private ICommandLineReader Reader { get; }

        protected T GetArgument<T>(Expression<Func<T>> selector) => Reader.GetItem(selector);

        [Position(0)]
        public string Name => GetArgument(() => Name);

        public bool Async => GetArgument(() => Async);

        [CanBeNull]
        public static TCommandLine Create<TCommandLine>(object argument)
        {
            if (argument is TCommandLine commandLine)
            {
                return commandLine;
            }

            var commandLineCtor =
                typeof(TCommandLine).GetConstructor(new[] { typeof(CommandLineDictionary) }) ??
                throw new ArgumentException($"{typeof(TCommandLine).ToPrettyString()} must have the following constructor: ctor{nameof(CommandLineDictionary)}");

            return
                argument is CommandLineDictionary arguments
                    ? (TCommandLine)commandLineCtor.Invoke(new object[] { arguments })
                    : default;
        }

        #region IEnumerable

        public IEnumerator<CommandArgument> GetEnumerator() => Reader.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        public override string ToString() => this.Join(" ");

        public static implicit operator string(CommandLineBase commandLine) => commandLine?.ToString();
    }

    public static class CommandLineExtensions
    {
        public static bool ContainsArguments(this ICommandLine commandLine) => commandLine.Skip(1).Any();
    }

//    internal class DefaultCommandLine : CommandLine
//    {
//        public DefaultCommandLine(CommandLineDictionary arguments) : base(arguments) { }
//    }
}