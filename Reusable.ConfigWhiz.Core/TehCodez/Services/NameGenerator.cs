using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Services
{
    public static class NameGenerator
    {
        // language=regexp
        private const string NamePattern = @"(?:(?<Namespace>[a-z0-9_.]+)\+)?(?:(?<Type>[a-z0-9_]+)\.)?(?<Name>[a-z0-9_]+)(?:&(?<Instance>[a-z0-9_]+))?";

        [NotNull, ItemNotNull]
        public static IEnumerable<CaseInsensitiveString> GenerateNames([NotNull] this CaseInsensitiveString name)
        {

            /*
            
            Paths in order of resolution
            
            Program.Environment&Foo
            Environment&Foo
            TheApp+Program.Environment&Foo

            Program.Environment
            Environment
            TheApp+Program.Environment

             */

            var match = Regex.Match(name.ToString(), NamePattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                throw new ArgumentException("Invalid name.");
            }

            if (match.Groups["Instance"].Success)
            {
                yield return $"{match.Groups["Type"].Value}.{match.Groups["Name"].Value}&{match.Groups["Instance"].Value}";
                yield return $"{match.Groups["Name"].Value}&{match.Groups["Instance"].Value}";
                yield return $"{match.Groups["Namespace"].Value}+{match.Groups["Type"].Value}.{match.Groups["Name"].Value}&{match.Groups["Instance"].Value}";
            }

            yield return $"{match.Groups["Type"].Value}.{match.Groups["Name"].Value}";
            yield return $"{match.Groups["Name"].Value}";
            yield return $"{match.Groups["Namespace"].Value}+{match.Groups["Type"].Value}.{match.Groups["Name"].Value}";
        }
    }
}