using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.CommandLine.Collections;

namespace Reusable.CommandLine.Services
{
    public static class CommandLineParser
    {
        // language=regexp
        private const string ArgumentPrefix = @"^[-/\.]";

        [NotNull]
        public static IEnumerable<ArgumentLookup> Parse([NotNull] this IEnumerable<string> args)
        {
            if (args == null) { throw new ArgumentNullException(nameof(args)); }

            var arguments = new ArgumentLookup();
            var currentArgumentName = ImmutableNameSet.Empty;

            foreach (var arg in args ?? throw new ArgumentNullException(nameof(args)))
            {
                switch (arg)
                {
                    case "|" when arguments.Any():
                        yield return arguments;
                        arguments = new ArgumentLookup();
                        break;

                    // ReSharper disable once PatternAlwaysOfType
                    case string value when Regex.IsMatch(value, ArgumentPrefix):
                        currentArgumentName = ImmutableNameSet.Create(Regex.Replace(arg, ArgumentPrefix, string.Empty));
                        arguments.Add(currentArgumentName);
                        break;

                    default:
                        arguments.Add(currentArgumentName, arg);
                        break;
                }
            }

            if (arguments.Any())
            {
                yield return arguments;
            }
        }
    }
}
