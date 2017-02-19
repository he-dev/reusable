using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Shelly.Collections;
using Reusable.Shelly.Data;

namespace Reusable.Shelly
{
    public class CommandLineParser
    {
        public static ArgumentCollection Parse(IEnumerable<string> args, string prefix)
        {
            var arguments = new List<CommandLineArgument>();

            foreach (var arg in args)
            {
                switch (arguments.Any())
                {
                    case true:
                        switch (arg.StartsWith(prefix))
                        {
                            case true:
                                arguments.Add(new CommandLineArgument(Regex.Replace(arg, $"^{prefix}", string.Empty)));
                                break;
                            default:
                                arguments.Last().Add(arg);
                                break;
                        }
                        break;
                    default:
                        arguments.Add(new CommandLineArgument(string.Empty) { arg });
                        break;
                }
            }
            return new ArgumentCollection(arguments);
        }
    }
}
