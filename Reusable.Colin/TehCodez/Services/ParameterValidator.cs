using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.CommandLine.Data;

namespace Reusable.CommandLine.Services
{
    internal static class ParameterValidator
    {
        public static void ValidateParameterNamesUniqueness(IReadOnlyCollection<ArgumentMetadata> parameters)
        {
            var duplicateNames =
                parameters
                    .SelectMany(p => p.Name)
                    .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

            if (duplicateNames.Any())
            {
                // ReSharper disable once PossibleNullReferenceException
                throw new ArgumentException($"{parameters.First().Property.DeclaringType.Name} contains duplicate parameter names: [{string.Join(", ", duplicateNames)}].");
            }
        }

        public static void ValidateParameterPositions(IReadOnlyCollection<ArgumentMetadata> parameters)
        {
            var positions = parameters.Where(p => p.Position > 0).Select(p => p.Position).ToList();

            var mid = positions.Count % 2 == 0 ? 0 : (positions.Count / 2) + 1;
            var sum = (((1 + positions.Count) * (positions.Count / 2)) + mid);
            if (sum != positions.Sum())
            {
                // ReSharper disable once PossibleNullReferenceException
                throw new ArgumentException($"The {parameters.First().Property.DeclaringType.Name} has some invalid parameter positions. They must begin with 1 and have positions increasing by 1.");
            }            
        }

        // todo: validate parameter types
    }
}
