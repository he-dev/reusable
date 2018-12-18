using System;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.Exceptionizer;
using Reusable.Extensions;

//namespace Reusable.OneTo1
namespace Reusable.OneTo1
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

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class TypeConverter : ITypeConverter
    {
        public static TypeConverter Empty => new CompositeConverter();

        //public static string DefaultFormat { get; } = default;

        public static IFormatProvider DefaultFormatProvider { get; } = CultureInfo.InvariantCulture;

        public abstract Type FromType { get; }

        public abstract Type ToType { get; }

        private string DebuggerDisplay => this is CompositeConverter ? nameof(CompositeConverter) : this.ToDebuggerDisplayString(builder =>
        {
            builder.Property(x => x.FromType);
            builder.Property(x => x.ToType);
        });

        public bool CanConvert(Type fromType, Type toType)
        {
            if (fromType == null) throw new ArgumentNullException(nameof(fromType));
            if (toType == null) throw new ArgumentNullException(nameof(toType));

            return IsConverted(fromType, toType) || SupportsConversion(fromType, toType);
        }

        public object Convert(IConversionContext<object> context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (IsConverted(context.FromType, context.ToType))
            {
                return context.Value;
            }

            try
            {
                return
                    CanConvert(context.FromType, context.ToType)
                        ? ConvertCore(context)
                        : throw DynamicException.Create
                        (
                            $"UnsupportedConversion",
                            $"There is no converter from '{context.FromType.ToPrettyString()}' to '{context.ToType.ToPrettyString()}'."
                        );
            }
            catch (Exception inner)
            {
                throw DynamicException.Create
                (
                    $"Conversion",
                    $"Could not convert from '{context.FromType.ToPrettyString()}' to '{context.ToType.ToPrettyString()}'.",
                    inner
                );
            }
        }

        [NotNull]
        protected abstract object ConvertCore([NotNull] IConversionContext<object> context);

        protected virtual bool SupportsConversion(Type fromType, Type toType) => fromType == FromType && toType.IsAssignableFrom(ToType);

        private static bool IsConverted(Type fromType, Type toType) => toType.IsAssignableFrom(fromType);

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

        protected override object ConvertCore(IConversionContext<object> context)
        {
            return ConvertCore(new ConversionContext<TValue>((TValue)context.Value, context.ToType, context.Converter)
            {
                Format = context.Format,
                FormatProvider = context.FormatProvider
            });
        }

        [NotNull]
        protected abstract TResult ConvertCore(IConversionContext<TValue> context);
    }

    /// <summary>
    /// Passes the value to be converted through without doing anything.
    /// </summary>
    public class RelayConverter : TypeConverter
    {
        public override Type FromType => typeof(object);

        public override Type ToType => typeof(object);

        protected override bool SupportsConversion(Type fromType, Type toType) => true;

        protected override object ConvertCore(IConversionContext<object> context) => context.Value;
    }
}