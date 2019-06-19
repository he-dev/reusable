using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.Reflection;

namespace Reusable.Commander
{
    internal static class CommandLineValidator
    {
        public static void Validate(ICommandLine commandLine, Type commandType, ITypeConverter argumentConverter)
        {
            //ValidateCommandName(command.Id);
            //ValidateParameters(command.Type, converter);
        }

//        private void ValidateCommandName(Identifier id)
//        {
//            if (!_commandNames.Add(id))
//            {
//                throw DynamicException.Create(
//                    $"DuplicateCommandName",
//                    $"Another command with the name {id} has already been added."
//                );
//            }
//        }

        private static void ValidateParameters(Type commandType, ITypeConverter converter)
        {            
            var parameters =
                commandType
                    .GetBagType()
                    .GetParameters()
                    .ToList();

            ValidateParameterNames(parameters);
            ValidateParameterPositions(parameters);
            ValidateParameterTypes(parameters, converter);
        }

        private static void ValidateParameterNames(IEnumerable<CommandArgumentMetadata> parameters)
        {
            var duplicateNames =
                parameters
                    .SelectMany(property => property.Id)
                    .GroupBy(propertyName => propertyName)
                    .Where(propertyNameGroup => propertyNameGroup.Count() > 1)
                    .Select(propertyNameGroupWithMultipleNames => propertyNameGroupWithMultipleNames.Key)
                    .ToList();

            if (duplicateNames.Any())
            {
                throw DynamicException.Create(
                    $"DuplicateParameterName",
                    $"There are one or more parameters with duplicate names: {duplicateNames.Join(", ").EncloseWith("[]")}"
                );
            }
        }

        private static void ValidateParameterPositions(IEnumerable<CommandArgumentMetadata> parameters)
        {
            var positions =
                parameters
                    .Where(p => p.Position > 0)
                    .Select(p => p.Position)
                    .ToList();

            if (positions.Any())
            {
                var mid = positions.Count % 2 == 0 ? 0 : (positions.Count / 2) + 1;
                var sum = (((1 + positions.Count) * (positions.Count / 2)) + mid);
                if (sum != positions.Sum())
                {
                    throw DynamicException.Create(
                        $"ParameterPosition",
                        $"There are one or more parameters with invalid positions. They must begin with 1 and be increasing by 1."
                    );
                }
            }
        }

        private static void ValidateParameterTypes(IEnumerable<CommandArgumentMetadata> parameters, ITypeConverter converter)
        {            
            var unsupportedParameters =
                parameters
                    .Where(parameter =>
                        {
                            var sourceType = parameter.Property.PropertyType.IsEnumerableOfT(except: typeof(string)) ? typeof(IEnumerable<string>) : typeof(string);
                            return !converter.CanConvert(sourceType, parameter.Property.PropertyType);
                        }
                    )
                    .ToList();

            if (unsupportedParameters.Any())
            {
                throw DynamicException.Create(
                    $"UnsupportedParameterType",
                    $"There are one or more parameters with unsupported types: {string.Join(", ", unsupportedParameters.Select(p => p.Property.PropertyType.Name)).EncloseWith("[]")}."
                );
            }
        }

        public static void ValidateCommandType(Type type)
        {
            if (!typeof(ICommand).IsAssignableFrom(type))
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"CommandType",
                    $"{type.Name} is not derived from {typeof(ICommand).Name}."
                );
            }
        }
    }
}