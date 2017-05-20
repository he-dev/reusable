using System;
using System.Collections.Generic;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Data;
using Reusable.Colin.Services;

namespace Reusable.Colin
{
    public static class CommandLineExtensions
    {
        [ContractAnnotation("commandLine: null => halt; text: null => halt")]
        public static void Execute([NotNull] this CommandLine commandLine, [NotNull] string text)
        {
            text
                .Tokenize(commandLine.ArgumentValueSeparator)
                .Parse(commandLine.ArgumentPrefix.ToString())
                .FindCommands(commandLine)
                .Execute(commandLine);
        }

        [ContractAnnotation("commandLine: null => halt")]
        public static void Execute(this IEnumerable<(ICommand Command, ArgumentLookup Arguments)> pairs, [NotNull] CommandLine commandLine)
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            foreach (var pair in pairs)
            {
                pair.Command.Execute(new CommandLineContext(commandLine, pair.Arguments));
            }
        }

        [ContractAnnotation("=> halt")]
        public static void Execute([NotNull] this CommandLine commandLine, [NotNull] IEnumerable<string> args)
        {
            commandLine.Execute(string.Join(" ", args ?? throw new ArgumentNullException(nameof(args))));
        }
    }
}