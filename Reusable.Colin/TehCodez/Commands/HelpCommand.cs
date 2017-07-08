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
using Reusable.CommandLine.Logging;
using Reusable.CommandLine.Services;

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
            //if (!(parameter is CommandContext context))
            //{
            //    throw new ArgumentException(
            //        message: $"'{nameof(CommandContext)} expected but found '{parameter?.GetType()}'", 
            //        paramName: nameof(parameter));
            //}

            if (!(context.Parameter is HelpCommandParameter commandParameter))
            {
                throw new ArgumentException(
                    message: $"'{nameof(HelpCommandParameter)} expected but found '{context.Parameter?.GetType()}'",
                    paramName: nameof(parameter));
            }

            var commandName = ImmutableNameSet.Create(commandParameter.Command);

            if (commandName.Any())
            {
                if (context.CommandContainer.TryGetValue(commandName, out var commandMetadata))
                {
                    commandName = context.CommandContainer.Keys.Single(k => k.Overlaps(commandName));
                    RenderParameterList(context, commandMetadata);
                }
                else
                {
                    context.Logger.Error($"Command \"{commandName}\" not found.");
                }
            }
            else
            {
                RenderCommandList(context);
            }
        }

        protected virtual void RenderCommandList(CommandContext context)
        {
            context.Logger.Info(string.Empty);
            context.Logger.Info($"{new string(' ', IndentWidth)}Commands");

            var commandSummaries = context.CommandContainer.Select(x => new CommandSummary
            {
                Names = x.Key.OrderByDescending(n => n.Length),
                Description = x.Value.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description
            });

            var captions = new[] { "NAME", "ABOUT" };

            context.Logger.Info(string.Empty);
            context.Logger.Debug(RenderColumns(captions, ColumnWidths));
            context.Logger.Debug(RenderColumns(captions.Select(h => new string('-', h.Length)), ColumnWidths));

            foreach (var commandSummary in commandSummaries)
            {
                context.Logger.Info(RenderColumns(new[]
                {
                    commandSummary.Names.First(),
                    string.IsNullOrEmpty(commandSummary.Description) ? "N/A" : commandSummary.Description
                }, ColumnWidths));
            }
            context.Logger.Info(string.Empty);
            context.Logger.Info(string.Empty);
        }

        protected virtual void RenderParameterList(CommandContext context, CommandMetadata commandMetadata)
        {
            var commandSummary = new CommandSummary
            {
                Names = commandMetadata.CommandName.OrderByDescending(n => n.Length),
                Description = commandMetadata.Command.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description
            };

            var parameterSummaries = commandMetadata.Parameter.Select(x => new ArgumentSummary
            {
                Names = x.Name.OrderByDescending(n => n.Length),
                Type = x.Property.PropertyType,
                Required = x.Required,
                Position = x.Position
            });


            var indent = new string(' ', IndentWidth);

            context.Logger.Info(string.Empty);
            context.Logger.Debug("NAME");
            context.Logger.Info(string.Join(Environment.NewLine, commandSummary.Names.Select(n => $"{indent}{n}")));

            context.Logger.Info(string.Empty);
            context.Logger.Debug("ABOUT");
            context.Logger.Info($"{indent}{(string.IsNullOrEmpty(commandSummary.Description) ? "N/A" : commandSummary.Description)}");

            context.Logger.Info(string.Empty);

            context.Logger.Debug("SYNTAX");
            context.Logger.Info(string.Empty);

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

            context.Logger.Info($"{indent}{commandSummary.Names.First()} {string.Join(" ", positional.Concat(required).Concat(optional))}");

            context.Logger.Info(string.Empty);
            context.Logger.Debug("ARGUMENTS");
            context.Logger.Info(string.Empty);

            foreach (var parameterSummary in parameterSummaries.OrderBy(p => p.Names.First()))
            {
                var name = parameterSummary.Names.First();
                var type = parameterSummary.Type.Name;
                context.Logger.Info(RenderColumns(new[] { $"{indent}{name}", type }, ColumnWidths));
            }

            context.Logger.Info(string.Empty);
            context.Logger.Info(string.Empty);
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
