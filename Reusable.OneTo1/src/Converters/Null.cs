using System;

namespace Reusable.OneTo1.Converters
{
    /// <summary>
    /// Passes the value to be converted through without doing anything.
    /// </summary>
    public class NullConverter : TypeConverter
    {
        public override Type FromType => typeof(object);

        public override Type ToType => typeof(object);

        protected override bool CanConvertCore(Type fromType, Type toType) => true;

        protected override object ConvertCore(IConversionContext<object> context) => context.Value;
    }
}