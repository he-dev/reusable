using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Data;
using Reusable.CommandLine.Logging;
using Reusable.CommandLine.Services;

namespace Reusable.CommandLine
{
    public static class CommandLineExecutor
    {
        private static readonly ICommandLineTokenizer Tokenizer = new CommandLineTokenizer();

        [PublicAPI]
        [ContractAnnotation("commands: null => halt; text: null => halt")]
        public static void Execute([NotNull] this CommandContainer commands, [NotNull] string text, CommandLineSettings settings = null)
        {
            settings = settings ?? CommandLineSettings.Default;

            var executables =
                Tokenizer.Tokenize(text)
                    .PrependCommandName(commands)
                    .Parse()
                    .Select(argument => (Argument: argument, CommandMetadata: argument.FindCommand(commands)))
                    .ToLookup(x => x.CommandMetadata != null);

            if (executables[false].Any())
            {
                var nonExecutable = executables[false].Select(x => x.Argument.ToCommandLineString("-:"));
                settings.Logger.Error($"Command not found. Arguments: [{string.Join(" | ", nonExecutable)}]");
                return;
            }

            foreach (var executable in executables[true])
            {
                var parameter = CommandParameterFactory.CreateParameter(executable.CommandMetadata.Parameter, executable.Argument, settings.Culture);
                executable.CommandMetadata.Command.Execute(new CommandContext(parameter, commands, settings.Logger));                
            }
        }

        [PublicAPI]
        [ContractAnnotation("=> halt")]
        public static void Execute([NotNull] this CommandContainer commands, [NotNull] IEnumerable<string> args, CommandLineSettings settings = null)
        {
            commands.Execute(string.Join(" ", args ?? throw new ArgumentNullException(nameof(args))), settings);
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