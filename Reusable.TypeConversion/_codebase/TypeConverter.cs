using System;
using System.Collections.Generic;
using System.Globalization;

namespace Reusable.TypeConversion
{
    public abstract class TypeConverter : IEqualityComparer<TypeConverter>
    {
        public static TypeConverter Empty => new EmptyConverter();

        public abstract Type FromType { get; }

        public abstract Type ToType { get; }

        public abstract bool CanConvert(object value, Type targetType);

        public object Convert(object value, Type targetType)
        {
            return Convert(value, targetType, null, CultureInfo.InvariantCulture);
        }

        public object Convert(object value, Type targetType, string format, IFormatProvider formatProvider)
        {
            return Convert(new ConversionContext<object>(value, targetType, format, formatProvider, Empty));
        }

        public object Convert(IConversionContext<object> context)
        {
            if (IsConverted(context.Value, context.TargetType))
            {
                return context.Value;
            }

            if (CanConvert(context.Value, context.TargetType))
            {
                return ConvertCore(context);
            }

            throw new FormatException($"Could not convert '{context.Value?.GetType()}' to '{context.TargetType}'.");
        }

        protected abstract object ConvertCore(IConversionContext<object> context);

        protected virtual bool IsConverted(object value, Type targetType)
        {
            return value.GetType() == targetType;
        }

        public override bool Equals(object obj)
        {
            return 
                !ReferenceEquals(obj, null) && 
                Equals(this, (TypeConverter)obj);
        }

        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        public bool Equals(TypeConverter x, TypeConverter y)
        {
            return 
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                x.FromType == y.FromType && 
                x.ToType == y.ToType;
        }

        public int GetHashCode(TypeConverter obj)
        {
            if (ReferenceEquals(obj.FromType, null) || ReferenceEquals(obj.ToType, null)) return 0;

            unchecked
            {
                var hash = 17;
                hash = hash * 31 + obj.FromType.GetHashCode();
                hash = hash * 31 + obj.ToType.GetHashCode();
                return hash;
            }
        }
    }

    public abstract class TypeConverter<TValue, TResult> : TypeConverter
    {
        public override Type FromType => typeof(TValue);

        public override Type ToType => typeof(TResult);

        public override bool CanConvert(object value, Type targetType)
        {
            return targetType.IsAssignableFrom(typeof(TResult)) && value.GetType() == typeof(TValue);
        }

        protected override object ConvertCore(IConversionContext<object> context)
        {
            return ConvertCore(new ConversionContext<TValue>(context));
        }

        protected abstract TResult ConvertCore(IConversionContext<TValue> context);
    }

    public class EmptyConverter : TypeConverter
    {
        public override Type FromType => null;

        public override Type ToType => null;

        public override bool CanConvert(object value, Type targetType)
        {
            return false;
        }

        protected override object ConvertCore(IConversionContext<object> context)
        {
            return null;
        }
    }

}
