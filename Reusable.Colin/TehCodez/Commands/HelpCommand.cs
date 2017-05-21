using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Reusable.Colin.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Data;

namespace Reusable.Colin.Commands
{
    [CommandName("help", "h", "?")]
    [Description("Display help.")]
    public class HelpCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (!(parameter is ExecuteContext context))
            {
                throw new ArgumentException(message: $"'{nameof(ExecuteContext)} expected but found '{parameter?.GetType()}'", paramName: nameof(parameter));
            }
            Execute(context.Parameter as HelpCommandParameter, context.CommandLine);
        }

        private void Execute(HelpCommandParameter parameter, CommandLine commandLine)
        {
            if (string.IsNullOrEmpty(parameter.CommandName))
            {
                //CreateCommandSummaries(parameters.CommandName.Commands);
                // Write command list.
            }
            else
            {
                // Write argument list for the command.
                if (commandLine.TryGetValue(ImmutableNameSet.Create(parameter.CommandName), out CommandInvoker command))
                {
                    //throw new Exception($"Command \"{CommandName}\" not found.");
                }

                //_helpWriter.WriteArguments(CreateArgumentSummaries(command));
            }

        }

        //private static IEnumerable<CommandSummary> CreateCommandSummaries(IEnumerable<CommandInfo> commands)
        //{
        //    //return commands.Select(command => new CommandSummary
        //    //{
        //    //    Names = command.CommandType.GetCommandNames().ToArray(),
        //    //    Description = command.CommandType.GetDescription(),
        //    //    IsDefault = command.IsDefault
        //    //});
        //    return null;
        //}

        //private static IEnumerable<ArgumentSummary> CreateArgumentSummaries(CommandInfo command)
        //{
        //    //return command.CommandType.GetCommandProperties().Select(commandProperty => new ArgumentSummary
        //    //{
        //    //    Names = commandProperty.Names.ToArray(),
        //    //    //Description = commandProperty..GetDescription(),
        //    //    Type = commandProperty.Type,
        //    //    Mandatory = commandProperty.Mandatory,
        //    //    Position = commandProperty.Position,
        //    //    ListSeparator = commandProperty.ListSeparator
        //    //});
        //    return null;
        //}

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