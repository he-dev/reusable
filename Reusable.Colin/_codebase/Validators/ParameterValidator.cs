using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Shelly.Validators
{
    internal static class ParameterValidator
    {
        public static void ValidateParameterNamesUniqueness(IEnumerable<Data.ParameterInfo> parameters)
        {
            var duplicateNames =
                parameters
                    .SelectMany(p => p.Names)
                    .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

            if (duplicateNames.Any())
            {
                throw new DuplicateParameterNameException(parameters.First().Property.DeclaringType, duplicateNames);
            }
        }
    }
}
