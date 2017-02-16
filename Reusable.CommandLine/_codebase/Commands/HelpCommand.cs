using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Reusable.Fuse;
using Reusable.Shelly.Data;
using Reusable.Shelly.Reflection;

namespace Reusable.Shelly.Commands
{
    public interface IHelpWriter
    {
        void WriteCommands(IEnumerable<CommandSummary> commandSummaries);
        void WriteArguments(IEnumerable<ArgumentSummary> argumentSummaries);
    }

    [Shortcut("h", "?")]
    [Description("Display help.")]
    public class HelpCommand : Command
    {
        private readonly CommandLine _commandLine;

        private readonly IHelpWriter _helpWriter;

        public HelpCommand(CommandLine commandLine, IHelpWriter helpWriter)
        {
            commandLine.Validate(nameof(commandLine)).IsNotNull();
            helpWriter.Validate(nameof(helpWriter)).IsNotNull();
            _commandLine = commandLine;
            _helpWriter = helpWriter;
        }

        [Parameter(Position = 1)]
        [Shortcut("Command", "cmd")]
        [Description("Display command usage.")]
        public string CommandName { get; set; }        

        public override void Execute()
        {
            //if (CommandLine == null) { throw new InvalidOperationException($"{nameof(CommandLine)} must be set first."); }

            if (string.IsNullOrEmpty(CommandName))
            {
                _helpWriter.WriteCommands(CreateCommandSummaries(_commandLine.Commands));
            }
            else
            {
                var command = _commandLine.FindCommand(CommandName);
                if (command == null)
                {
                    throw new Exception($"Command \"{CommandName}\" not found.");
                }
                _helpWriter.WriteArguments(CreateArgumentSummaries(command));
            }            
        }

        private static IEnumerable<CommandSummary> CreateCommandSummaries(IEnumerable<CommandInfo> commands)
        {
            return commands.Select(command => new CommandSummary
            {
                Names = command.CommandType.GetCommandNames().ToArray(),
                Description = command.CommandType.GetDescription(),
                IsDefault = command.IsDefault
            });
        }

        private static IEnumerable<ArgumentSummary> CreateArgumentSummaries(CommandInfo command)
        {
            return command.CommandType.GetCommandProperties().Select(commandProperty => new ArgumentSummary
            {
                Names = commandProperty.Names.ToArray(),
                //Description = commandProperty..GetDescription(),
                Type = commandProperty.Type,
                Mandatory = commandProperty.Mandatory,
                Position = commandProperty.Position,
                ListSeparator = commandProperty.ListSeparator
            });
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