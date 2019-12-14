using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reusable
{
//    public readonly struct Maybe<T> where T : class
//    {
//        public Maybe(T value, object? tag = default) => (Value, Tag) = (value, tag);
//
//        public T Value { get; }
//
//        public bool HasValue => Value is {};
//
//        public object? Tag { get; }
//
//        public static implicit operator bool(Maybe<T> maybe) => maybe.HasValue;
//
//        public static implicit operator Maybe<T>(T value) => new Maybe<T>(value);
//
//        public static implicit operator Maybe<T>((T Value, object? Tag) maybe) => new Maybe<T>(maybe.Value, maybe.Tag);
//    }

    public class Maybe<T> : IEnumerable<T>
    {
        private readonly List<T> _data;

        internal Maybe(List<T> data, object? tag = default) => (_data, Tag) = (data, tag);

        public object? Tag { get; }

        public static Maybe<T> Empty(object? tag = default) => new Maybe<T>(new List<T>(), tag);

        public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> convert)
        {
            return this ? convert(this.Single()) : Maybe<TResult>.Empty();
        }

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_data).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

        public static implicit operator bool(Maybe<T> maybe) => maybe.Any();

        public static implicit operator Maybe<T>(T value) => value == null ? Empty() : Maybe.Create(value);

        // public static implicit operator Maybe<T>((T value, object tag) option)
        // {
        //     return
        //         option.value == null
        //             ? Empty(option.tag)
        //             : Maybe.Create(option.value, option.tag);
        // }
    }

    public static class Maybe
    {
        public static Maybe<T> Create<T>(T value, object? tag = default) => new Maybe<T>(new List<T> { value }, tag);
        
        public static Maybe<T> SingleRef<T>(T? value, object? tag = default) where T: class
        {
            return new Maybe<T>(value is null ? new List<T>() : new List<T> { value }, tag);
        }

        //public static Option<T> Create<T>(T value, object? tag = default) where T : class => new Option<T>(new List<T> { value }, tag);
    }
}