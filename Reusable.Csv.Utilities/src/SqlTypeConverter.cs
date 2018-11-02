using System;
using Reusable.Convertia;
using Reusable.Convertia.Converters;

namespace Reusable.Csv.Utilities
{
    public class SqlTypeConverter : TypeConverter
    {
        private static readonly ITypeConverter Converter =
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

        public override Type FromType => Converter.FromType;

        public override Type ToType => Converter.ToType;

        protected override object ConvertCore(IConversionContext<object> context)
        {
            return Converter.Convert(context);
        }
    }
}
