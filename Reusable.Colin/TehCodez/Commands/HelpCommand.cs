using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Data;
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

        public int NameColumnWidth { get; set; } = 30;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (!(parameter is CommandContext context))
            {
                throw new ArgumentException(message: $"'{nameof(CommandContext)} expected but found '{parameter?.GetType()}'", paramName: nameof(parameter));
            }

            // ReSharper disable once PossibleNullReferenceException
            var commandName = ((HelpCommandParameter)context.Parameter).Command;

            if (string.IsNullOrEmpty(commandName))
            {
                RenderCommandList(context.CommandCollection.Select(x => new CommandSummary
                {
                    Names = x.Key,
                    //IsDefault = context.CommandLine.
                    Description = x.Value.Command.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description
                }), context.Logger);
            }
            else
            {
                // Write argument list for the command.
                if (context.CommandCollection.TryGetValue(ImmutableNameSet.Create(commandName), out Services.CommandMapping mapping))
                {
                    var commandSummary = new CommandSummary
                    {
                        Names = mapping.Name,
                        Description = mapping.Command.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description
                    };

                    var parameterSummaries = mapping.ParameterFactory.Select(x => new ParameterSummary
                    {
                        Names = x.Name.OrderByDescending(n => n.Length).ToArray(),
                        Type = x.Property.PropertyType,
                        Required = x.Required,
                        Position = x.Position
                    });

                    RenderParameterList(commandSummary, parameterSummaries, context.Logger);
                }
                else
                {
                    context.Logger.Error($"Command {commandName} not found.");
                }
            }
        }

        protected virtual void RenderCommandList(IEnumerable<CommandSummary> commandSummaries, ILogger logger)
        {
            var indent = new string(' ', IndentWidth);

            var count = 0;
            foreach (var commandSummary in commandSummaries)
            {
                if (count > 0)
                {
                    logger.Info(string.Empty);
                    logger.Debug("---");
                    logger.Info(string.Empty);
                }

                logger.Debug("NAME");
                logger.Info($"{indent}{string.Join(" | ", commandSummary.Names.OrderByDescending(n => n.Length))}");

                logger.Info(string.Empty);
                logger.Debug("ABOUT");
                logger.Info($"{indent}{commandSummary.Description}");

                count++;
            }
        }

        protected virtual void RenderParameterList(CommandSummary commandSummary, IEnumerable<ParameterSummary> parameterSummaries, ILogger logger)
        {
            var indent = new string(' ', IndentWidth);

            logger.Debug("NAME");
            logger.Info($"{indent}{string.Join(" | ", commandSummary.Names.OrderByDescending(n => n.Length))}");

            logger.Info(string.Empty);
            logger.Debug("ABOUT");
            logger.Info($"{indent}{(string.IsNullOrEmpty(commandSummary.Description) ? "N/A" : commandSummary.Description)}");

            logger.Info(string.Empty);

            logger.Debug("SYNTAX");
            logger.Info(string.Empty);

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

            logger.Info($"{indent}{commandSummary.Names.OrderByDescending(n => n).First()} {string.Join(" ", positional.Concat(required).Concat(optional))}");

            logger.Info(string.Empty);
            logger.Debug("ARGUMENTS");
            logger.Info(string.Empty);

            foreach (var parameter in positional.Concat(required.Concat(optional)))
            {
                
            }
        }
    }

    internal static class StringExtensions
    {
        public static string Optional(this string value) => $"<{value}>";
        public static string Required(this string value) => $"[{value}]";
    }
}

/*
 

NAME
    command1 | cmd1
    
ABOUT
    Does this

---

NAME
    command2

ABOUT
    Does that.

---

NAME

    command1 
    cmd1
    
ABOUT

    Does this.

SYNTAX

    command1 <1:arg1> <2:arg2> <arg3> <arg3> [arg4] [arg5]

ARGUMENTS

    arg1            blah
    arg2            blah     
     
     
     */
