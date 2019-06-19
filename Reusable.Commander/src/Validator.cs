﻿using System;
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
    internal static class Validator
    {
        public static void ValidateCommand(Type commandType, ITypeConverter argumentConverter)
        {
            var commandArguments =
                commandType
                    .GetCommandArgumentGroupType()
                    .GetCommandArgumentMetadata()
                    .ToList();

            ValidateArgumentNames(commandArguments);
            ValidateArgumentPositions(commandArguments);
            ValidateArgumentTypes(commandArguments, argumentConverter);
        }

        private static void ValidateArgumentNames(IEnumerable<CommandArgumentMetadata> parameters)
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
                throw DynamicException.Create
                (
                    $"DuplicateArgumentName",
                    $"There is one or more arguments with duplicate names: {duplicateNames.Join(", ").EncloseWith("[]")}"
                );
            }
        }

        private static void ValidateArgumentPositions(IEnumerable<CommandArgumentMetadata> parameters)
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
                    throw DynamicException.Create
                    (
                        $"ArgumentPosition",
                        $"There is one or more arguments with invalid positions. They must begin with 1 and be increasing by 1."
                    );
                }
            }
        }

        private static void ValidateArgumentTypes(IEnumerable<CommandArgumentMetadata> parameters, ITypeConverter converter)
        {
            var unsupportedParameters =
                parameters
                    .Where(parameter =>
                        {
                            var sourceType = parameter.Property.PropertyType.IsEnumerableOfT(except: typeof(string)) ? typeof(IEnumerable<string>) : typeof(string);
                            return !converter.CanConvert(sourceType, parameter.Property.PropertyType);
                        }
                    ).ToList();

            if (unsupportedParameters.Any())
            {
                throw DynamicException.Create(
                    $"UnsupportedParameterType",
                    $"There are one or more parameters with unsupported types: {string.Join(", ", unsupportedParameters.Select(p => p.Property.PropertyType.Name)).EncloseWith("[]")}."
                );
            }
        }

        public static void ValidateCommandLine(ICommandLine commandLine, Type commandType)
        {
            var commandArguments =
                commandType
                    .GetCommandArgumentGroupType()
                    .GetCommandArgumentMetadata()
                    .Where(m => m.Required)
                    .ToList();

            foreach (var commandArgument in commandArguments)
            {
                if (commandLine[commandArgument.Id] is null)
                {
                    throw DynamicException.Create
                    (
                        $"CommandArgumentNotFound",
                        $"Argument '{commandArgument.Id.Default.ToString()}' is required but is missing."
                    );
                }
            }
        }
    }
}