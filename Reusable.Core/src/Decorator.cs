using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;

namespace Reusable
{
    public static class Factory
    {
        public static T Create<T>(Func<T> factory) => factory();
    }

    public interface IDecorator<out TDecoratee>
    {
        TDecoratee Decoratee { get; }
    }

    public delegate T DecorateDelegate<T>(T decoratee);
    
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
                from d in s.Value._decorators
                select d;

            return decorators.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose() => AsyncScope<DecoratorScope<T>>.Current?.Dispose();
    }

    public static class DecoratorScope
    {
        public static DecoratorScope<T> For<T>() => DecoratorScope<T>.Begin();
    }

    public static class DecoratorHelper
    {
        public static T Decorate<T>(this T decoratee) where T : class => DecoratorScope<T>.Current?.Decorate(decoratee) ?? decoratee;
    }
}