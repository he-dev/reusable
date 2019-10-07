using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Reusable.Experimental
{
    public class Name : ValueObjectCollection<Name>
    {
        public Name(string first, string middle, string last)
        {
            First = first;
            Middle = middle;
            Last = last;
        }

        public Name(Word first, WordOrEmpty middle, Word last)
        {
            First = first;
            Middle = middle;
            Last = last;
        }

        public Word First { get; }
        public WordOrEmpty Middle { get; }
        public Word Last { get; }

        public override IEnumerator<object> GetEnumerator()
        {
            yield return First;
            yield return Middle;
            yield return Last;
        }
    }

    public abstract class ValueObject<T> : IEquatable<ValueObject<T>>
    {
        protected ValueObject(T value, IEqualityComparer<T> comparer)
        {
            Value = value;
            Comparer = comparer ?? EqualityComparer<T>.Default;
        }

        protected T Value { get; }

        public IEqualityComparer<T> Comparer { get; }

        public override int GetHashCode() => Comparer.GetHashCode(Value);

        public override bool Equals(object obj) => Equals(obj as ValueObject<T>);

        public virtual bool Equals(ValueObject<T> other)
        {
            return !ReferenceEquals(other, null) && Comparer.Equals(Value, other.Value);
        }

        public override string ToString() => Value?.ToString() ?? string.Empty;

        public static bool operator ==(ValueObject<T> left, ValueObject<T> right) => Equals(left, right);

        public static bool operator !=(ValueObject<T> left, ValueObject<T> right) => !Equals(left, right);
    }

    public abstract class ValueObjectCollection<T> : IEquatable<ValueObjectCollection<T>>, IEnumerable<object>
    {
        public bool Equals(ValueObjectCollection<T> other)
        {
            return !ReferenceEquals(other, null) && this.Zip(other, (x, y) => x.Equals(y)).All(x => x);
        }

        public override bool Equals(object obj) => Equals(obj as ValueObjectCollection<T>);

        public override int GetHashCode() => this.CalcHashCode();

        public abstract IEnumerator<object> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    public abstract class StringObject : ValueObject<string>
    {
        public static implicit operator string(StringObject s) => s?.Value;

        protected StringObject(string text, IEqualityComparer<string> comparer, IValueFilter<string> filter)
            : base(filter.Apply(text), comparer) { }

        protected static class Alter
        {
            public static string Upper(string text) => text?.ToUpper();
            public static string Lower(string text) => text?.ToLower();
            public static string Trim(string text) => text?.Trim();
        }

        protected static class Is
        {
            public static Func<string, string> Like(string pattern, RegexOptions regexOptions = RegexOptions.Compiled)
            {
                return text => text is null ? text : Regex.IsMatch(text, pattern, regexOptions) ? text : throw new TextException();
            }
        }

        protected static class Use
        {
            public static string EmptyIfNull(string text) => text ?? string.Empty;
            public static string NullIfEmpty(string text) => string.IsNullOrWhiteSpace(text) ? null : text;

            public static string SpaceIfNewLine(string text) => text
                ?.Replace("\n\r", " ")
                ?.Replace("\r\n", " ")
                ?.Replace("\r", " ")
                ?.Replace("\n", " ");
        }

        protected static class Not
        {
            public static string Space(string text) => text.Contains(' ') ? throw new TextException() : text;
            public static string Null(string text) => text ?? throw new TextException();
            public static string NullOrWhitespace(string text) => string.IsNullOrWhiteSpace(text) ? throw new TextException() : text;
            public static string NullOrEmpty(string text) => string.IsNullOrEmpty(text) ? throw new TextException() : text;
            public static string Multiline(string text) => text.Contains('\n') || text.Contains('\r') ? throw new TextException() : text;
        }
    }

    public interface IValueFilter<T>
    {
        T Apply(T value);
    }

    public abstract class ValueFilter<T> : IValueFilter<T>
    {
        private readonly bool _canHandleNull;

        protected ValueFilter(bool canHandleNull)
        {
            _canHandleNull = canHandleNull;
        }

        public T Apply(T value)
        {
            return
                _canHandleNull || value != null
                    ? ApplyCore(value)
                    : default;
        }

        protected abstract T ApplyCore(T value);
    }

    public class IsLike : ValueFilter<string>
    {
        private readonly string _pattern;
        private readonly RegexOptions _options;

        public IsLike(string pattern, RegexOptions options) : base(false)
        {
            _pattern = pattern;
            _options = options;
        }

        protected override string ApplyCore(string value)
        {
            return Regex.IsMatch(value, _pattern, _options) ? value : throw new TextException();
        }

        public static IValueFilter<string> Create(string pattern, RegexOptions options = RegexOptions.Compiled)
        {
            return new IsLike(pattern, options);
        }
    }

    public static class CompositeValueFilterExtensions
    {
        public static CompositeValueFilter<string> IsLike(this CompositeValueFilter<string> filter, string pattern, RegexOptions options = RegexOptions.Compiled)
        {
            return filter.Add(new IsLike(pattern, options));
        }

        public static CompositeValueFilter<string> NotNullOrWhitespace(this CompositeValueFilter<string> filter)
        {
            return filter.Add(new NotNullOrWhitespace());
        }
    }


    public class NotNullOrWhitespace : IValueFilter<string>
    {
        public static IValueFilter<string> Create() => new NotNullOrWhitespace();

        public string Apply(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? throw new TextException() : value;
        }
    }

    public class CompositeValueFilter<T> : IValueFilter<T>
    {
        private readonly IImmutableList<IValueFilter<T>> _filters;

        private CompositeValueFilter(IImmutableList<IValueFilter<T>> filters)
        {
            _filters = filters;
        }

        public static CompositeValueFilter<T> Empty => new CompositeValueFilter<T>(ImmutableList<IValueFilter<T>>.Empty);

        public T Apply(T value)
        {
            return _filters.Aggregate(value, (current, f) => f.Apply(current));
        }

        public CompositeValueFilter<T> Add<TFilter>() where TFilter : IValueFilter<T>, new()
        {
            return Add(Add(new TFilter()));
        }

        public CompositeValueFilter<T> Add(IValueFilter<T> filter)
        {
            return new CompositeValueFilter<T>(_filters.Add(filter));
        }
    }

    public class Word : StringObject
    {
        public Word(string text, IEqualityComparer<string> comparer)
            : base(text, comparer, CompositeValueFilter<string>.Empty.NotNullOrWhitespace().IsLike(@"\w")) { }

        public Word(string text)
            : this(text, null) { }

        public static implicit operator Word(string text) => new Word(text);
    }

    public class WordOrEmpty : StringObject
    {
        public WordOrEmpty(string text)
            : base(text, null, Use.EmptyIfNull, Alter.Trim, Is.Like(@"\w")) { }

        public static implicit operator WordOrEmpty(string text) => new WordOrEmpty(text);
    }

    public class TextException : Exception
    {
        public TextException([CallerMemberName] string rule = null)
            : base($"Must be {rule}.") { }
    }
}