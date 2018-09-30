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
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.Reflection;

namespace Reusable.Commander.Commands
{
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
        public string Command { get; set; }

        internal bool HasCommand => Command.IsNotNullOrEmpty();
    }

    [PublicAPI]
    [Alias("h", "?")]
    [Description("Display help.")]
    public class Help : ConsoleCommand<HelpBag>
    {
        private readonly IList<Type> _commands;

        public Help(CommandServiceProvider<Help> serviceProvider, TypeList<IConsoleCommand> commands)
            : base(serviceProvider)
        {
            _commands = commands;
        }

        protected override Task ExecuteAsync(HelpBag parameter, CancellationToken cancellationToken)
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
            var command = _commands.SingleOrDefault(t => CommandHelper.GetCommandId(t) == commandId);
            if (command is null)
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
            
            

//            var commandParameterType = command.ParameterType();
//            
//            var commandSummary = new CommandSummary
//            {
//                Names = command.Name.OrderByDescending(n => n.Length),
//                Description = command.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description
//            };

            //var parameterSummaries = command.Parameter.Select(x => new ArgumentSummary
            //{
            //    Names = x.Name.OrderByDescending(n => n.Length),
            //    Type = x.Property.PropertyType,
            //    Required = x.Required,
            //    Position = x.Position
            //});


            //var indent = new string(' ', IndentWidth);

            //logger.Log(e => e.Message(string.Empty));
            //logger.Log(e => e.Debug().Message("NAME"));
            //logger.Log(e => e.Message(string.Join(Environment.NewLine, commandSummary.Names.Select(n => $"{indent}{n}"))));

            //logger.Log(e => e.Message(string.Empty));
            //logger.Log(e => e.Debug().Message("ABOUT"));
            //logger.Log(e => e.Message($"{indent}{(string.IsNullOrEmpty(commandSummary.Description) ? "N/A" : commandSummary.Description)}"));

            //logger.Log(e => e.Message(string.Empty));

            //logger.Log(e => e.Debug().Message("SYNTAX"));
            //logger.Log(e => e.Message(string.Empty));

            //var positional =
            //    from p in parameterSummaries
            //    where p.Position > 0
            //    orderby p.Position
            //    let text = $"{p.Position}:{p.Names.First()}"
            //    select p.Required ? text.Required() : text.Optional();

            //var required =
            //    from p in parameterSummaries
            //    where p.Required && p.Position < 1
            //    orderby p.Names.First()
            //    select $"{p.Names.First()}".Required();

            //var optional =
            //    from p in parameterSummaries
            //    where !p.Required && p.Position < 1
            //    orderby p.Names.First()
            //    select $"{p.Names.First()}".Optional();

            //logger.Log(e => e.Message($"{indent}{commandSummary.Names.First()} {string.Join(" ", positional.Concat(required).Concat(optional))}"));

            //logger.Log(e => e.Message(string.Empty));
            //logger.Log(e => e.Debug().Message("ARGUMENTS"));
            //logger.Log(e => e.Message(string.Empty));

            //foreach (var parameterSummary in parameterSummaries.OrderBy(p => p.Names.First()))
            //{
            //    var name = parameterSummary.Names.First();
            //    var type = parameterSummary.Type.Name;
            //    logger.Log(e => e.Message(RenderColumns(new[] { $"{indent}{name}", type }, ColumnWidths)));
            //}

            //logger.Log(e => e.Message(string.Empty));
            //logger.Log(e => e.Message(string.Empty));
        }

        private static IEnumerable<string> RenderRow(IEnumerable<string> values, IEnumerable<int> columnWidths)
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