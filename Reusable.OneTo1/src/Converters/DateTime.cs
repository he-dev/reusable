using System;
using System.Globalization;
using Reusable.Extensions;

namespace Reusable.OneTo1.Converters
{
    public class StringToDateTime : FromStringConverter<DateTime>
    {
        protected override DateTime Convert(string value, ConversionContext context)
        {
            return
                FormatString.IsNullOrEmpty()
                    ? DateTime.Parse(value, FormatProvider, DateTimeStyles.None)
                    : DateTime.ParseExact(value, FormatString, FormatProvider, DateTimeStyles.None);
        }
    }

    public class DateTimeToString : ToStringConverter<DateTime>
    {
        protected override string Convert(DateTime value, ConversionContext context)
        {
            return
                FormatString.IsNullOrEmpty()
                    ? value.ToString(FormatProvider)
                    : value.ToString(FormatString, FormatProvider);
        }
    }
}