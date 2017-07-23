using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.CommandLine.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Data;
using Reusable.Logging.Loggex;

namespace Reusable.CommandLine.Commands
{
    [PublicAPI]
    [CommandNames("help", "h", "?")]
    [Description("Display help.")]
    public class HelpCommand : ICommand
    {
        public int IndentWidth { get; set; } = 4;

        public int[] ColumnWidths { get; set; } = { 17, 60 };

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {

            if (parameter is HelpCommandParameter commandParameter)
            {
                Execute(commandParameter);
            }
            else
            {
                throw new ArgumentException(
                    message: $"'{nameof(HelpCommandParameter)} expected but found '{parameter?.GetType()}'",
                    paramName: nameof(parameter));
            }

        }

        private void Execute(HelpCommandParameter parameter)
        {
            var commandName = ImmutableNameSet.Create(parameter.Command);

            if (commandName.Any())
            {
                if (parameter.Commands.TryGetValue(commandName, out var consoleCommand))
                {
                    commandName = parameter.Commands.Keys.Single(k => k.Overlaps(commandName));
                    RenderParameterList(consoleCommand, parameter.Logger);
                }
                else
                {
                    parameter.Logger.Log(e => e.Error().Message($"Command \"{commandName}\" not found."));
                }
            }
            else
            {
                RenderCommandList(parameter.Commands, parameter.Logger);
            }
            
        }

        protected virtual void RenderCommandList(CommandContainer commands, ILogger logger)
        {
            logger.Log(e => e.Message(string.Empty));
            logger.Log(e => e.Message($"{new string(' ', IndentWidth)}Commands"));

            var commandSummaries = commands.Select(x => new CommandSummary
            {
                Names = x.Key.OrderByDescending(n => n.Length),
                Description = x.Value.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description
            });

            var captions = new[] { "NAME", "ABOUT" };

            logger.Log(e => e.Message(string.Empty));
            logger.Log(e => e.Debug().Message(RenderColumns(captions, ColumnWidths)));
            logger.Log(e => e.Debug().Message(RenderColumns(captions.Select(h => new string('-', h.Length)), ColumnWidths)));

            foreach (var commandSummary in commandSummaries)
            {
                logger.Log(e => e.Message(RenderColumns(new[]
                {
                    commandSummary.Names.First(),
                    string.IsNullOrEmpty(commandSummary.Description) ? "N/A" : commandSummary.Description
                }, ColumnWidths)));
            }
            logger.Log(e => e.Message(string.Empty));
            logger.Log(e => e.Message(string.Empty));
        }

        protected virtual void RenderParameterList(IConsoleCommand command, ILogger logger)
        {
            var commandSummary = new CommandSummary
            {
                Names = command.Name.OrderByDescending(n => n.Length),
                Description = command.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description
            };

            var parameterSummaries = command.Parameter.Select(x => new ArgumentSummary
            {
                Names = x.Name.OrderByDescending(n => n.Length),
                Type = x.Property.PropertyType,
                Required = x.Required,
                Position = x.Position
            });


            var indent = new string(' ', IndentWidth);

            logger.Log(e => e.Message(string.Empty));
            logger.Log(e => e.Debug().Message("NAME"));
            logger.Log(e => e.Message(string.Join(Environment.NewLine, commandSummary.Names.Select(n => $"{indent}{n}"))));

            logger.Log(e => e.Message(string.Empty));
            logger.Log(e => e.Debug().Message("ABOUT"));
            logger.Log(e => e.Message($"{indent}{(string.IsNullOrEmpty(commandSummary.Description) ? "N/A" : commandSummary.Description)}"));

            logger.Log(e => e.Message(string.Empty));

            logger.Log(e => e.Debug().Message("SYNTAX"));
            logger.Log(e => e.Message(string.Empty));

            var positional =
                from p in parameterSummaries
                where p.Position > 0
                orderby p.Position
                let text = $"{p.Position}:{p.Names.First()}"
                select p.Required ? text.Required() : text.Optional();

            var required =
                from p in parameterSummaries
                where p.Required && p.Position < 1
                orderby p.Names.First()
                select $"{p.Names.First()}".Required();

            var optional =
                from p in parameterSummaries
                where !p.Required && p.Position < 1
                orderby p.Names.First()
                select $"{p.Names.First()}".Optional();

            logger.Log(e => e.Message($"{indent}{commandSummary.Names.First()} {string.Join(" ", positional.Concat(required).Concat(optional))}"));

            logger.Log(e => e.Message(string.Empty));
            logger.Log(e => e.Debug().Message("ARGUMENTS"));
            logger.Log(e => e.Message(string.Empty));

            foreach (var parameterSummary in parameterSummaries.OrderBy(p => p.Names.First()))
            {
                var name = parameterSummary.Names.First();
                var type = parameterSummary.Type.Name;
                logger.Log(e => e.Message(RenderColumns(new[] { $"{indent}{name}", type }, ColumnWidths)));
            }

            logger.Log(e => e.Message(string.Empty));
            logger.Log(e => e.Message(string.Empty));
        }

        private static string RenderColumns(IEnumerable<string> values, IEnumerable<int> columnWidths)
        {
            var result = new StringBuilder();
            foreach (var tuple in values.Zip(columnWidths, (x, w) => (Value: x, Width: w)))
            {
                result.Append(Pad(tuple.Value, tuple.Width));
            }

            return result.ToString();

            string Pad(string value, int width)
            {
                return
                    width < 0
                        ? value.PadLeft(-width, ' ')
                        : value.PadRight(width, ' ');
            }
        }

        private class CommandSummary
        {
            public IEnumerable<string> Names { get; set; }

            public string Description { get; set; }

            public bool IsDefault { get; set; }
        }

        private class ArgumentSummary
        {
            public IEnumerable<string> Names { get; set; }

            public Type Type { get; set; }

            public bool Required { get; set; }

            public int Position { get; set; }

            public string Description { get; set; }
        }
    }

    internal static class StringExtensions
    {
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
