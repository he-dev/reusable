using Reusable.Drawing;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz.Services
{
    public static class TypeConverterFactory
    {
        public static TypeConverter CreateDefaultConverter()
        {
            return TypeConverter.Empty
                .Add(
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
                    new StringToColorConverter(new ColorParser[]
                    {
                        new NameColorParser(),
                        new DecimalColorParser(),
                        new HexadecimalColorParser(),
                    }),
                    new StringToBooleanConverter(),
                    new StringToDateTimeConverter(),
                    new StringToEnumConverter(),
                    new SByteToStringConverter(),
                    new ByteToStringConverter(),
                    new CharToStringConverter(),
                    new Int16ToStringConverter(),
                    new Int32ToStringConverter(),
                    new Int64ToStringConverter(),
                    new UInt16ToStringConverter(),
                    new UInt32ToStringConverter(),
                    new UInt64ToStringConverter(),
                    new SingleToStringConverter(),
                    new DoubleToStringConverter(),
                    new DecimalToStringConverter(),
                    new ColorToStringConverter(),
                    new BooleanToStringConverter(),
                    new DateTimeToStringConverter(),
                    new EnumToStringConverter(),
                    new EnumerableToArrayConverter(),
                    new EnumerableToListConverter(),
                    new EnumerableToHashSetConverter(),
                    new DictionaryToDictionaryConverter()
                );
        }
    }
}