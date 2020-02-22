using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;

namespace Reusable.OneTo1
{
    public interface ITypeConverter
    {
        object? ConvertOrDefault(object value, Type toType, ConversionContext? context = default);
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

    public interface IDecorator<out TDecoratee>
    {
        TDecoratee Decoratee { get; }
    }

    public delegate T DecorateDelegate<T>(T decoratee);

    public static class DecoratorScope
    {
        public static DecoratorScope<T> For<T>() => DecoratorScope<T>.Begin();
    }

    public class DecoratorScope<T> : IDisposable, IEnumerable<DecorateDelegate<T>>
    {
        private readonly Stack<DecorateDelegate<T>> _decorators;

        public DecoratorScope() => _decorators = new Stack<DecorateDelegate<T>>();

        public static DecoratorScope<T>? Current => AsyncScope<DecoratorScope<T>>.Current?.Value;

        public static DecoratorScope<T> Begin() => AsyncScope<DecoratorScope<T>>.Push(new DecoratorScope<T>());

        public DecoratorScope<T> Add(DecorateDelegate<T> decorator) => this.Pipe(t => t._decorators.Add(decorator));

        public DecoratorScope<T> Add<TDecorator>() => Add(decoratee => (T)Activator.CreateInstance(typeof(TDecorator), decoratee));

        public T Decorate(T decoratee) => this.Aggregate(decoratee is IDecorator<T> decorator ? decorator.Decoratee : decoratee, (current, decorate) => decorate(current));

        public T Decorate<TDecoratee>() where TDecoratee : T, new() => Decorate(new TDecoratee());

        public IEnumerator<DecorateDelegate<T>> GetEnumerator()
        {
            var decorators =
                from s in AsyncScope<DecoratorScope<T>>.Current.Enumerate()
                from d in _decorators
                select d;

            return decorators.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose() => AsyncScope<DecoratorScope<T>>.Current?.Dispose();
    }
}