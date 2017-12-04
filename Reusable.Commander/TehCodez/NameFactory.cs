using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Extensions;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander
{
    internal static class NameFactory
    {
        [NotNull]
        public static SoftKeySet CreateCommandName([NotNull] Type commadType)
        {
            if (commadType == null)
            {
                throw new ArgumentNullException(nameof(commadType));
            }
            
            if (!typeof(IConsoleCommand).IsAssignableFrom(commadType))
            {
                throw new ArgumentException(paramName: nameof(commadType), message: $"'{nameof(commadType)}' needs to be derived from '{nameof(ICommand)}'");
            }

            var names = GetCommandNames();

            var category = commadType.GetCustomAttribute<CategoryAttribute>()?.Category;
            if (category.IsNotNull())
            {
                names = names.Select(n => SoftString.Create($"{category}.{n}"));
            }

            return SoftKeySet.Create(names);

            IEnumerable<SoftString> GetCommandNames()
            {
                yield return GetCommandDefaultName(commadType);
                foreach (var name in commadType.GetCustomAttribute<AliasAttribute>() ?? Enumerable.Empty<SoftString>())
                {
                    yield return name;
                }
            }
        }

        private static string GetCommandDefaultName(Type commadType)
        {
            return Regex.Replace(commadType.Name, "C(omman|m)d$", string.Empty, RegexOptions.IgnoreCase);
        }

        [NotNull]
        public static SoftKeySet CreatePropertyName([NotNull] PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            var names = GetParameterNames();
            return SoftKeySet.Create(names);

            IEnumerable<SoftString> GetParameterNames()
            {
                // Always use the property name as default.
                yield return property.Name;

                // Then get alias if any.
                foreach (var alias in property.GetCustomAttribute<AliasAttribute>() ?? Enumerable.Empty<SoftString>())
                {
                    yield return alias;
                }
            }
        }

        //// Creates a short name from the capital letters.
        //private static string CreateShortName(string fullName)
        //{
        //    return Regex
        //        .Matches(fullName, "[A-Z]")
        //        .Cast<Match>()
        //        .Select(m => m.Groups[0].Value)
        //        .Aggregate(new StringBuilder(), (current, next) => current.Append(next))
        //        .ToString();
        //}
    }
}