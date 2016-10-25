using System;

namespace Reusable.Converters
{
    public class StringToEnumConverter : GenericConverter<String>
    {
        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            if (context.Type.BaseType == typeof(Enum) && arg is String)
            {
                instance = Convert((string)arg, context);
                return true;
            }

            instance = null;
            return false;
        }

        public override object Convert(String value, ConversionContext context)
        {
            return Enum.Parse(context.Type, value);
        }
    }

    public class EnumToStringConverter : GenericConverter<Enum>
    {
        public override bool TryConvert(ConversionContext context, object arg, out object instance)
        {
            if (context.Type == typeof(String) && arg.GetType().BaseType == typeof(Enum))
            {
                instance = Convert((Enum)arg, context);
                return true;
            }

            instance = null;
            return false;
        }

        public override object Convert(Enum value, ConversionContext context)
        {
            return value.ToString();
        }
    }
}
