using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Colin.Data;
using System;
using System.Collections.Immutable;
using Reusable.Colin.Collections;

namespace Reusable.Colin
{
    public class CommandLineParser
    {
        public static IEnumerable<ArgumentCollection> Parse(IEnumerable<string> args, string argumentPrefix)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (string.IsNullOrEmpty(argumentPrefix)) throw new ArgumentNullException(nameof(argumentPrefix));

            var arguments = new ArgumentCollection();
            var currentArgumentName = ImmutableNameSet.Empty;

            foreach (var arg in args ?? throw new ArgumentNullException(nameof(args)))
            {
                switch (arg)
                {
                    case "|" when arguments.Any():
                        yield return arguments;
                        arguments = new ArgumentCollection();
                        break;

                    case string value when value.StartsWith(argumentPrefix):
                        currentArgumentName = ImmutableNameSet.Create(Regex.Replace(arg, $"^{argumentPrefix}", string.Empty));
                        arguments.Add(currentArgumentName, null);
                        break;

                    default:
                        arguments.Add(currentArgumentName, arg);
                        break;
                }
            }
            if (arguments.Any()) yield return arguments;
        }
    }
}
