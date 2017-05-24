using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Colin.Data;

namespace Reusable.Colin
{
    public static class CommandExecutor
    {
        [PublicAPI]
        [ContractAnnotation("commands: null => halt; text: null => halt")]
        public static void Execute([NotNull] this CommandCollection commands, [NotNull] string text, CommandLineSettings settings = null)
        {
            settings = settings ?? CommandLineSettings.Default;

            text
                .Tokenize(settings.ArgumentValueSeparator)
                .PrependCommandName(commands)
                .Parse(settings.ArgumentPrefix.ToString())
                .Select(a => (Executor: a.Executor(commands), Arguments: a))
                .Where(x => x.Executor != null)
                .Execute(commands);
        }

        [PublicAPI]
        [ContractAnnotation("=> halt")]
        public static void Execute([NotNull] this CommandCollection commands, [NotNull] IEnumerable<string> args, CommandLineSettings settings = null)
        {
            commands.Execute(string.Join(" ", args ?? throw new ArgumentNullException(nameof(args))), settings);
        }

        [PublicAPI]
        [ContractAnnotation("commands: null => halt")]
        public static void Execute(this IEnumerable<(Services.CommandExecutor Executor, ArgumentLookup Arguments)> pairs, [NotNull] CommandCollection commands, CommandLineSettings settings = null)
        {
            if (commands == null) { throw new ArgumentNullException(nameof(commands)); }

            settings = settings ?? CommandLineSettings.Default;

            foreach (var pair in pairs)
            {
                var commandParameter = pair.Executor.ParameterFactory.CreateParameter(pair.Arguments);
                pair.Executor.Command.Execute(new CommandContext(commandParameter, commands, settings.Logger));
                //pair.Executor.Execute(pair.Arguments, commands, settings.Logger);
            }
        }

        private static IEnumerable<string> PrependCommandName(this IEnumerable<string> tokens, CommandCollection commands)
        {
            return
                commands.Count == 1
                    ? new[] { commands.Single().Key.First() }.Concat(tokens)
                    : tokens;
        }
    }
}