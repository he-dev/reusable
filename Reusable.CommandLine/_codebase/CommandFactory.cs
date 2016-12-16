using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reusable.Converters;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Shelly.Data;
using Reusable.Shelly.Reflection;

namespace Reusable.Shelly
{
    internal class CommandFactory
    {
        private readonly TypeConverter _defaultConverter = TypeConverter.Empty.Add(new TypeConverter[]
        {
            new StringToSByteConverter(),
            new StringToByteConverter(),
            new StringToCharConverter(),
            new StringToInt16Converter(),
            new StringToInt32Converter(),
            new StringToInt64Converter(),
            new StringToUInt16Converter(),
            new StringToUInt32Converter(),
            new StringToUInt64Converter(),
            new StringToSingleConverter(),
            new StringToDoubleConverter(),
            new StringToDecimalConverter(),
            new StringToColorConverter(),
            new StringToBooleanConverter(),
            new StringToDateTimeConverter(),
            new StringToEnumConverter(),

            new EnumerableObjectToArrayObjectConverter(),
            new EnumerableObjectToListObjectConverter(),
            new EnumerableObjectToHashSetObjectConverter(),
            new DictionaryObjectObjectToDictionaryObjectObjectConverter(),
        });

        internal CommandFactory() { }

        public Command CreateCommand(CommandInfo commandInfo, IEnumerable<IGrouping<string, string>> arguments, CommandLine commandLine)
        {
            commandInfo.Validate(nameof(commandInfo)).IsNotNull();
            arguments.Validate(nameof(arguments)).IsNotNull();
            commandLine.Validate(nameof(commandLine)).IsNotNull();

            // todo: implement with Autofac
            var requriesCommandLine = commandInfo.CommandType.GetConstructor(new[] { typeof(CommandLine) }.Concat(commandInfo.Args.Select(x => x.GetType())).ToArray()) != null;

            var command =
                requriesCommandLine
                ? (Command)Activator.CreateInstance(commandInfo.CommandType, new object[] { commandLine }.Concat(commandInfo.Args).ToArray())
                : (Command)Activator.CreateInstance(commandInfo.CommandType, commandInfo.Args);

            //PopulateProperties(command, arguments);
            return command;
        }

        private void PopulateProperties(Command command, IEnumerable<IGrouping<string, string>> arguments)
        {
            //var customConverter = commandType.GetCustomAttribute<ConvertersAttribute>() ?? Enumerable.Empty<Type>();
            var customConverter = Enumerable.Empty<Type>();
            var converter = customConverter.Aggregate(_defaultConverter, (current, c) => current.Add(c));

            foreach (var commandProperty in command.GetProperties())
            {
                var values = Enumerable.Empty<string>();

                var argument = arguments.SingleOrDefault(a => commandProperty.Names.Contains(a.Key, StringComparer.OrdinalIgnoreCase));
                if (argument == null)
                {
                    if (commandProperty.Mandatory)
                    {
                        throw new Exception($"Mandatory parameter [{string.Join(", ", commandProperty.Names)}] not found.");
                    }
                    continue;
                }

                if (commandProperty.Type.IsEnumerable())
                {
                    if (commandProperty.ListSeparator != char.MinValue)
                    {
                        values = values.FirstOrDefault()?.Split(commandProperty.ListSeparator);
                    }
                    var value = converter.Convert(values, commandProperty.Type);
                    commandProperty.SetValue(command, value);
                }
                else
                {
                    var arg = values.FirstOrDefault();
                    if (string.IsNullOrEmpty(arg))
                    {
                        continue;
                    }
                    var value = converter.Convert(arg, commandProperty.Type);
                    commandProperty.SetValue(command, value);
                }
            }
        }
    }
}