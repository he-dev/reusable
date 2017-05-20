using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Annotations;

namespace Reusable.Colin.Collections
{
    /// <summary>
    /// Name set used for command and argument names.
    /// </summary>
    public class ImmutableNameSet : IImmutableSet<string>
    {
        private readonly IImmutableSet<string> _names;

        private ImmutableNameSet(params string[] names) => _names = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, names);

        public static readonly ImmutableNameSet Empty = Create(string.Empty);

        [NotNull]
        public static ImmutableNameSet Create(IEnumerable<string> values) => Create(values.ToArray());

        [NotNull]
        public static ImmutableNameSet Create([NotNull] params string[] names)
        {
            if (names == null) throw new ArgumentNullException(nameof(names));
            if (names.Length == 0) throw new ArgumentException("You need to specify at least one name.", nameof(names));

            return new ImmutableNameSet(names.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray());
        }

        [NotNull]
        public static ImmutableNameSet From([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!typeof(ICommand).IsAssignableFrom(type)) { throw new ArgumentException(paramName: nameof(type), message: $"'{nameof(type)}' needs to be derived from '{nameof(ICommand)}'"); }

            var names = GetCommandNames();
            AddCommandNamespace();

            return Create(names);

            List<string> GetCommandNames()
            {
                return
                    type.GetCustomAttribute<CommandNameAttribute>()?.ToList() ??
                    new List<string> { Regex.Replace(type.Name, "Command$", string.Empty, RegexOptions.IgnoreCase) };
            }

            void AddCommandNamespace()
            {
                var commandNamespace = type.GetCustomAttribute<CommandNamespaceAttribute>();
                if (commandNamespace == null) return;
                if (commandNamespace.Required) { names.Clear(); }
                names.AddRange(names.Select(n => $"{commandNamespace}.{n}"));
            }
        }

        [NotNull]
        public static ImmutableNameSet From([NotNull] ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            return From(command.GetType());
        }

        public static ImmutableNameSet From([NotNull] PropertyInfo parameterProperty)
        {
            var parameter = parameterProperty.GetCustomAttribute<ParameterAttribute>();
            if (parameter.Names.Any())
            {
                return Create(parameter.Names);
            }

            // Get the default name first.
            var names = new List<string> { parameterProperty.Name };

            if (parameter.CanCreateShortName)
            {
                var initials =
                    Regex
                        .Matches(parameterProperty.Name, "[A-Z]")
                        .Cast<Match>()
                        .Select(m => m.Groups[0].Value)
                        .ToList();
                if (initials.Any())
                {
                    names.Add(string.Join(string.Empty, initials));
                }
            }

            return Create(names);
        }

        #region IImmutableSet<string>

        public int Count => _names.Count;
        public IImmutableSet<string> Clear() => _names.Clear();
        public bool Contains(string value) => _names.Contains(value);
        public IImmutableSet<string> Add(string value) => _names.Add(value);
        public IImmutableSet<string> Remove(string value) => _names.Remove(value);
        public bool TryGetValue(string equalValue, out string actualValue) => _names.TryGetValue(equalValue, out actualValue);
        public IImmutableSet<string> Intersect(IEnumerable<string> other) => _names.Intersect(other);
        public IImmutableSet<string> Except(IEnumerable<string> other) => _names.Except(other);
        public IImmutableSet<string> SymmetricExcept(IEnumerable<string> other) => _names.SymmetricExcept(other);
        public IImmutableSet<string> Union(IEnumerable<string> other) => _names.Union(other);
        public bool SetEquals(IEnumerable<string> other) => _names.SetEquals(other);
        public bool IsProperSubsetOf(IEnumerable<string> other) => _names.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<string> other) => _names.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<string> other) => _names.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<string> other) => _names.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<string> other) => _names.Overlaps(other);
        public IEnumerator<string> GetEnumerator() => _names.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _names.GetEnumerator();

        #endregion

        public override bool Equals(object obj) => Comparer.Equals(this, obj as ImmutableNameSet);

        public override int GetHashCode() => Comparer.GetHashCode(this);

        public static bool operator ==(ImmutableNameSet left, ImmutableNameSet right) => Comparer.Equals(left, right);

        public static bool operator !=(ImmutableNameSet left, ImmutableNameSet right) => !(left == right);

        public static IEqualityComparer<ImmutableNameSet> Comparer { get; } = new ImmutableNameSetEqualityComparer();

        private sealed class ImmutableNameSetEqualityComparer : IEqualityComparer<ImmutableNameSet>
        {
            public bool Equals(ImmutableNameSet x, ImmutableNameSet y)
            {
                return
                    !ReferenceEquals(x, null) &&
                    !ReferenceEquals(y, null) &&
                    x.Overlaps(y);
            }

            // The hash code are always different thus this comparer. We need to check if the sets overlap so we cannot relay on the hash code.
            public int GetHashCode(ImmutableNameSet obj) => 0;
        }
    }
}
