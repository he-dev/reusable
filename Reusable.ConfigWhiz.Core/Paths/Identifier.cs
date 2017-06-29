using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.ConfigWhiz.Extensions;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz.Paths
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class Identifier : IEquatable<Identifier>, IFormattable
    {
        public Identifier(
            [NotNull, ItemNotNull] IEnumerable<string> context,
            [CanBeNull] string consumer,
            [CanBeNull] string instance,
            [NotNull] string container,
            [CanBeNull] string setting,
            [CanBeNull] string element,
            IdentifierLength length)
        {
            Context = context.ToImmutableList();
            Consumer = consumer.NullIfEmpty();
            Instance = instance.NullIfEmpty();
            Container = container.NullIfEmpty();
            Setting = setting;
            Element = element.NullIfEmpty();
            Length = length;
        }

        private string DebuggerDisplay => ToString();

        [NotNull, ItemNotNull]
        public IImmutableList<string> Context { get; }

        [CanBeNull]
        public string Consumer { get; }

        [CanBeNull]
        public string Instance { get; }

        [NotNull]
        public string Container { get; }

        [CanBeNull]
        public string Setting { get; }

        [CanBeNull]
        public string Element { get; }

        public IdentifierLength Length { get; }

        public static Identifier Create<TContainer>(IdentifierLength length)
        {
            return new Identifier(
                context: ImmutableList<string>.Empty,
                consumer: null,
                instance: null,
                container: typeof(TContainer).GetCustomNameOrDefault(),
                setting: null,
                element: null,
                length: length);
        }

        public static Identifier Create<TConsumer, TContainer>([CanBeNull] string instance, IdentifierLength length)
        {
            return new Identifier(
                context: typeof(TConsumer).Namespace.Split('.').ToImmutableList(),
                consumer: typeof(TConsumer).GetCustomNameOrDefault(),
                instance: instance,
                container: typeof(TContainer).GetCustomNameOrDefault(),
                setting: null,
                element: null,
                length: length);
        }

        public static Identifier From(Identifier identifier, string setting)
        {
            return new Identifier(
                identifier.Context,
                identifier.Consumer,
                identifier.Instance,
                identifier.Container,
                setting,
                null,
                identifier.Length);
        }

        public static Identifier Parse(string value)
        {
            var matches = Regex.Matches(value, @"(?<Delimiter>[^a-z0-9_])?(?<Name>[a-z_][a-z0-9_]*)(?:\[(?<Key>"".+?""|'.+?'|.+?)\])?", RegexOptions.IgnoreCase);

            var items =
                (from m in matches.Cast<Match>()
                 select new
                 {
                     Delimiter = m.Groups["Delimiter"].Value,
                     Name = m.Groups["Name"].Value,
                     Key = m.Groups["Key"].Value.Trim('"', '\'')
                 }).ToList();

            var length = DetermineLength(items.Count);

            var containerNameCount = 2;
            var consumerNameCount = items.Count - containerNameCount;
            var isUnique = consumerNameCount > 0;

            var consumerNamespace = items.Take(consumerNameCount - 1).Select(x => x.Name).ToImmutableList();
            var consumerName = isUnique ? items.Skip(consumerNameCount - 1).Take(1).SingleOrDefault()?.Name : null;
            var instanceName = isUnique ? items.Skip(consumerNameCount - 1).Take(1).SingleOrDefault()?.Key : null;
            var containerName = items.Skip(consumerNameCount).Take(1).SingleOrDefault()?.Name;
            var settingName = items.Skip(consumerNameCount + 1).Take(1).Single().Name;
            var elementName = items.Skip(consumerNameCount + 1).Take(1).SingleOrDefault()?.Key;

            return
                new Identifier(
                    consumerNamespace,
                    consumerName,
                    instanceName,
                    containerName,
                    settingName,
                    elementName,
                    length
                );

            IdentifierLength DetermineLength(int itemCount)
            {
                if (itemCount == (int)IdentifierLength.Short) return IdentifierLength.Short;
                if (itemCount == (int)IdentifierLength.Medium) return IdentifierLength.Medium;
                if (itemCount == (int)IdentifierLength.Long) return IdentifierLength.Long;
                if (itemCount >= (int)IdentifierLength.Unique) return IdentifierLength.Unique;

                throw new ArgumentOutOfRangeException($"Invalid item count {itemCount}. It must be between 1-4 inclusive.");
            }
        }

        #region IEquatable<ContainerPath>

        public bool Equals(Identifier other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            return
                Context.SequenceEqual(other.Context, StringComparer.OrdinalIgnoreCase) &&
                StringComparer.OrdinalIgnoreCase.Equals(Consumer, other.Consumer) &&
                StringComparer.OrdinalIgnoreCase.Equals(Instance, other.Instance) &&
                StringComparer.OrdinalIgnoreCase.Equals(Container, other.Container) &&
                StringComparer.OrdinalIgnoreCase.Equals(Setting, other.Setting) &&
                StringComparer.OrdinalIgnoreCase.Equals(Element, other.Element) &&
                Length == other.Length;
        }

        #endregion

        public override bool Equals(object obj)
        {
            return Equals(obj as Identifier);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return GetHashCodes().Aggregate(0, (current, next) => (current * 397) ^ next);
            }

            IEnumerable<int> GetHashCodes()
            {
                foreach (var name in Context) yield return name.GetHashCode();
                yield return Consumer?.GetHashCode() ?? 0;
                yield return Instance?.GetHashCode() ?? 0;
                yield return Container?.GetHashCode() ?? 0;
                yield return Setting?.GetHashCode() ?? 0;
                yield return Element?.GetHashCode() ?? 0;
                yield return Length.GetHashCode();
            }
        }

        public override string ToString()
        {
            return ToString($".{Length}", IdentifierFormatter.Instance);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format.IsNullOrEmpty() || formatProvider == null) { return ToString(); }

            return
                formatProvider.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter x
                    ? x.Format(format, this, formatProvider)
                    : base.ToString();
        }

        public static bool operator ==(Identifier left, Identifier right)
        {
            if (ReferenceEquals(left, null)) return false;
            if (ReferenceEquals(right, null)) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Identifier left, Identifier right)
        {
            return !(left == right);
        }
    }
}