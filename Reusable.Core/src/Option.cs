using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable
{
    [PublicAPI]
    public abstract class Option
    {
        protected const string Unknown = nameof(Unknown);

        protected static readonly OptionComparer Comparer = new OptionComparer();

        public static readonly IImmutableList<SoftString> ReservedNames =
            ImmutableList<SoftString>
                .Empty
                .Add(nameof(Option<Option>.None))
                .Add(nameof(Option<Option>.All))
                .Add(nameof(Option<Option>.Max));

        // Disallow anyone else to use this class.
        // This way we can guarantee that it is used only by the Option<T>.
        private protected Option() { }

        [NotNull]
        public abstract SoftString Name { get; }

        public abstract int Flag { get; }

        /// <summary>
        /// Returns True if Option is power of two.
        /// </summary>
        public abstract bool IsBit { get; }

        protected class OptionComparer : IComparer<Option>, IComparer
        {
            public int Compare(Option left, Option right)
            {
                if (ReferenceEquals(left, right)) return 0;
                if (ReferenceEquals(left, null)) return 1;
                if (ReferenceEquals(right, null)) return -1;
                return left.Flag - right.Flag;
            }

            public int Compare(object left, object right)
            {
                return Compare(left as Option, right as Option);
            }
        }
    }

    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class Option<T> : Option, IEquatable<Option<T>>, IComparable<Option<T>>, IComparable, IFormattable where T : Option
    {
        // ReSharper disable once StaticMemberInGenericType - this is correct
        private static readonly ConstructorInfo Constructor;

        private static IImmutableSet<T> _options;

        static Option()
        {
            Constructor =
                typeof(T).GetConstructor(new[] { typeof(SoftString), typeof(int) })
                ?? throw DynamicException.Create
                (
                    "ConstructorNotFound",
                    $"{typeof(T).ToPrettyString()} must provide a constructor with the following signature: " +
                    $"ctor({typeof(SoftString).ToPrettyString()}, {typeof(int).ToPrettyString()})"
                );

            // Always initialize "None".
            _options = ImmutableSortedSet<T>.Empty.Add(Create(nameof(None), 0));
        }

        protected Option(SoftString name, int flag)
        {
            if (GetType() != typeof(T)) throw DynamicException.Create("OptionTypeMismatch", "Option must be a type of itself.");

            Name = name;
            Flag = flag;
        }

        #region Default options

        [NotNull]
        public static T None => _options.First();

        [NotNull]
        public static T Max => _options.Last();

        [NotNull]
        public static IEnumerable<T> All => _options;

        #endregion

        [NotNull, ItemNotNull]
        public static IEnumerable<T> Bits => _options.Where(o => o.IsBit);

        #region Option

        public override SoftString Name { [DebuggerStepThrough] get; }

        [AutoEqualityProperty]
        public override int Flag { [DebuggerStepThrough] get; }

        public override bool IsBit => (Flag & (Flag - 1)) == 0;

        #endregion

        [NotNull, ItemNotNull]
        public IEnumerable<T> Flags => Bits.Where(f => (Flag & f.Flag) > 0);

        #region Factories

        [NotNull]
        public static T Create(SoftString name, T option = default)
        {
            if (name.In(_options.Select(o => o.Name).Concat(ReservedNames)))
            {
                throw DynamicException.Create("DuplicateOption", $"The option '{name}' is defined more the once.");
            }

            var bitCount = _options.Count(o => o.IsBit);
            var newOption = Create(name, bitCount == 1 ? 1 : (bitCount - 1) << 1);
            _options = _options.Add(newOption);

            return newOption;
        }

        [NotNull]
        public static T CreateWithCallerName(T option = default, [CallerMemberName] string name = default)
        {
            return Create(name, option);
        }

        private static T Create(SoftString name, IEnumerable<int> flags)
        {
            var flag = flags.Aggregate(0, (current, next) => current | next);
            return (T)Constructor.Invoke(new object[] { name, flag });
        }

        public static T Create(SoftString name, params int[] flags)
        {
            return Create(name, flags.AsEnumerable());
        }

        [NotNull]
        public static T FromName([NotNull] string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            return
                _options.FirstOrDefault(o => o.Name == value)
                ?? throw DynamicException.Create("OptionOutOfRange", $"There is no such option as '{value}'.");
        }

        [NotNull]
        public static T FromValue(int value)
        {
            if (value > Max.Flag)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(value), $"Value {value} is greater than the highest option.");
            }

            // Is this a known value?
            if (TryGetKnownOption(value, out var knownOption))
            {
                return knownOption;
            }

            var newFlags = Bits.Where(o => (o.Flag & value) == o.Flag).Select(o => o.Flag);
            return Create(Unknown, newFlags);
        }

        private static bool TryGetKnownOption(int flag, out T option)
        {
            if (_options.SingleOrDefault(o => o.Flag == flag) is var knownOption && !(knownOption is null))
            {
                option = knownOption;
                return true;
            }
            else
            {
                option = default;
                return false;
            }
        }

        #endregion

        public T Set(Option<T> option) => this | option;

        public T Reset(Option<T> option) => this ^ option;

        [DebuggerStepThrough]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (SoftString.Comparer.Equals(format, "names"))
            {
                return Flags.Select(o => $"{o.Name.ToString()}").Join(", ");
            }

            if (SoftString.Comparer.Equals(format, "flags"))
            {
                return Flags.Select(o => $"{o.Flag}").Join(", ");
            }

            if (SoftString.Comparer.Equals(format, "names+flags"))
            {
                return Flags.Select(o => $"{o.Name.ToString()}[{o.Flag}]").Join(", ");
            }

            return ToString();
        }

        public override string ToString() => $"{this:names}";

        public bool Contains(T option) => Contains(option.Flag);

        public bool Contains(int flags) => (Flag & flags) == flags;

        public int CompareTo(Option<T> other) => Comparer.Compare(this, other);

        public int CompareTo(object other) => Comparer.Compare(this, other);

        #region IEquatable

        public bool Equals(Option<T> other) => AutoEquality<Option<T>>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as Option<T>);

        public override int GetHashCode() => AutoEquality<Option<T>>.Comparer.GetHashCode(this);

        #endregion

        #region Operators

        public static implicit operator string(Option<T> option) => option?.ToString() ?? throw new ArgumentNullException(nameof(option));

        public static implicit operator int(Option<T> option) => option?.Flag ?? throw new ArgumentNullException(nameof(option));

        public static bool operator ==(Option<T> left, Option<T> right) => Comparer.Compare(left, right) == 0;

        public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);

        public static bool operator <(Option<T> left, Option<T> right) => Comparer.Compare(left, right) < 0;

        public static bool operator <=(Option<T> left, Option<T> right) => Comparer.Compare(left, right) <= 0;

        public static bool operator >(Option<T> left, Option<T> right) => Comparer.Compare(left, right) > 0;

        public static bool operator >=(Option<T> left, Option<T> right) => Comparer.Compare(left, right) >= 0;

        [NotNull]
        public static T operator |(Option<T> left, Option<T> right) => GetKnownOrCreate(left.Flag | right.Flag);

        [NotNull]
        public static T operator ^(Option<T> left, Option<T> right) => GetKnownOrCreate(left.Flag ^ right.Flag);

        private static T GetKnownOrCreate(int flag)
        {
            return
                TryGetKnownOption(flag, out var knownOption)
                    ? knownOption
                    : Create(Unknown, flag);
        }

        #endregion
    }
}