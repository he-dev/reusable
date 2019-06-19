using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.OneTo1.Converters.Collections.Generic;

namespace Reusable.Commander
{
    public static class CommandArgumentConverter
    {
        public static readonly ITypeConverter Default =
            TypeConverter
                .Empty
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
                .Add<EnumerableToListConverter>();
    }
}