using System;
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

namespace Reusable.Data
{
    [PublicAPI]
    public abstract class Option
    {
        protected const string Unknown = nameof(Unknown);

        public static readonly IImmutableList<SoftString> ReservedNames =
            ImmutableList<SoftString>
                .Empty
                .Add(nameof(Option<Option>.None))
                .Add(nameof(Option<Option>.Known));

        // Disallow anyone else to use this class.
        // This way we can guarantee that it is used only by the Option<T>.
        private protected Option() { }

        [NotNull]
        public abstract SoftString Name { get; }

        public abstract IImmutableSet<SoftString> Values { get; }

        public abstract bool IsFlag { get; }
    }

    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class Option<T> : Option, IEquatable<Option<T>>, IFormattable where T : Option
    {
        // Values are what matters for equality.
        private static readonly IEqualityComparer<Option<T>> Comparer = EqualityComparerFactory<Option<T>>.Create
        (
            @equals: (left, right) => left.Values.SetEquals(right.Values),
            getHashCode: (obj) => obj.Values.GetHashCode()
        );

        // ReSharper disable once StaticMemberInGenericType - this is correct
        private static readonly ConstructorInfo Constructor;

        static Option()
        {
            Constructor =
                typeof(T).GetConstructor(new[] { typeof(SoftString), typeof(IImmutableSet<SoftString>) })
                ?? throw DynamicException.Create
                (
                    "ConstructorNotFound",
                    $"{typeof(T).ToPrettyString()} must provide a constructor with the following signature: " +
                    $"ctor({typeof(SoftString).ToPrettyString()}, {typeof(int).ToPrettyString()})"
                );

            // Always initialize "None".
            var none = New(nameof(None), ImmutableHashSet<SoftString>.Empty.Add(nameof(None)));
            Known = ImmutableHashSet<T>.Empty.Add(none);
        }

        protected Option(SoftString name, IImmutableSet<SoftString> values)
        {
            Name = name;
            Values = values;
        }

        [NotNull]
        public static T None => Known.Single(o => o.Name == nameof(None));

        /// <summary>
        /// Gets all known options ever created for this type.
        /// </summary>
        [NotNull]
        public static IImmutableSet<T> Known { get; private set; }

        /// <summary>
        /// Gets options that have only a single value.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<T> Bits => Known.Where(o => o.IsFlag);

        #region Option

        public override SoftString Name { [DebuggerStepThrough] get; }

        public override IImmutableSet<SoftString> Values { get; }

        /// <summary>
        /// Gets value indicating whether this option has only a single value.
        /// </summary>
        public override bool IsFlag => Values.Count == 1;

        #endregion

        #region Factories

        public static T Create(SoftString name, params SoftString[] values)
        {
            return Create(name, values.ToImmutableHashSet());
        }

        [NotNull]
        public static T Create(SoftString name, IImmutableSet<SoftString> values)
        {
            if (name.In(ReservedNames))
            {
                throw DynamicException.Create("ReservedOption", $"The option '{name}' is reserved and must not be created by the user.");
            }

            if (name.In(Known.Select(o => o.Name)))
            {
                throw DynamicException.Create("DuplicateOption", $"The option '{name}' is already defined.");
            }

            var newOption = New(name, values);

            if (name == Unknown)
            {
                return newOption;
            }

            Known = Known.Add(newOption);
            return newOption;
        }

        private static T New(SoftString name, IImmutableSet<SoftString> values)
        {
            return (T)Constructor.Invoke(new object[]
            {
                name,
                values.Any()
                    ? values
                    : ImmutableHashSet<SoftString>.Empty.Add(name)
            });
        }

        [NotNull]
        public static T CreateWithCallerName([CanBeNull] string value = default, [CallerMemberName] string name = default)
        {
            return Create(name, value ?? name);
        }

        [NotNull]
        public static T FromName([NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return
                Known.FirstOrDefault(o => o.Name == name)
                ?? throw DynamicException.Create("OptionOutOfRange", $"There is no such option as '{name}'.");
        }

        private static bool TryGetKnownOption(IEnumerable<SoftString> values, out T option)
        {
            if (Known.SingleOrDefault(o => o.Values.SetEquals(values)) is var knownOption && !(knownOption is null))
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
            if (format.In(new[] { "asc", null }, SoftString.Comparer))
            {
                return Values.OrderBy(x => x).Select(x => $"{x.ToString()}").Join(", ");
            }

            if (format.In(new[] { "desc" }, SoftString.Comparer))
            {
                return Values.OrderByDescending(x => x).Select(x => $"{x.ToString()}").Join(", ");
            }

            return ToString();
        }

        public override string ToString() => $"{this:asc}";

        public bool Contains(T option) => Values.Overlaps(option.Values);

        #region IEquatable

        public bool Equals(Option<T> other) => Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as Option<T>);

        public override int GetHashCode() => Comparer.GetHashCode(this);

        #endregion

        #region Operators

        public static implicit operator string(Option<T> option) => option?.ToString() ?? throw new ArgumentNullException(nameof(option));

        public static bool operator ==(Option<T> left, Option<T> right) => Comparer.Equals(left, right);

        public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);

        [NotNull]
        public static T operator |(Option<T> left, Option<T> right)
        {
            var values = left.Values.Concat(right.Values).ToImmutableHashSet();
            return GetKnownOrCreate(values);
        }

        [NotNull]
        public static T operator ^(Option<T> left, Option<T> right)
        {
            var values = left.Values.Except(right.Values).ToImmutableHashSet();
            return GetKnownOrCreate(values);
        }

        private static T GetKnownOrCreate(IImmutableSet<SoftString> values)
        {
            return
                TryGetKnownOption(values, out var knownOption)
                    ? knownOption
                    : Create(Unknown, values);
        }

        #endregion
    }
}