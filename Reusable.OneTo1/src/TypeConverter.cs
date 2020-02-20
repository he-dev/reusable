using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.OneTo1
{
    public interface ITypeConverter : IEquatable<ITypeConverter>
    {
        [AutoEqualityProperty]
        Type FromType { get; }

        [AutoEqualityProperty]
        Type ToType { get; }

        bool CanConvert(Type fromType, Type toType);

        object Convert(IConversionContext<object> context);
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class TypeConverter : ITypeConverter
    {
        public static TypeConverter Empty => new CompositeConverter();

        public static TypeConverter PassThru { get; } = new _PassThru();

        public abstract Type FromType { get; }

        public abstract Type ToType { get; }

        private string DebuggerDisplay =>
            this is CompositeConverter
                ? nameof(CompositeConverter)
                : this.ToDebuggerDisplayString(b =>
                {
                    b.DisplaySingle(x => x.FromType);
                    b.DisplaySingle(x => x.ToType);
                });

        public virtual bool CanConvert(Type fromType, Type toType)
        {
            if (fromType == null) throw new ArgumentNullException(nameof(fromType));
            if (toType == null) throw new ArgumentNullException(nameof(toType));

            return IsConverted(fromType, toType) || CanConvertCore(fromType, toType);
        }

        protected virtual bool CanConvertCore(Type fromType, Type toType)
        {
            return fromType == FromType && toType.IsAssignableFrom(ToType);
        }

        public abstract object Convert(IConversionContext<object> context);

        private static bool IsConverted(Type fromType, Type toType) => toType.IsAssignableFrom(fromType);

        #region IEquatable

        public override int GetHashCode() => AutoEquality<ITypeConverter>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => Equals(obj as ITypeConverter);

        public bool Equals(ITypeConverter other) => AutoEquality<ITypeConverter>.Comparer.Equals(this, other);

        #endregion

        private class _PassThru : TypeConverter
        {
            public override Type FromType => typeof(object);

            public override Type ToType => typeof(object);

            public override bool CanConvert(Type fromType, Type toType) => true;

            public override object Convert(IConversionContext<object> context) => context.Value;
        }

        public abstract class Decorator : TypeConverter
        {
            private readonly ITypeConverter _converter;
            protected Decorator(ITypeConverter converter) => _converter = converter;
            public override Type FromType => _converter.FromType;
            public override Type ToType => _converter.ToType;
            public override bool CanConvert(Type fromType, Type toType) => _converter.CanConvert(fromType, toType);
            public override object Convert(IConversionContext<object> context) => _converter.Convert(context);
        }
    }

    public class SkipConversionForSameTypes : TypeConverter.Decorator
    {
        public SkipConversionForSameTypes(ITypeConverter converter) : base(converter) { }

        public override object Convert(IConversionContext<object> context)
        {
            return
                context.SameTypes
                    ? context.Value
                    : base.Convert(context);
        }
    }

    public class ThrowConversionException : TypeConverter.Decorator
    {
        public ThrowConversionException(ITypeConverter converter) : base(converter) { }

        public override object Convert(IConversionContext<object> context)
        {
            if (CanConvert(context.FromType, context.ToType))
            {
                try
                {
                    return base.Convert(context);
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
            else
            {
                throw DynamicException.Create
                (
                    $"UnsupportedConversion",
                    $"There is no converter from '{context.FromType.ToPrettyString()}' to '{context.ToType.ToPrettyString()}'."
                );
            }
        }
    }

    public class TypeConverterBuilder
    {
        private readonly List<Type> _decorators = new List<Type>();
        private readonly List<Type> _converters = new List<Type>();
        
        public static TypeConverterBuilder Empty => new TypeConverterBuilder();

        public TypeConverterBuilder DecorateWith<T>() where T : TypeConverter.Decorator
        {
            _decorators.Add(typeof(T));
            return this;
        }

        public TypeConverterBuilder Use<T>() where T : ITypeConverter, new()
        {
            _converters.Add(typeof(T));
            return this;
        }

        public ITypeConverter Build()
        {
            return _decorators.Aggregate<Type, ITypeConverter>(new T(), (current, decoratorType) => (ITypeConverter)Activator.CreateInstance(decoratorType, current));
        }
    }

    public interface ITypeConverter<TValue, TResult> : ITypeConverter { }

    public abstract class TypeConverter<TValue, TResult> : TypeConverter, ITypeConverter<TValue, TResult>
    {
        public override Type FromType => typeof(TValue);

        public override Type ToType => typeof(TResult);

        public override object Convert(IConversionContext<object> context)
        {
            return Convert(new ConversionContext<TValue>((TValue)context.Value, context.ToType, context.Converter)
            {
                Format = context.Format,
                FormatProvider = context.FormatProvider
            });
        }

        protected abstract TResult Convert(IConversionContext<TValue> context);
    }
}