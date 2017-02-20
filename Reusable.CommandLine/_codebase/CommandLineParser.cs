using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Shelly.Collections;
using Reusable.Shelly.Data;
using System;

namespace Reusable.Shelly
{
    public class CommandLineParser
    {
        public static ArgumentCollection Parse(IEnumerable<string> args, string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) throw new ArgumentNullException(nameof(prefix));

            var arguments = new List<CommandLineArgument>();

            foreach (var arg in args ?? throw new ArgumentNullException(nameof(args)))
            {
                switch (arg.StartsWith(prefix))
                {
                    case true:
                        arguments.Add(new CommandLineArgument(Regex.Replace(arg, $"^{prefix}", string.Empty)));
                        break;

                    case false:
                        switch (arguments.Any())
                        {
                            case true:
                                arguments.Last().Add(arg);
                                break;

                            case false:
                                arguments.Add(new CommandLineArgument(string.Empty) { arg });
                                break;
                        }
                        break;
                }

            }
            return new ArgumentCollection(arguments);
        }
    }
}
