using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Data
{
    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class Option<T> : IEnumerable<SoftString>, IEquatable<Option<T>>
    {
        public const string Unknown = nameof(Unknown);

        public static readonly IImmutableList<SoftString> ReservedNames =
            ImmutableList<SoftString>
                .Empty
                .Add(nameof(None));

        // Values are what matters for equality.
        private static readonly IEqualityComparer<Option<T>> Comparer = EqualityComparerFactory<Option<T>>.Create
        (
            equals: (left, right) => left._values.SetEquals(right._values),
            getHashCode: (obj) => obj._values.GetHashCode()
        );

        private static readonly ConcurrentDictionary<SoftString, Option<T>> Options;

        static Option()
        {
            Options = new ConcurrentDictionary<SoftString, Option<T>>
            {
                [nameof(None)] = new Option<T>(nameof(None), ImmutableHashSet<SoftString>.Empty.Add(nameof(None)))
            };
        }

        private IImmutableSet<SoftString> _values;

        private Option(SoftString name, IEnumerable<SoftString> values)
        {
            Name = name;
            _values = values.ToImmutableSortedSet();
        }

        private string DebuggerDisplay => ToString(); // this.ToDebuggerDisplayString(b => b.DisplayScalar(x => x.))

        [NotNull]
        public static Option<T> None => Options[nameof(None)];

        /// <summary>
        /// Gets all known options ever created for this type.
        /// </summary>
        [NotNull]
        public static IEnumerable<Option<T>> Known => Options.Values;

        /// <summary>
        /// Gets options that have only a single value.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<Option<T>> Bits => Options.Values.Where(o => o.IsFlag);

        public SoftString Name
        {
            [DebuggerStepThrough]
            get;
        }

        /// <summary>
        /// Gets value indicating whether this option has only a single value.
        /// </summary>
        public bool IsFlag => _values.Count == 1;

        #region Factories

        [NotNull]
        public static Option<T> Create(SoftString name, IEnumerable<SoftString> values)
        {
            if (name.In(ReservedNames))
            {
                throw DynamicException.Create("ReservedOption", $"The option '{name}' is reserved and must not be created.");
            }

            if (Options.ContainsKey(name))
            {
                throw DynamicException.Create("DuplicateOption", $"The option '{name}' is already defined.");
            }

            return Options[name] = new Option<T>(name, values);
        }

        public static Option<T> Create(SoftString name, params SoftString[] values)
        {
            return Create(name, values.AsEnumerable());
        }

        [NotNull]
        public static Option<T> CreateWithCallerName([CanBeNull] string value = default, [CallerMemberName] string name = default)
        {
            return Create(name, value ?? name);
        }

        public static bool TryParse(string option, out Option<T> knownOption)
        {
            if (option == null)
            {
                knownOption = default;
                return false;
            }

            var values =
                Regex
                    .Matches(option, @"[a-z0-9_]+", RegexOptions.IgnoreCase)
                    .Cast<Match>()
                    .Select(m => m.Value.ToSoftString())
                    .ToImmutableHashSet(SoftString.Comparer);

            knownOption = Options.Values.SingleOrDefault(o => values.SetEquals(o));

            return !(knownOption is null);
        }

        [NotNull]
        public static Option<T> Parse([NotNull] string option)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));

            return
                TryParse(option, out var knownOption)
                    ? knownOption
                    : throw DynamicException.Create("OptionOutOfRange", $"There is no such option as '{option}'.");
        }

        #endregion

        public Option<T> SetFlag(Option<T> option) => this | option;

        public Option<T> RemoveFlag(Option<T> option) => this ^ option;


        [DebuggerStepThrough]
        public override string ToString()
        {
            return
                _values
                    .Select(x => $"{x.ToString()}")
                    .Join(", ");
        }

        public bool Contains(Option<T> option) => _values.Overlaps(option);

        #region IEquatable

        public bool Equals(Option<T> other) => Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as Option<T>);

        public override int GetHashCode() => Comparer.GetHashCode(this);

        #endregion

        public IEnumerator<SoftString> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_values).GetEnumerator();

        #region Operators

        public static implicit operator string(Option<T> option) => option?.ToString() ?? throw new ArgumentNullException(nameof(option));

        public static bool operator ==(Option<T> left, Option<T> right) => Comparer.Equals(left, right);

        public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);

        [NotNull]
        public static Option<T> operator |(Option<T> left, Option<T> right)
        {
            var values = left._values.Concat(right._values).ToImmutableHashSet();
            return GetKnownOrCreate(values);
        }

        [NotNull]
        public static Option<T> operator ^(Option<T> left, Option<T> right)
        {
            var values = left._values.Except(right._values).ToImmutableHashSet();
            return
                values.Any()
                    ? GetKnownOrCreate(values)
                    : None;
        }

        private static Option<T> GetKnownOrCreate(IImmutableSet<SoftString> values)
        {
            return Options.Values.SingleOrDefault(values.SetEquals) ?? new Option<T>(Unknown, values);
        }

        #endregion
    }
}