using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Colin.Data;
using Reusable.Colin.Logging;

namespace Reusable.Colin
{
    public static class CommandLineExecutor
    {
        [PublicAPI]
        [ContractAnnotation("commands: null => halt; text: null => halt")]
        public static void Execute([NotNull] this CommandCollection commands, [NotNull] string text, CommandLineSettings settings = null)
        {
            settings = settings ?? CommandLineSettings.Default;

            var executables =
                text
                    .Tokenize(settings.ArgumentValueSeparator)
                    .PrependCommandName(commands)
                    .Parse(settings.ArgumentPrefix)
                    .Select(argument => new
                    {
                        Argument = argument,
                        Mapping = argument.Map(commands),
                    })
                    .ToLookup(x => x.Mapping != null);

            if (executables[false].Any())
            {
                var nonExecutable = executables[false].Select(x => x.Argument.ToCommandLine($"{settings.ArgumentPrefix}{settings.ArgumentValueSeparator}"));
                settings.Logger.Error($"Command not found. Arguments: [{string.Join(" | ", nonExecutable)}]");
                return;
            }

            foreach (var executable in executables[true])
            {
                var commandParameter = executable.Mapping.ParameterFactory.CreateParameter(executable.Argument, settings.Culture);
                executable.Mapping.Command.Execute(new CommandContext(commandParameter, commands, settings.Logger));
            }
        }

        [PublicAPI]
        [ContractAnnotation("=> halt")]
        public static void Execute([NotNull] this CommandCollection commands, [NotNull] IEnumerable<string> args, CommandLineSettings settings = null)
        {
            commands.Execute(string.Join(" ", args ?? throw new ArgumentNullException(nameof(args))), settings);
        }

        // Prepends command name to the collection of tokens when there is only one command. It's considered as default.
        private static IEnumerable<string> PrependCommandName(this IEnumerable<string> tokens, CommandCollection commands)
        {
            return
                commands.Count == 1
                    ? new[] { commands.Single().Key.First() }.Concat(tokens)
                    : tokens;
        }
    }
}