using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Data;
using Reusable.CommandLine.Logging;
using Reusable.CommandLine.Services;
using System.Collections.Immutable;
using Reusable.CommandLine.Commands;

namespace Reusable.CommandLine
{
    public static class CommandLineExecutor
    {
        private static readonly ICommandLineTokenizer Tokenizer = new CommandLineTokenizer();

        [PublicAPI]
        [ContractAnnotation("commands: null => halt; text: null => halt")]
        public static void Execute([NotNull] this string text, [NotNull] CommandContainer commands, CommandLineSettings settings = null)
        {
            settings = settings ?? CommandLineSettings.Default;

            var executables =
                Tokenizer
                    .Tokenize(text)
                    //.PrependCommandName(commands)
                    .Parse()
                    .Select(argument => (Argument: argument, Command: commands.Find(argument.CommandName())))
                    .ToLookup(x => x.Command != null);

            if (!executables.Any())
            {
                settings.Logger.Error("Default ");
                return;
            }

            if (executables[false].Any())
            {
                var nonExecutable = executables[false].Select(x => x.Argument.ToCommandLineString("-:"));
                settings.Logger.Error($"Command not found. Arguments: [{string.Join(" | ", nonExecutable)}]");
                return;
            }


            foreach (var executable in executables[true])
            {
                executable.Command.Execute(new ConsoleContext(executable.Argument, settings.Culture, commands, settings.Logger));                
            }
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