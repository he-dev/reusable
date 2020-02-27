using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Commander.Utilities;
using Reusable.Data.Annotations;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Helpers;
using Reusable.OmniLog.Utilities;
using t = Reusable.Commander.ConsoleTemplates;

namespace Reusable.Commander.Commands
{
    [Alias("v")]
    [Description("Display version.")]
    public class Version : Command<CommandParameter>
    {
        private readonly ILogger<Version> _logger;
        private readonly string _version;

        public Version(ILogger<Version> logger, string version)
        {
            _logger = logger;
            _version = version;
        }

        public ConsoleStyle Style { get; set; }

        protected override Task ExecuteAsync(CommandParameter parameter, CancellationToken cancellationToken)
        {
            _logger.WriteLine(Style, new t.Indent(1), new t.Info { Text = _version });

            return Task.CompletedTask;
        }
    }

    [PublicAPI]
    [UsedImplicitly]
    [Alias("h", "?")]
    [Description("Display help.")]
    public class Help : Command<Help.Parameter>
    {
        private static readonly int IndentWidth = 4;

        private static readonly int[] ColumnWidths = { 27, 50 };

        public Help(ILogger<Help> logger) => Logger = logger;

        private ILogger Logger { get; }

        public ConsoleStyle Style { get; set; }

        protected override Task ExecuteAsync(Parameter parameter, CancellationToken cancellationToken)
        {
            if (parameter.Command.IsNullOrEmpty())
            {
                RenderCommandList(parameter);
            }
            else
            {
                RenderParameterList(parameter);
            }

            return Task.CompletedTask;
        }

        protected virtual void RenderCommandList(Parameter parameter)
        {
            // Headers
            var captions = new[] { "Command", "Description" }.Pad(ColumnWidths);
            Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = captions });

            // Separators
            var separators = captions.Select(c => new string('-', c.Trim().Length)).Pad(ColumnWidths);
            Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = separators });

            // Commands
            var userExecutableCommands =
                from command in parameter.Commands
                where !command.CommandType.IsDefined(typeof(InternalAttribute))
                orderby command.Name.Primary
                select command;

            foreach (var command in userExecutableCommands)
            {
                var defaultId = command.Name.Primary;
                var aliases = string.Join("|", command.Name.Secondary.Select(x => x.ToString()));
                var description = command.CommandType.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "N/A";
                var row = new[] { $"{defaultId} ({(aliases.Length > 0 ? aliases : "-")})", description }.Pad(ColumnWidths);
                Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = row });
            }
        }

        protected virtual void RenderParameterList(Parameter parameter)
        {
            var command =
                parameter
                    .Commands
                    .Where(c => c.Name.Contains(parameter.Command, SoftString.Comparer))
                    .SingleOrThrow(onEmpty: ($"CommandNotFound", $"Could not find a command with the name '{parameter.Command}'"));

            // Headers
            var captions = new[] { "Option", "Description" }.Pad(ColumnWidths).ToList();
            Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = captions });

            // Separators
            var separators = captions.Select(c => new string('-', c.Trim().Length)).Pad(ColumnWidths);
            Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = separators });

            var commandParameterProperties =
                from commandArgument in command.ParameterType.GetParameterProperties()
                orderby commandArgument.Name.First()
                select commandArgument;

            foreach (var commandParameterProperty in commandParameterProperties)
            {
                var name = commandParameterProperty.GetArgumentName();
                var defaultId = name.First();
                var aliases = name.Skip(1).OrderByDescending(a => a.Length).Join("|");
                var description = commandParameterProperty.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "N/A";
                var row = new[] { $"{defaultId} ({(aliases.Length > 0 ? aliases : "-")})", description }.Pad(ColumnWidths);
                Logger.WriteLine(Style, new t.Indent(1), new t.Help.TableRow { Cells = row });
            }
        }

        [PublicAPI]
        public class Parameter : CommandParameter
        {
            [Description("Display command usage.")]
            [Alias("cmd")]
            [Position(1)]
            public string? Command { get; set; }

            //[Service]
            //public TypeList<ICommand> Commands { get; set; }
            [Service]
            public IEnumerable<CommandInfo> Commands { get; set; }
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