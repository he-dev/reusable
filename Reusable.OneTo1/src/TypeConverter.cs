using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.OneTo1
{
    public interface ITypeConverter
    {
        object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default);
    }

    public class SkipConvert : ITypeConverter
    {
        private readonly ITypeConverter _converter;

        public SkipConvert(ITypeConverter converter) => _converter = converter;

        public virtual object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            return
                toType.IsInstanceOfType(value)
                    ? value
                    : _converter.ConvertOrDefault(value, toType, context);
        }
    }

    public class TypeConvertException : ITypeConverter
    {
        private readonly ITypeConverter _converter;

        public TypeConvertException(ITypeConverter converter) => _converter = converter;

        public object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            try
            {
                return _converter.ConvertOrDefault(value, toType, context ?? new ConversionContext());
            }
            catch (Exception inner)
            {
                throw DynamicException.Create
                (
                    $"TypeConversion",
                    $"Could not convert from '{value.GetType().ToPrettyString()}' to '{toType.ToPrettyString()}'.",
                    inner
                );
            }
        }
    }

    public abstract class TypeConverter<TValue, TResult> : ITypeConverter
    {
        public virtual object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default)
        {
            return
                value is TValue x && toType == typeof(TResult)
                    ? Convert(x, context ?? new ConversionContext { Converter = this })
                    : default;
        }

        protected abstract TResult Convert(TValue value, ConversionContext context);
    }

    public static class Factory
    {
        public static T Create<T>(Func<T> factory) => factory();
    }

    public delegate T DecorateDelegate<T>(T decoratee);

    public class Decorator<T> : IDisposable
    {
        private readonly IEnumerable<DecorateDelegate<T>> _decorators;

        private Decorator(IEnumerable<DecorateDelegate<T>> decorators) => _decorators = decorators.ToList();

        public static IEnumerable<Decorator<T>> Current => AsyncScope<Decorator<T>>.Current.Enumerate().Select(scope => scope.Value);

        public static Decorator<T> BeginScope(params DecorateDelegate<T>[] decorators) => AsyncScope<Decorator<T>>.Push(new Decorator<T>(decorators));

        public T Decorate(T decoratee) => _decorators.Aggregate(decoratee, (current, decorate) => decorate(current));

        public T Decorate<TDecoratee>() where TDecoratee : T, new() => Decorate(new TDecoratee());

        public void Dispose() => AsyncScope<Decorator<T>>.Current?.Dispose();
    }

    public class AsyncScope<T> : IDisposable
    {
        private static readonly AsyncLocal<AsyncScope<T>> State = new AsyncLocal<AsyncScope<T>>();

        private AsyncScope(T value) => Value = value;

        public T Value { get; }

        public AsyncScope<T>? Parent { get; private set; }

        public static AsyncScope<T>? Current
        {
            get => State.Value;
            private set => State.Value = value!;
        }

        /// <summary>
        /// Gets a value indicating whether there are any states on the stack.
        /// </summary>
        public static bool Any => Current is {};

        public static AsyncScope<T> Push(T value)
        {
            return Current = new AsyncScope<T>(value) { Parent = Current };
        }

        public void Dispose() => Current = Current?.Parent;

        public static implicit operator T(AsyncScope<T> scope) => scope.Value;
    }

    public static class AsyncScopeExtensions
    {
        public static IEnumerable<AsyncScope<T>> Enumerate<T>(this AsyncScope<T>? scope)
        {
            for (; scope is {}; scope = scope.Parent) yield return scope;
        }
    }
}