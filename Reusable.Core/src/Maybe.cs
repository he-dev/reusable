using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reusable
{
    public readonly struct Maybe<T> where T : class
    {
        public Maybe(T value, object? tag = default) => (Value, Tag) = (value, tag);

        public T Value { get; }

        public bool HasValue => Value is {};

        public object? Tag { get; }

        public static implicit operator bool(Maybe<T> maybe) => maybe.HasValue;

        public static implicit operator Maybe<T>(T value) => new Maybe<T>(value);

        public static implicit operator Maybe<T>((T Value, object? Tag) maybe) => new Maybe<T>(maybe.Value, maybe.Tag);
    }

    public class Option<T> : IEnumerable<T>
    {
        private readonly List<T> _data;

        internal Option(List<T> data, object? tag = default) => (_data, Tag) = (data, tag);

        public object? Tag { get; }

        public static Option<T> Empty(object? tag = default) => new Option<T>(new List<T>(), tag);

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_data).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

        public static implicit operator bool(Option<T> option) => option.Any();

        public static implicit operator Option<T>(T value) => value == null ? Empty() : Option.Create(value);

        public static implicit operator Option<T>((T value, object tag) option)
        {
            return
                option.value == null
                    ? Empty(option.tag)
                    : Option.Create(option.value, option.tag);
        }
    }

    public static class Option
    {
        public static Option<T> Create<T>(T value, object? tag = default) => new Option<T>(new List<T> { value }, tag);

        //public static Option<T> Create<T>(T value, object? tag = default) where T : class => new Option<T>(new List<T> { value }, tag);
    }
}