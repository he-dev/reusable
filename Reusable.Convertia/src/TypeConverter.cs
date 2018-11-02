using System;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Convertia
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

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class TypeConverter : ITypeConverter
    {
        public static TypeConverter Empty => new CompositeConverter();

        public static string DefaultFormat { get; } = string.Empty;

        public static IFormatProvider DefaultFormatProvider { get; } = CultureInfo.InvariantCulture;

        public abstract Type FromType { get; }

        public abstract Type ToType { get; }

        private string DebuggerDisplay => this is CompositeConverter ? nameof(CompositeConverter) : this.ToDebuggerDisplayString(builder =>
        {
            builder.Property(x => x.FromType);
            builder.Property(x => x.ToType);
        });

        public virtual bool CanConvert(Type fromType, Type toType)
        {
            TypeConverterHelper.AssertNotNull(fromType, toType);
            return IsConverted(fromType, toType) || (fromType == FromType && toType.IsAssignableFrom(ToType));
        }

        public object Convert(IConversionContext<object> context)
        {
            return
                IsConverted(context.FromType, context.ToType)
                    ? context.Value
                    : CanConvert(context.FromType, context.ToType)
                        ? ExecuteConvertCore()
                        : throw DynamicException.Create(
                            $"UnsupportedConversion",
                            $"There is no converter from '{context.FromType.ToPrettyString()}' to '{context.ToType.ToPrettyString()}'.");

            // This wrapps the inner exception.
            object ExecuteConvertCore()
            {
                try { return ConvertCore(context); }
                catch (Exception inner)
                {
                    throw DynamicException.Create(
                        $"Conversion",
                        $"Could not convert from '{context.FromType.ToPrettyString()}' to '{context.ToType.ToPrettyString()}'.",
                        inner);
                }
            }
        }

        [NotNull]
        protected abstract object ConvertCore([NotNull] IConversionContext<object> context);

        protected virtual bool IsConverted(Type fromType, Type toType)
        {
            return toType.IsAssignableFrom(fromType);
        }

        #region IEquatable

        public override int GetHashCode() => AutoEquality<ITypeConverter>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => Equals(obj as ITypeConverter);

        public bool Equals(ITypeConverter other) => AutoEquality<ITypeConverter>.Comparer.Equals(this, other);

        #endregion
    }

    public abstract class TypeConverter<TValue, TResult> : TypeConverter
    {
        public override Type FromType => typeof(TValue);

        public override Type ToType => typeof(TResult);

        protected override object ConvertCore(IConversionContext<object> context) => ConvertCore(new ConversionContext<TValue>((TValue)context.Value, context.ToType, context.Converter)
        {
            Format = context.Format,
            FormatProvider = context.FormatProvider
        });

        protected abstract TResult ConvertCore(IConversionContext<TValue> context);
    }

    //public class EmptyConverter : TypeConverter
    //{
    //    public override Type FromType => typeof(object);

    //    public override Type ToType => typeof(object);

    //    public override bool CanConvert(Type fromType, Type toType) => fromType == toType;

    //    protected override object ConvertCore(IConversionContext<object> context) => context.Value;
    //}

    internal static class TypeConverterHelper
    {
        [ContractAnnotation("fromType: null => halt; toType: null => halt")]
        public static void AssertNotNull([CanBeNull] Type fromType, [CanBeNull] Type toType)
        {
            if (fromType == null) throw new ArgumentNullException(nameof(fromType));
            if (toType == null) throw new ArgumentNullException(nameof(toType));
        }
    }
}