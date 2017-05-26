using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Data;
using Reusable.Colin.Data.Help;
using Reusable.Colin.Logging;
using Reusable.Colin.Services;

namespace Reusable.Colin.Commands
{
    [PublicAPI]
    [CommandName("help", "h", "?")]
    [Description("Display help.")]
    public class HelpCommand : ICommand
    {
        public int IndentWidth { get; set; } = 4;

        public int[] ColumnWidths { get; set; } = { 17, 60 };

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (!(parameter is CommandContext context))
            {
                throw new ArgumentException(
                    message: $"'{nameof(CommandContext)} expected but found '{parameter?.GetType()}'", 
                    paramName: nameof(parameter));
            }

            if (!(context.Parameter is HelpCommandParameter commandParameter))
            {
                throw new ArgumentException(
                    message: $"'{nameof(HelpCommandParameter)} expected but found '{context.Parameter?.GetType()}'",
                    paramName: nameof(parameter));
            }

            var commandName = commandParameter.Command;

            if (string.IsNullOrEmpty(commandName))
            {
                RenderCommandList(context);
            }
            else
            {
                if (context.CommandCollection.TryGetValue(ImmutableNameSet.Create(commandName), out Services.CommandMapping mapping))
                {
                    RenderParameterList(context, mapping);
                }
                else
                {
                    context.Logger.Error($"Command \"{commandName}\" not found.");
                }
            }
        }

        protected virtual void RenderCommandList(CommandContext context)
        {
            context.Logger.Info(string.Empty);
            context.Logger.Info($"{new string(' ', IndentWidth)}Commands");

            var commandSummaries = context.CommandCollection.Select(x => new CommandSummary
            {
                Names = x.Key.OrderByDescending(n => n.Length),
                Description = x.Value.Command.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description
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

        protected virtual void RenderParameterList(CommandContext context, CommandMapping mapping)
        {
            var commandSummary = new CommandSummary
            {
                Names = mapping.Name.OrderByDescending(n => n.Length),
                Description = mapping.Command.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description
            };

            var parameterSummaries = mapping.ParameterFactory.Select(x => new ParameterSummary
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
