using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Shelly.Collections;

namespace Reusable.Shelly
{
    public class CommandLineParser
    {
        public static ArgumentCollection Parse(IEnumerable<string> args, string prefix)
        {
            var arguments = new List<Argument>();

            foreach (var arg in args)
            {
                switch (arguments.Any())
                {
                    case true:
                        switch (arg.StartsWith(prefix))
                        {
                            case true:
                                arguments.Add(new Argument(Regex.Replace(arg, $"^{prefix}", string.Empty)));
                                break;
                            default:
                                arguments.Last().Add(arg);
                                break;
                        }
                        break;

                    default:
                        arguments.Add(new Argument(arg));
                        break;
                }
            }
            return new ArgumentCollection(arguments);
        }
    }
}
