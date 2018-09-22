using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Commander
{
    internal static class CommandValidator
    {
        public static void ValidateCommand([NotNull] Type commandType)
        {
            if (!typeof(IConsoleCommand).IsAssignableFrom(commandType))
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"CommandType{nameof(Exception)}",
                    $"{commandType.Name} is not derived from {typeof(IConsoleCommand).Name}.");
            }

            var bagType = commandType.BaseType.GetGenericArguments().SingleOrDefault(t => typeof(ICommandBag).IsAssignableFrom(t));

            if (bagType is null)
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"CommandBag{nameof(Exception)}",
                    $"{commandType.Name}'s bag is not derived from {typeof(ICommandBag).Name}.");
            }

            var parameters = bagType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Select(CommandParameter.Create);

            var duplicateNames =
                parameters
                    .SelectMany(property => property.Name)
                    .GroupBy(propertyName => propertyName)
                    .Where(propertyNameGroup => propertyNameGroup.Count() > 1)
                    .Select(propertyNameGroupWithMultipleNames => propertyNameGroupWithMultipleNames.Key)
                    .ToList();

            if (duplicateNames.Any())
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"DuplicatePropertyName{nameof(Exception)}",
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
