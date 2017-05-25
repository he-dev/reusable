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
            var commandName = ((HelpCommandParameter)context.Parameter).CommandName;
            
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
                if (context.CommandCollection.TryGetValue(ImmutableNameSet.Create(commandName), out Services.CommandMapping command))
                {
                    RenderParameterList(command.ParameterFactory.Select(x => new ParameterSummary
                    {
                        Names = x.Name,
                        Type = x.Property.PropertyType,
                        Required = x.Required,
                        Position = x.Position
                    }), context.Logger);
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
                foreach (var name in commandSummary.Names.OrderByDescending(n => n.Length))
                {
                    logger.Info($"{indent}{name}");
                }

                logger.Info(string.Empty);
                logger.Debug("ABOUT");
                logger.Info($"{indent}{commandSummary.Description}");

                count++;
            }
        }

        protected virtual void RenderParameterList(IEnumerable<ParameterSummary> parameterSummaries, ILogger logger)
        {
            foreach (var parameterSummary in parameterSummaries)
            {
                logger.Info(parameterSummary.Names.First());
            }
        }
                

        //private void RenderCommandUsage(Type commandType)
        //{
        //    Debug.Assert(commandType != null);

        //    Logger.Info(commandType.Description() ?? "(No description)");
        //    Logger.Info(string.Empty);

        //    var parameters = ReflectionHelper.GetCommandProperties(commandType).Aggregate(
        //        new StringBuilder($"<{string.Join("|", commandType.Names())}>"), (result, p) => 
        //        {
        //            var names = $"{string.Join($"|", p.Names.Select(n => $"{CommandLine.ArgumentPrefix}{n}"))}";
        //            if (p.Position > 0)
        //            {
        //                names = $"{names}|{p.Position}";
        //            }
        //            return result.Append($" {(p.Mandatory ? $"<{names}>" : $"[{names}]")}");
        //        }).ToString();
        //    Logger.Info(parameters);            
        //}

        //private void RenderParameterDescription(Type commandType)
        //{
        //    var parameters = ReflectionHelper.GetCommandProperties(commandType);
        //    Logger.Info(parameters.Aggregate(new StringBuilder(), (result, p) 
        //        => result.AppendLine().AppendFormat(" {0}  {1}", p.Names.First().PadRight(14), p.Description)).ToString()
        //    );
        //    Logger.Info(string.Empty);
        //}
    }


    //internal class ArgumentOrderComparer : IComparer<Argument>
    //{
    //    public int Compare(Argument x, Argument y)
    //    {
    //        var args = new[]
    //        {
    //            new {a = x, o = -1}, new {a = y, o = 1}
    //        };

    //        if (args.Any(z => z.a.Properties.HasPosition))
    //        {
    //            return args.All(z => z.a.Properties.HasPosition) ? x.Properties.Position - y.Properties.Position : args.First(z => z.a.Properties.HasPosition).o;
    //        }

    //        if (args.Any(z => z.a.Properties.IsRequired))
    //        {
    //            return args.All(z => z.a.Properties.IsRequired) ? string.Compare(x.Names.First(), y.Names.First(), StringComparison.OrdinalIgnoreCase) : args.First(z => z.a.Properties.IsRequired).o;
    //        }

    //        return string.Compare(x.Names.First(), y.Names.First(), StringComparison.OrdinalIgnoreCase);
    //    }
    //}
}

/*
 

NAME
    command1
    cmd1
    
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
