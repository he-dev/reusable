using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Services;
using System;
using System.Collections.Generic;

namespace Reusable.Colin
{
    public static class CommandLineExecutor
    {
        [PublicAPI]
        [ContractAnnotation("commandLine: null => halt; text: null => halt")]
        public static void Execute([NotNull] this CommandLine commandLine, [NotNull] string text, CommandLineSettings settings = null)
        {
            settings = settings ?? CommandLineSettings.Default;

            text
                .Tokenize(settings.ArgumentValueSeparator)
                .Parse(settings.ArgumentPrefix.ToString())
                .FindCommands(commandLine)
                .Execute(commandLine);
        }

        [PublicAPI]
        [ContractAnnotation("=> halt")]
        public static void Execute([NotNull] this CommandLine commandLine, [NotNull] IEnumerable<string> args, CommandLineSettings settings = null)
        {
            commandLine.Execute(string.Join(" ", args ?? throw new ArgumentNullException(nameof(args))), settings);
        }

        [PublicAPI]
        [ContractAnnotation("commandLine: null => halt")]
        public static void Execute(this IEnumerable<(CommandExecutor CommandInvoker, ArgumentLookup Arguments)> pairs, [NotNull] CommandLine commandLine, CommandLineSettings settings = null)
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            settings = settings ?? CommandLineSettings.Default;

            foreach (var pair in pairs)
            {
                pair.CommandInvoker.Execute(pair.Arguments, commandLine, settings.Logger);
            }
        }       
    }
}