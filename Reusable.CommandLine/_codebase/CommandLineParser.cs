using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reusable.Shelly
{
    public class CommandLineParser
    {
        public CommandLineParseResult Parse(IEnumerable<string> args, string argumentPrefix, string argumentValueSeparator, IEnumerable<string> commandNames)
        {
            // ~ ignores null or empty args
            if (args == null) { return null; }

            // https://regex101.com/r/qY0lT3/3
            // ^(?<prefix>-)?(?<name>[a-z][a-z0-9-_]*)

            var parameterMatcher = new Regex($"^(?<prefix>(?:{argumentPrefix}))(?<name>([a-z_][a-z0-9-_]*|\\?))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            var arguments = new List<CommandLineArgument>();
            var lastParameter = new Func<CommandLineArgument>(() => arguments.Last());

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
                    arguments.Add(new CommandLineArgument(parameterMatch.Groups["name"].Value));

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
                    lastParameter().Add(value);
                }
                else
                {
                    if (!arguments.Any())
                    {
                        arguments.Add(new CommandLineArgument(string.Empty));
                    }
                    lastParameter().Add(arg);
                }
            }

            return new CommandLineParseResult(commandName, arguments);
        }
    }
}
