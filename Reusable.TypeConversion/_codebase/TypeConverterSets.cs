using Reusable.Converters.Converters;

namespace Reusable.Converters
{
    public static class TypeConverterSets
    {
        public static readonly TypeConverter BasicConverter = TypeConverter.Empty.Add(new TypeConverter[]
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
            new StringToBooleanConverter(),
            new StringToDateTimeConverter(),
            new StringToTimeSpanConverter(), 
            new StringToEnumConverter(),
            new StringToColorConverter(),

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
            new BooleanToStringConverter(),
            new DateTimeToStringConverter(),
            new TimeSpanToStringConverter(), 
            new EnumToStringConverter(),
            new ColorToStringConverter(),
        });

        public static readonly TypeConverter EnumerableConverter = TypeConverter.Empty.Add(new TypeConverter[]
        {
            new DictionaryObjectObjectToDictionaryObjectObjectConverter(),
            new EnumerableObjectToArrayObjectConverter(), 
            new EnumerableObjectToHashSetObjectConverter(), 
            new EnumerableObjectToListObjectConverter(), 
        });

        public static readonly TypeConverter AdditionalConverter = TypeConverter.Empty.Add(new TypeConverter[]
        {
            new StringToXElementConverter(), 
            new StringToXDocumentConverter(), 
            
            new XElementToStringConverter(), 
            new XDocumentToStringConverter(), 
        });
    }
}
