using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.SmartConfig.Extensions;

namespace Reusable.SmartConfig.Data
{
   

    public interface IIdentifier : IEnumerable<IToken>, IEquatable<IIdentifier>, IFormattable { }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class Identifier : IIdentifier
    {
        // language=regexp
        public const string ContainerSuffix = "Config(uration)?|Setting(s)?";

        private readonly IEnumerable<IToken> _tokens;

        public Identifier(IEnumerable<IToken> tokens)
        {
            // Collapse same consecutive names.
            _tokens = tokens.GroupBy(x => x, (id, items) => items.First()).ToList();
        }

        public Identifier(params IToken[] tokens) : this((IEnumerable<IToken>)tokens) { }

        private string DebuggerDisplay => ToString();

        //public bool IsEmpty => !this.Any();

        public static IIdentifier Create(params IToken[] tokens) => new Identifier(tokens);

        public static IIdentifier Create<TContainer>()
        {
            return new Identifier(
                new Token(typeof(TContainer).GetCustomNameOrDefault().TrimEnd(ContainerSuffix, true), TokenType.Literal));
        }

        public static IIdentifier Create<TConsumer, TContainer>()
        {
            return new Identifier(
                new Token(typeof(TConsumer).GetCustomNameOrDefault(), TokenType.Literal), 
                new Token(typeof(TContainer).GetCustomNameOrDefault().TrimEnd(ContainerSuffix, true), TokenType.Literal));
        }

        public static IIdentifier Create<TConsumer, TContainer>(TConsumer consumer, Func<TConsumer, string> getInstanceName)
        {
            return new Identifier(
                new Token(typeof(TConsumer).GetCustomNameOrDefault(), TokenType.Literal), 
                new Token(getInstanceName(consumer), TokenType.Element), 
                new Token(typeof(TContainer).GetCustomNameOrDefault().TrimEnd(ContainerSuffix, true), TokenType.Literal));
        }

        public static IIdentifier From(IIdentifier identifier, string setting)
        {
            return new Identifier(identifier.Concat(new IToken[] { new Token(setting, TokenType.Literal) }).ToArray());
        }

        public static Identifier Parse(string value)
        {
            return new Identifier(Tokenize(value).ToList());
        }

        private static IEnumerable<IToken> Tokenize(string value)
        {
            //var matches = Regex.Matches(value, @"(?<Delimiter>[^a-z0-9_])?(?<Name>[a-z_][a-z0-9_]*)(?:\[(?<Key>"".+?""|'.+?'|.+?)\])?", RegexOptions.IgnoreCase);
            // https://regex101.com/r/YZOu6f/3
            var matches = Regex.Matches(value, @"((?<Literal>[a-z_][a-z0-9_]*)?(?:\[(?<Element>[a-z0-9_]+)\])?)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture).Cast<Match>();

            foreach (var match in matches)
            {
                if (match.Groups["Literal"].Success) yield return new Token(match.Groups["Literal"].Value, TokenType.Literal);
                if (match.Groups["Element"].Success) yield return new Token(match.Groups["Element"].Value, TokenType.Element);
            }
        }

        public override string ToString()
        {
            return ToString(".", CultureInfo.InvariantCulture);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return _tokens.Aggregate(0, (current, next) => (current * 397) ^ next.Value.GetHashCode());
            }
        }

        public override bool Equals(object obj)
        {
            return obj is IIdentifier identifier && Equals(identifier);
        }

        #region IEquatable<IIdentifier>

        public bool Equals(IIdentifier other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.SequenceEqual(other);
        }

        #endregion

        #region IFormattable

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var delimiter = format;

            var tokenStrings = _tokens.Select((t, i) =>
            {
                switch (t.Type)
                {
                    case TokenType.Literal: return $"{(i > 0 ? delimiter : string.Empty)}{t.Value}";
                    case TokenType.Element: return $"[{t.Value}]";
                    default: throw new ArgumentOutOfRangeException($"Invalid token type: {t.Type}");
                }
            });

            return string.Join(string.Empty, tokenStrings);

            //if (format.IsNullOrEmpty() || formatProvider == null) { return ToString(); }

            //return
            //    formatProvider.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter x
            //        ? x.Format(format, this, formatProvider)
            //        : base.ToString();
        }

        #endregion

        #region IEnumerable<IToken>

        public IEnumerator<IToken> GetEnumerator() => _tokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        //public static bool operator ==(Identifier left, Identifier right)
        //{
        //    if (ReferenceEquals(left, null)) return false;
        //    if (ReferenceEquals(right, null)) return false;
        //    return left.Equals(right);
        //}

        //public static bool operator !=(Identifier left, Identifier right)
        //{
        //    return !(left == right);
        //}
    }
}