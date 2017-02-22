using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Shelly.Collections;
using Reusable.Shelly.Data;
using System;
using System.Collections.Immutable;

namespace Reusable.Shelly
{
    public class CommandLineParser
    {
        public static ArgumentCollection Parse(IEnumerable<string> args, string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) throw new ArgumentNullException(nameof(prefix));

            var arguments = new ArgumentCollection();
            var currentArgumentName = ImmutableNameSet.Create(string.Empty);

            foreach (var arg in args ?? throw new ArgumentNullException(nameof(args)))
            {
                switch (arg.StartsWith(prefix))
                {
                    case true:
                        currentArgumentName = ImmutableNameSet.Create(Regex.Replace(arg, $"^{prefix}", string.Empty));
                        arguments.Add(currentArgumentName, null);
                        break;

                    case false:
                        arguments.Add(currentArgumentName, arg);
                        break;
                }

            }
            return arguments;
        }
    }
}
