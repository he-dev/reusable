using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Reusable
{
    public class Maybe<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _data;

        internal Maybe(IEnumerable<T> data, object? tag = default) => (_data, Tag) = (data, tag);

        public object? Tag { get; }

        public static Maybe<T> Empty(object? tag = default) => new Maybe<T>(Enumerable.Empty<T>(), tag);

        public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> convert)
        {
            return this ? convert(this.Single()) : Maybe<TResult>.Empty();
        }

        public IEnumerator<T> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

        public static implicit operator bool(Maybe<T> maybe) => maybe.Any();

        public static implicit operator Maybe<T>(T value) => value == null ? Empty() : new Maybe<T>(new[] { value });

        public static implicit operator Maybe<T>((T value, object tag) maybe)
        {
            return
                maybe.value == null
                    ? Empty(maybe.tag)
                    : new Maybe<T>(new[] { maybe.value }, maybe.tag);
        }
    }

    public static class Maybe
    {
        public static Maybe<T> FromObject<T>(T? obj, object? tag = default) where T : class
        {
            return new Maybe<T>(obj is null ? Enumerable.Empty<T>() : new[] { obj }, tag);
        }
    }
}