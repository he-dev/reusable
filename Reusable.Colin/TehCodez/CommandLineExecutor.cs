using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Data;
using Reusable.CommandLine.Services;
using Reusable.CommandLine.Commands;
using Reusable.Loggex;

namespace Reusable.CommandLine
{
    public static class CommandLineExecutor
    {
        private static readonly ICommandLineTokenizer Tokenizer = new CommandLineTokenizer();

        [PublicAPI]
        [ContractAnnotation("commands: null => halt; text: null => halt")]
        public static bool Execute([NotNull] this string text, [NotNull] CommandContainer commands, CommandLineSettings settings = null)
        {
            settings = settings ?? CommandLineSettings.Default;

            var executables =
                Tokenizer
                    .Tokenize(text)
                    //.PrependCommandName(commands)
                    .Parse()
                    .Select(argument => (Argument: argument, Command: commands.Find(argument.CommandName())))
                    .ToLookup(IsExecutable);

            if (executables.Any())
            {
                if (executables[false].Any())
                {
                    var nonExecutable = executables[false].Select(x => x.Argument.ToCommandLineString("-:"));
                    settings.Logger.Log(e => e.Error().Message($"Command not found. Arguments: [{string.Join(" | ", nonExecutable)}]"));
                    return false;
                }
                else
                {
                    foreach (var executable in executables[true])
                    {
                        executable.Command.Execute(new ConsoleContext(executable.Argument, settings.Culture, commands, settings.Logger));
                    }
                    return true;
                }
            }
            else
            {
                var primaryCommands = commands.PrimaryCommands().ToList();
                if (primaryCommands.Count == 1)
                {
                    primaryCommands.Single().Execute(new ConsoleContext(ArgumentLookup.Empty, settings.Culture, commands, settings.Logger));
                    return true;
                }
                else
                {
                    settings.Logger.Log(e => e.Error().Message("Command not found."));
                    return false;
                }
            }

            bool IsExecutable((ArgumentLookup Arguments, IConsoleCommand Command) t) => t.Command != null;
        }

        [PublicAPI]
        [ContractAnnotation("=> halt")]
        public static void Execute([NotNull] this IEnumerable<string> args, [NotNull] CommandContainer commands, CommandLineSettings settings = null)
        {
            string.Join(" ", args ?? throw new ArgumentNullException(nameof(args))).Execute(commands, settings);
        }

        // Prepends command name to the collection of tokens when there is only one command. It's considered as default.
        private static IEnumerable<string> PrependCommandName(this IEnumerable<string> tokens, CommandContainer commands)
        {
            return
                commands.Count == 1
                    ? new[] { commands.Single().Key.First() }.Concat(tokens)
                    : tokens;
        }
    }
}