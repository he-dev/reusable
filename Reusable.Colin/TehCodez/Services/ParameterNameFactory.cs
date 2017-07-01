using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.CommandLine.Annotations;
using Reusable.CommandLine.Collections;

namespace Reusable.CommandLine.Services
{
    internal static class ParameterNameFactory
    {
        [NotNull]
        public static IImmutableNameSet From([NotNull] PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var names = GetParameterNames(property);
            return ImmutableNameSet.Create(names);

        }

        private static IEnumerable<string> GetParameterNames(MemberInfo property)
        {
            var parameter = property.GetCustomAttribute<ParameterAttribute>();

            if (parameter.Names.Any())
            {
                foreach (var name in parameter.Names)
                {
                    yield return name;
                }
            }
            else
            {
                yield return property.Name;
                if (parameter.AllowShortName)
                {
                    yield return CreateShortName(property.Name);
                }
            }

        }

        private static string CreateShortName(string fullName)
        {
            return Regex
                .Matches(fullName, "[A-Z]")
                .Cast<Match>()
                .Select(m => m.Groups[0].Value)
                .Aggregate(new StringBuilder(), (current, next) => current.Append(next))
                .ToString();
        }
    }
}
