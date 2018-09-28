using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Converters;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Commander
{
    internal class CommandValidator
    {
        private readonly ISet<SoftKeySet> _commandNames = new HashSet<SoftKeySet>();

        public (Type Type, SoftKeySet Name) ValidateCommand((Type Type, SoftKeySet Name) command, [NotNull] ITypeConverter converter)
        {
            if (command.Type == null) throw new ArgumentNullException(nameof(command.Type));
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            if (!typeof(IConsoleCommand).IsAssignableFrom(command.Type))
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"CommandType{nameof(Exception)}",
                    $"{command.Type.Name} is not derived from {typeof(IConsoleCommand).Name}."
                );
            }

            ValidateCommandName(command.Name);
            ValidateParameters(command.Type, converter);

            return command;
        }        

        private void ValidateCommandName(SoftKeySet name)
        {
            if (!_commandNames.Add(name))
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"DuplicateCommandName{nameof(Exception)}",
                    $"Command names and aliases must be unique but there are duplicates: {name}"
                );
            }
        }

        private static void ValidateParameters(Type commandType, ITypeConverter converter)
        {
            // ReSharper disable once PossibleNullReferenceException
            // The first validation makes sure that this is never null.
            var bagType =
                commandType
                    .BaseType
                    .GetGenericArguments()
                    .Single(t => typeof(ICommandBag).IsAssignableFrom(t));

            var parameters =
                bagType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Select(CommandParameter.Create)
                    .ToList();

            ValidateParameterNames(parameters);
            ValidateParameterPositions(parameters);
            ValidateParameterTypes(parameters, converter);
        }

        private static void ValidateParameterNames(IEnumerable<CommandParameter> parameters)
        {
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
                    $"DuplicateParameterName{nameof(Exception)}",
                    $"There are one or more parameters with duplicate names: {duplicateNames.Join(", ").EncloseWith("[]")}"
                );
            }
        }

        private static void ValidateParameterPositions(IEnumerable<CommandParameter> parameters)
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
                    throw DynamicException.Factory.CreateDynamicException(
                        $"ParameterPosition{nameof(Exception)}",
                        $"There are one or more parameters with invalid positions. They must begin with 1 and be increasing by 1."
                    );
                }
            }
        }

        private static void ValidateParameterTypes(IEnumerable<CommandParameter> parameters, ITypeConverter converter)
        {
            var unsupportedParameters =
                parameters
                    .Where(parameter => !converter.CanConvert(typeof(string), parameter.Type))
                    .ToList();

            if (unsupportedParameters.Any())
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"UnsupportedParameterType{nameof(Exception)}",
                    $"There are one or more parameters with unsupported types: {string.Join(", ", unsupportedParameters.Select(p => p.Type.Name)).EncloseWith("[]")}."
                );
            }
        }
    }
}