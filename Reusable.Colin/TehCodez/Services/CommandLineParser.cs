using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Colin.Collections;

namespace Reusable.Colin.Services
{
    public static class CommandLineParser
    {
        [NotNull]
        public static IEnumerable<ArgumentLookup> Parse([NotNull] this IEnumerable<string> args, [NotNull] string argumentPrefix)
        {
            if (args == null) { throw new ArgumentNullException(nameof(args)); }
            if (string.IsNullOrEmpty(argumentPrefix)) { throw new ArgumentNullException(nameof(argumentPrefix)); }

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
                    case string value when value.StartsWith(argumentPrefix):
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
