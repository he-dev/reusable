using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Converters;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.Reflection;
using TypeConverter = Reusable.Converters.TypeConverter;
using System.Linq.Custom;

namespace Reusable.Commander
{
    public interface ICommandParameterMapper
    {
        T Map<T>([NotNull] T command, [NotNull] ICommandLine commandLine) where T : IConsoleCommand;
    }

    [UsedImplicitly]
    public class CommandParameterMapper : ICommandParameterMapper
    {
        private readonly ILogger _logger;
        private readonly ICommandRegistrationContainer _registrations;
        private readonly ITypeConverter _converter;

        public CommandParameterMapper(
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] ICommandRegistrationContainer registrations,
            [NotNull] ITypeConverter converter)
        {
            _logger = loggerFactory.CreateLogger(nameof(CommandParameterMapper));
            _registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public T Map<T>(T command, ICommandLine commandLine) where T : IConsoleCommand
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            // "Single" is fine because it's not possible to misconfigure the factory if commander-module is used so there is always a matching registration.
            var registration = _registrations.Single(r => r.CommandType == command.GetType());

            IEnumerable<string> GetValues(CommandParameter parameter)
            {
                if (parameter.Metadata.Position > CommandLine.CommandIndex)
                {
                    return commandLine.Anonymous().Skip(parameter.Metadata.Position).Take(1);
                }

                if (commandLine.Contains(parameter.Name))
                {
                    return commandLine[parameter.Name];
                }

                return null;
            }

            foreach (var parameter in registration)
            {
                _logger.Trace($"Mapping parameter {parameter.Name}.");

                var values = GetValues(parameter)?.ToList();

                // If parameter does not exist at all...
                if (values is null)
                {
                    // ...check if it should exist.
                    if (parameter.IsRequired)
                    {
                        throw ($"ArgumentNotFound{nameof(Exception)}", $"The required argument {parameter.Name} was not found.").ToDynamicException();
                    }

                    if (parameter.DefaultValue.IsNotNull())
                    {
                        parameter.SetValue(command, _converter.Convert(parameter.DefaultValue, parameter.Type));
                    }
                }
                else
                {
                    if (parameter.Type == typeof(bool))
                    {
                        var flag = values.SingleOrDefault();

                        // User did not specify any value but a flag alone.
                        if (flag is null)
                        {
                            if (parameter.DefaultValue is bool defaultValue)
                            {
                                parameter.SetValue(command, !defaultValue);
                            }
                            else
                            {
                                // If DefaultValue is not set then the default is false. This negates it.
                                parameter.SetValue(command, true);
                            }
                        }
                        else
                        {
                            var value = _converter.Convert(flag, typeof(bool));
                            parameter.SetValue(command, value);
                        }
                    }
                    else
                    {
                        // Parameter is required but not values are specified. This is an error.
                        if (parameter.IsRequired && values.None())
                        {
                            throw ($"ArgumentNotFound{nameof(Exception)}", $"The required argument {parameter.Name} was not found.").ToDynamicException();
                        }

                        var convertible = parameter.Type.IsEnumerable(ignore: typeof(string)) ? (object)values : values.SingleOrDefault();

                        var value = _converter.Convert(convertible, parameter.Type);
                        parameter.SetValue(command, value);                        
                    }
                }
            }

            return command;
        }

        [PublicAPI]
        public static readonly ITypeConverter DefaultConverter =
            TypeConverter.Empty
                .Add<StringToSByteConverter>()
                .Add<StringToByteConverter>()
                .Add<StringToCharConverter>()
                .Add<StringToInt16Converter>()
                .Add<StringToInt32Converter>()
                .Add<StringToInt64Converter>()
                .Add<StringToUInt16Converter>()
                .Add<StringToUInt32Converter>()
                .Add<StringToUInt64Converter>()
                .Add<StringToSingleConverter>()
                .Add<StringToDoubleConverter>()
                .Add<StringToDecimalConverter>()
                .Add<StringToColorConverter>()
                .Add<StringToBooleanConverter>()
                .Add<StringToDateTimeConverter>()
                .Add<StringToEnumConverter>()
                .Add<EnumerableToArrayConverter>();
    }
}