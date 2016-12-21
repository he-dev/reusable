using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Shelly.Collections;

namespace Reusable.Shelly
{
    public class CommandLineParser
    {
        public ArgumentCollection Parse(IEnumerable<string> args, string argumentPrefix, string argumentValueSeparator, IList<string> commandNames)
        {
            // ~ ignores null or empty args
            if (args == null) { return null; }

            // https://regex101.com/r/qY0lT3/3
            // ^(?<prefix>-)?(?<name>[a-z][a-z0-9-_]*)

            var parameterMatcher = new Regex($"^(?<prefix>(?:{argumentPrefix}))(?<name>([a-z_][a-z0-9-_]*|\\?))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            var arguments = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var lastArgumentName = (string)null;

            var commandName = string.Empty;
            var isFirstArg = true;

            foreach (var arg in args)
            {
                if (isFirstArg && commandNames.Contains(arg, StringComparer.OrdinalIgnoreCase))
                {
                    commandName = arg;
                    isFirstArg = false;
                    continue;
                }

                var parameterMatch = parameterMatcher.Match(arg);

                if (parameterMatch.Success)
                {
                    lastArgumentName = parameterMatch.Groups["name"].Value;
                    arguments.Add(lastArgumentName, new List<string>());

                    if (string.IsNullOrEmpty(argumentValueSeparator))
                    {
                        continue;
                    }

                    var separatorIndex = arg.IndexOf(argumentValueSeparator, StringComparison.OrdinalIgnoreCase);
                    if (separatorIndex <= 0)
                    {
                        continue;
                    }

                    var value = arg.Substring(separatorIndex + 1);
                    arguments[lastArgumentName].Add(value);
                }
                else
                {
                    if (!arguments.Any())
                    {
                        arguments.Add(lastArgumentName = string.Empty, new List<string>());
                    }
                    arguments[lastArgumentName].Add(arg);
                }
            }

            return new ArgumentCollection(commandName, arguments);
        }
    }
}
