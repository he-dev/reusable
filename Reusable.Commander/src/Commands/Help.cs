using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Commander.Utilities;
using Reusable.Data.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;

namespace Reusable.Commander.Commands
{
    [PublicAPI]
    public interface IHelpArgumentGroup : ICommandArgumentGroup
    {
        [CanBeNull]
        [Description("Display command usage.")]
        //[Position(1)]
        [Tags("cmd")]
        //[Position(1)]
        string Command { get; set; }
    }

    [PublicAPI]
    [Tags("h", "?")]
    [Description("Display help.")]
    public class Help : Command<IHelpArgumentGroup>
    {
        private static readonly int IndentWidth = 4;

        private static readonly int[] ColumnWidths = { 27, 50 };

        private readonly IList<Type> _commandTypes;

        public Help(CommandServiceProvider<Help> serviceProvider, TypeList<ICommand> commands)
            : base(serviceProvider)
        {
            _commandTypes = commands;
        }        

        protected override Task ExecuteAsync(ICommandLineReader<IHelpArgumentGroup> parameter, NullContext context, CancellationToken cancellationToken)
        {
            var commandSelected = parameter.GetItem(x => x.Command).IsNotNullOrEmpty();
            if (commandSelected)
            {
                RenderParameterList(parameter);
            }
            else
            {
                RenderCommandList(parameter);
            }

            return Task.CompletedTask;
        }

        protected virtual void RenderCommandList(ICommandLineReader<IHelpArgumentGroup> parameter)
        {
            // Headers
            var captions = new[] { "NAME", "ABOUT" }.Pad(ColumnWidths);
            Logger.WriteLine(p => p.text(string.Join(string.Empty, captions)));

            // Separators
            var separators = captions.Select(c => new string('-', c.Trim().Length)).Pad(ColumnWidths);
            Logger.WriteLine(p => p.text(string.Join(string.Empty, separators)));

            // Commands
            var userExecutableCommands =
                from commandType in _commandTypes
                where !commandType.IsDefined(typeof(InternalAttribute))
                orderby CommandHelper.GetCommandId(commandType).Default
                select commandType;

            foreach (var commandType in userExecutableCommands)
            {
                var defaultId = CommandHelper.GetCommandId(commandType).Default.ToString();
                var aliases = string.Join("|", CommandHelper.GetCommandId(commandType).Aliases.Select(x => x.ToString()));
                var description = commandType.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "N/A";
                var row = new[] { $"{defaultId} ({(aliases.Length > 0 ? aliases : "-")})", description }.Pad(ColumnWidths);
                Logger.WriteLine(p => p.text(string.Join(string.Empty, row)));
            }
        }

        protected virtual void RenderParameterList(ICommandLineReader<IHelpArgumentGroup> parameter)
        {
            var commandId = new Identifier(new Name(parameter.GetItem(x => x.Command)));
            var commandType = _commandTypes.SingleOrDefault(t => CommandHelper.GetCommandId(t) == commandId);
            if (commandType is null)
            {
                throw DynamicException.Create
                (
                    $"CommandNotFound",
                    $"Could not find a command with the name '{parameter.GetItem(x => x.Command)}'"
                );
            }

            // Headers
            var captions = new[] { "NAME", "ABOUT" }.Pad(ColumnWidths);
            Logger.WriteLine(p => p.text(string.Join(string.Empty, captions)));

            // Separators
            var separators = captions.Select(c => new string('-', c.Trim().Length)).Pad(ColumnWidths);
            Logger.WriteLine(p => p.text(string.Join(string.Empty, separators)));

            var bagType = commandType.GetBagType();
            var parameters =
                from commandParameter in bagType.GetParameters()
                orderby commandParameter.Id.Default.ToString()
                select commandParameter;

            foreach (var commandParameter in parameters)
            {
                var defaultId = commandParameter.Id.Default.ToString();
                var aliases = string.Join("|", commandParameter.Id.Aliases.Select(x => x.ToString()));
                var description = commandParameter.Description ?? "N/A";
                var row = new[] { $"{defaultId} ({(aliases.Length > 0 ? aliases : "-")})", description }.Pad(ColumnWidths);
                Logger.WriteLine(p => p.text(string.Join(string.Empty, row)));
            }
        }
    }

    internal static class HelpExtensions
    {
        public static IEnumerable<string> Pad(this IEnumerable<string> values, IEnumerable<int> columnWidths)
        {
            foreach (var (value, width) in values.Zip(columnWidths, (x, w) => (Value: x, Width: w)))
            {
                yield return Pad(value, width);
            }

            string Pad(string value, int width)
            {
                return
                    width < 0
                        ? value.PadLeft(-width, ' ')
                        : value.PadRight(width, ' ');
            }
        }

        public static string Required(this string value) => $"<{value}>";

        public static string Optional(this string value) => $"[{value}]";
    }
}

/*
 
NAME                ABOUT
----
command1     Does this.
cmd1             N/A

---

NAME

    command1 
    cmd1
    
ABOUT

    Does this.

SYNTAX

    command1 <1:arg1> <2:arg2> <arg3> <arg3> [arg4] [arg5]

ARGUMENTS

    arg1         blah
    arg2         blah     
     
     
     */