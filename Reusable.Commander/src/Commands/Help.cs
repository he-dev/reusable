using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Commander.Utilities;
using Reusable.Exceptionizer;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.Reflection;

namespace Reusable.Commander.Commands
{
    [PublicAPI]
    [UsedImplicitly]
    public class HelpBag : SimpleBag
    {
        [NotMapped]
        public int IndentWidth { get; set; } = 4;

        [NotMapped]
        public int[] ColumnWidths { get; set; } = {27, 50};

        [CanBeNull]
        //[DefaultValue(false)]
        [Description("Display command usage.")]
        //[Position(1)]
        [Alias("cmd")]
        //[Position(1)]
        public string Command { get; set; }

        internal bool HasCommand => Command.IsNotNullOrEmpty();
    }

    [PublicAPI]
    [Alias("h", "?")]
    [Description("Display help.")]
    public class Help : ConsoleCommand<HelpBag, object>
    {
        private readonly IList<Type> _commands;

        public Help(CommandServiceProvider<Help> serviceProvider, TypeList<IConsoleCommand> commands)
            : base(serviceProvider)
        {
            _commands = commands;
        }

        protected override Task ExecuteAsync(HelpBag parameter, object context, CancellationToken cancellationToken)
        {
            if (!parameter.HasCommand)
            {
                RenderCommandList(parameter);
            }
            else
            {
                RenderParameterList(parameter);
            }

            return Task.CompletedTask;
        }

        protected virtual void RenderCommandList(HelpBag parameter)
        {
            // Headers
            var captions = new[] {"NAME", "ABOUT"}.Pad(parameter.ColumnWidths);
            Logger.WriteLine(p => p.text(string.Join(string.Empty, captions)));

            // Separators
            var separators = captions.Select(c => new string('-', c.Trim().Length)).Pad(parameter.ColumnWidths);
            Logger.WriteLine(p => p.text(string.Join(string.Empty, separators)));

            // Commands
            var userExecutableCommands =
                from commandType in _commands
                where !commandType.IsDefined(typeof(InternalAttribute))
                orderby CommandHelper.GetCommandId(commandType).Default
                select commandType;

            foreach (var commandType in userExecutableCommands)
            {
                var defaultId = CommandHelper.GetCommandId(commandType).Default.ToString();
                var aliases = string.Join("|", CommandHelper.GetCommandId(commandType).Aliases.Select(x => x.ToString()));
                var description = commandType.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "N/A";
                var row = new[] {$"{defaultId} ({(aliases.Length > 0 ? aliases : "-")})", description}.Pad(parameter.ColumnWidths);
                Logger.WriteLine(p => p.text(string.Join(string.Empty, row)));
            }
        }

        protected virtual void RenderParameterList(HelpBag parameter)
        {
            var commandId = new Identifier(parameter.Command);
            var commandType = _commands.SingleOrDefault(t => CommandHelper.GetCommandId(t) == commandId);
            if (commandType is null)
            {
                throw DynamicException.Create(
                    $"CommandNotFound",
                    $"Could not find a command with the name '{parameter.Command}'"
                );
            }

            // Headers
            var captions = new[] {"NAME", "ABOUT"}.Pad(parameter.ColumnWidths);
            Logger.WriteLine(p => p.text(string.Join(string.Empty, captions)));

            // Separators
            var separators = captions.Select(c => new string('-', c.Trim().Length)).Pad(parameter.ColumnWidths);
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
                var row = new[] {$"{defaultId} ({(aliases.Length > 0 ? aliases : "-")})", description}.Pad(parameter.ColumnWidths);
                Logger.WriteLine(p => p.text(string.Join(string.Empty, row)));
            }
        }

        protected static IEnumerable<string> RenderRow(IEnumerable<string> values, IEnumerable<int> columnWidths)
        {
            foreach (var tuple in values.Zip(columnWidths, (x, w) => (Value: x, Width: w)))
            {
                yield return Pad(tuple.Value, tuple.Width);
            }

            string Pad(string value, int width)
            {
                return
                    width < 0
                        ? value.PadLeft(-width, ' ')
                        : value.PadRight(width, ' ');
            }
        }
    }

    internal static class HelpExtensions
    {
        public static IEnumerable<string> Pad(this IEnumerable<string> values, IEnumerable<int> columnWidths)
        {
            foreach (var tuple in values.Zip(columnWidths, (x, w) => (Value: x, Width: w)))
            {
                yield return Pad(tuple.Value, tuple.Width);
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