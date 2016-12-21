using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Converters;

namespace Reusable.Shelly.Reflection
{
    internal class CommandPropertyMapper
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

        public void PopulateProperties(Command command, IEnumerable<IGrouping<string, string>> arguments)
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
