using Reusable.Converters;

namespace Reusable.FileFormats.Csv.SqlExtensions
{
    public static class SqlTypeConverter
    {
        public static readonly ITypeConverter Default =
            TypeConverter
                .Empty
                .Add<StringToInt16Converter>()
                .Add<StringToInt32Converter>()
                .Add<StringToInt64Converter>()
                .Add<StringToSingleConverter>()
                .Add<StringToDoubleConverter>()
                .Add<StringToTimeSpanConverter>()
                .Add<StringToByteConverter>()
                .Add<StringToDecimalConverter>()
                .Add<StringToDateTimeOffsetConverter>()
                .Add<StringToGuidConverter>()
                //.Add<StringBase64ToByteArrayConverter>()
                .Add<StringToDateTimeConverter>();
    }
}
