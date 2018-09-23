using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander.Commands
{
    public class HelpBag : SimpleBag
    {
        public int IndentWidth { get; set; } = 4;

        public int[] ColumnWidths { get; set; } = {17, 60};
        
        [CanBeNull]
        [DefaultValue(false)]
        [Description("Display command usage.")]
        [Position(1)]
        [Alias("cmd")]
        public string Command { get; set; }
    }
    
    [PublicAPI]
    [Alias("h", "?")]
    [Description("Display help.")]
    [Category("Testing")]
    public class Help : ConsoleCommand<HelpBag>
    {
        private readonly IEnumerable<IConsoleCommand> _commands;

        public Help(ILogger<Help> logger, ICommandLineMapper mapper, IEnumerable<IConsoleCommand> commands) : base(logger, mapper)
        {
            _commands = commands;
        }                

        protected override Task ExecuteAsync(HelpBag parameter, CancellationToken cancellationToken)
        {
            //var parameter = context.Parameter as Parameter ?? throw DynamicException.Factory.CreateDynamicException($"{nameof(Parameter)}Null{nameof(Exception)}", $"Command parameter must not be null.", null);

            var commandName = parameter.Command is null ? null : SoftKeySet.Create(parameter.Command);

            if (parameter.Command.IsNullOrEmpty())
            {
                RenderCommandList(parameter, _commands);
            }
            else
            {
                var command = _commands.SingleOrDefault(c => c.Name == commandName);

                if (command is null)
                {
                    Logger.Error($"Command {commandName.QuoteAllWith("'")} not found.");
                }
                else
                {
                    RenderParameterList(command);
                }
            }
                    

            return Task.CompletedTask;
        }

        protected virtual void RenderCommandList(HelpBag parameter, IEnumerable<IConsoleCommand> commands)
        {
            Logger.Information(string.Empty);
            Logger.Information($"{new string(' ', parameter.IndentWidth)}Commands");

            var commandSummaries = commands.Select(x => new CommandSummary
            {
                //Names = x.Name.OrderByDescending(n => n.Length),
                Description = x.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description
            });

            var captions = new[] { "NAME", "ABOUT" };

            Logger.Information(string.Empty);
            Logger.Debug(RenderColumns(captions, parameter.ColumnWidths));
            Logger.Debug(RenderColumns(captions.Select(h => new string('-', h.Length)), parameter.ColumnWidths));

            foreach (var commandSummary in commandSummaries)
            {
//                context.Logger.Log(e => e.Message(RenderColumns(new[]
//                {
//                    commandSummary.Names.First(),
//                    string.IsNullOrEmpty(commandSummary.Description) ? "N/A" : commandSummary.Description
//                }, ColumnWidths)));
            }
            Logger.Information(string.Empty);
            Logger.Information(string.Empty);
        }

        protected virtual void RenderParameterList(IConsoleCommand command)
        {
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
            public IEnumerable<SoftString> Names { get; set; }

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
        
//        [PublicAPI]
//        public class Parameter
//        {
//            //[CommandParameterProperty(Position = 1, Required = false)]
//            [CanBeNull]
//            [Position(1)]
//            [DefaultValue(false)]
//            [Description("Display command usage.")]
//            public string Command { get; set; }
//        }

        
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