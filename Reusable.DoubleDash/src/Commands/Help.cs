using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.DoubleDash.Annotations;
using Reusable.Marbles;
using Reusable.Marbles.Annotations;
using Reusable.Prism;
using Reusable.Utilities.Autofac;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Channels;

namespace Reusable.DoubleDash.Commands;

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

    protected override Task ExecuteAsync(CommandParameter parameter, CancellationToken cancellationToken)
    {
        _logger.Log(ConsoleElement<>.ComposeLine().Indent(1).Text(_version));

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
        // Print headers,
        var captions = new[] { "Command", "Description" }; //.Pad(ColumnWidths);
        //Logger.WriteLine(Style, new Indent(1), new TableRow { Cells = captions });
        Logger.Log(ConsoleElement<>.ComposeLine().Indent(1).Record(captions, ColumnWidths));

        // Underline headers. Each one is as wide as its text.
        var separators = captions.Select(c => new string('-', c.Trim().Length)); //.Pad(ColumnWidths);
        //Logger.WriteLine(Style, new Indent(1), new TableRow { Cells = separators });
        Logger.Log(ConsoleElement<>.ComposeLine().Indent(1).Record(separators, ColumnWidths));

        // Commands
        var userExecutableCommands =
            from command in parameter.Commands
            where !command.CommandType.IsDefined(typeof(InternalAttribute))
            orderby command.NameCollection.Primary
            select command;

        foreach (var command in userExecutableCommands)
        {
            var defaultId = command.NameCollection.Primary;
            var aliases = string.Join("|", command.NameCollection.Secondary.Select(x => x.ToString()));
            var description = command.CommandType.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "N/A";
            var row = new[] { $"{defaultId} ({(aliases.Length > 0 ? aliases : "-")})", description }.Pad(ColumnWidths);
            //Logger.WriteLine(Style, new Indent(1), new TableRow { Cells = row });
            //Logger.Log(ConsoleTemplate.ComposeLine().Indent(1).Record());
        }
    }

    protected virtual void RenderParameterList(Parameter parameter)
    {
        var command =
            parameter
                .Commands
                .Where(c => c.NameCollection.Contains(parameter.Command, SoftString.Comparer))
                .SingleOrThrow($"Could not find a command with the name '{parameter.Command}'");

        // Headers
        var captions = new[] { "Option", "Description" }.Pad(ColumnWidths).ToList();
        //Logger.WriteLine(Style, new Indent(1), new TableRow { Cells = captions });

        // Separators
        var separators = captions.Select(c => new string('-', c.Trim().Length)).Pad(ColumnWidths);
        //Logger.WriteLine(Style, new Indent(1), new TableRow { Cells = separators });

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
            //Logger.WriteLine(Style, new Indent(1), new TableRow { Cells = row });
        }
    }

    [PublicAPI]
    public class Parameter : CommandParameter
    {
        [Description("Display command usage.")]
        [Alias("cmd")]
        [Position(1)]
        public string? Command { get; set; }

        [Service]
        public IEnumerable<CommandInfo> Commands { get; set; }
    }
}

internal static class HelpExtensions
{
    public static string Required(this string value) => $"<{value}>";

    public static string Optional(this string value) => $"[{value}]";
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