using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Commander
{
    internal static class CommandParameterValidator
    {
        public static void ValidateParameterNamesUniqueness(ICommandInfo info)
        {
            var duplicateNames =
                info
                    .SelectMany(property => property.Name)
                    .GroupBy(propertyName => propertyName)
                    .Where(propertyNameGroup => propertyNameGroup.Count() > 1)
                    .Select(propertyNameGroupWithMultipleNames => propertyNameGroupWithMultipleNames.Key)
                    .ToList();

            if (duplicateNames.Any())
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"DupliatePropertyName{nameof(Exception)}", 
                    $"Command line properties must have unique names. Duplicates: {duplicateNames.Join(", ").EncloseWith("[]")}", 
                    null
                );
            }
        }

        //public static void ValidateParameterPositions(IReadOnlyCollection<ArgumentMetadata> parameters)
        //{
        //    var positions = parameters.Where(p => p.Position > 0).Select(p => p.Position).ToList();

        //    var mid = positions.Count % 2 == 0 ? 0 : (positions.Count / 2) + 1;
        //    var sum = (((1 + positions.Count) * (positions.Count / 2)) + mid);
        //    if (sum != positions.Sum())
        //    {
        //        // ReSharper disable once PossibleNullReferenceException
        //        throw new ArgumentException($"The {parameters.First().Property.DeclaringType.Name} has some invalid parameter positions. They must begin with 1 and have positions increasing by 1.");
        //    }            
        //}

        // todo: validate parameter types
    }
}
