using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.Converters
{
    public interface ITypeConverter : IEquatable<ITypeConverter>
    {
        [NotNull]
        [AutoEqualityProperty]
        Type FromType { get; }

        [NotNull]
        [AutoEqualityProperty]
        Type ToType { get; }

        bool CanConvert([NotNull] Type fromType, [NotNull] Type toType);

        [NotNull]
        object Convert([NotNull] IConversionContext<object> context);
    }

    public abstract class TypeConverter : ITypeConverter
    {
        public static TypeConverter Empty => new EmptyConverter();

        public abstract Type FromType { get; }

        public abstract Type ToType { get; }

        public virtual bool CanConvert(Type fromType, Type toType)
        {
            if (fromType == null) throw new ArgumentNullException(nameof(fromType));
            if (toType == null) throw new ArgumentNullException(nameof(toType));
            
            return toType.IsAssignableFrom(ToType) && fromType == FromType;
        }

        public object Convert(IConversionContext<object> context)
        {
            if (IsConverted(context.FromType, context.ToType))
            {
                return context.Value;
            }

            if (CanConvert(context.FromType, context.ToType))
            {
                return ConvertCore(context);
            }

            throw new FormatException($"Could not convert '{context.Value.GetType()}' to '{context.ToType}'.");
        }

        protected abstract object ConvertCore([NotNull] IConversionContext<object> context);

        protected virtual bool IsConverted(Type fromType, Type toType)
        {
            return toType.IsAssignableFrom(fromType);
        }

        public bool Equals(ITypeConverter other) => AutoEquality<ITypeConverter>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as ITypeConverter);

        public override int GetHashCode() => AutoEquality<ITypeConverter>.Comparer.GetHashCode(this);
    }

    public abstract class TypeConverter<TValue, TResult> : TypeConverter
    {
        public override Type FromType => typeof(TValue);

        public override Type ToType => typeof(TResult);

        protected override object ConvertCore(IConversionContext<object> context)
        {
            return ConvertCore(new ConversionContext<TValue>((TValue)context.Value, context.ToType)
            {
                Format = context.Format,
                FormatProvider = context.FormatProvider,
                Converter = context.Converter
            });
        }

        protected abstract TResult ConvertCore(IConversionContext<TValue> context);
    }

    public class EmptyConverter : TypeConverter
    {
        public override Type FromType => typeof(object);

        public override Type ToType => typeof(object);

        public override bool CanConvert(Type fromType, Type toType)
        {
            return fromType == toType;
        }

        protected override object ConvertCore(IConversionContext<object> context)
        {
            return context.Value;
        }
    }
}