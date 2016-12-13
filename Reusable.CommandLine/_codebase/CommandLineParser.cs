using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reusable.Shelly
{
    //public interface ICommandLineParser
    //{
    //    ArgumentCollection Parse(IEnumerable<string> args, string argumentPrefix, string nameValueSeparator);
    //}

    public class CommandLineParser //: ICommandLineParser
    {
        public IEnumerable<IGrouping<string, string>> Parse(IEnumerable<string> args, string parameterPrefix, string parameterValueSeparator = null)
        {
            // ~ ignores null or empty args
            if (args == null) { return null; }

            // https://regex101.com/r/qY0lT3/3
            // ^(?<prefix>-)?(?<name>[a-z][a-z0-9-_]*)

            var parameterMatcher = new Regex($"^(?<prefix>(?:{parameterPrefix}))(?<name>([a-z_][a-z0-9-_]*|\\?))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

            var parameters = new List<CommandLineArgument> { new CommandLineArgument(string.Empty) };
            var lastParameter = new Func<CommandLineArgument>(() => parameters.Last());

            foreach (var arg in args)
            {
                var parameterMatch = parameterMatcher.Match(arg);
                if (parameterMatch.Success)
                {
                    parameters.Add(new CommandLineArgument(parameterMatch.Groups["name"].Value));

                    if (string.IsNullOrEmpty(parameterValueSeparator))
                    {
                        continue;
                    }

                    var separatorIndex = arg.IndexOf(parameterValueSeparator, StringComparison.OrdinalIgnoreCase);
                    if (separatorIndex <= 0)
                    {
                        continue;
                    }

                    var value = arg.Substring(separatorIndex + 1);
                    lastParameter().Add(value);
                }
                else
                {
                    lastParameter().Add(arg);
                }
            }

            return parameters;
        }
    }
}
