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
        [NotNull]
        public static IEnumerable<ArgumentLookup> Parse([NotNull] this IEnumerable<string> args, char argumentPrefix)
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
                    case string value when value.StartsWith(argumentPrefix.ToString()):
                        currentArgumentName = ImmutableNameSet.Create(Regex.Replace(arg, $"^{argumentPrefix}", string.Empty));
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
