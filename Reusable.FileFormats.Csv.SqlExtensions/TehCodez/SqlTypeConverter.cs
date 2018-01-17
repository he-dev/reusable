using Reusable.Converters;

namespace Reusable
{
    public static class SqlTypeConverter
    {
        public static readonly ITypeConverter Default =
            TypeConverter
                .Empty
                .Add<StringToInt32Converter>()
                .Add<StringToDateTimeConverter>();
    }
}
