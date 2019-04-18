using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Text;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.SmartConfig.Reflection;

namespace Reusable.SmartConfig
{
    using Token = SettingNameToken;

    /// <summary>
    /// Represents a setting name with the format: [Prefix:][Sco.pe+][Type.]Member[,Handle]
    /// </summary>
    [PublicAPI]
    public class SettingIdentifier
    {
        private readonly IDictionary<Token, ReadOnlyMemory<char>> _tokens;

        //public static readonly string Format = "[Prefix:][Name.space+][Type.]Member[,Instance]";

        public SettingIdentifier
        (
            [CanBeNull] string prefix,
            [CanBeNull] string scope,
            [CanBeNull] string type,
            [NotNull] string member,
            [CanBeNull] string handle
        )
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            _tokens = new Dictionary<Token, ReadOnlyMemory<char>>
            {
                [Token.Prefix] = prefix is null ? ReadOnlyMemory<char>.Empty : new ReadOnlyMemory<char>(prefix.ToCharArray()),
                [Token.Scope] = scope is null ? ReadOnlyMemory<char>.Empty : new ReadOnlyMemory<char>(scope.ToCharArray()),
                [Token.Type] = type is null ? ReadOnlyMemory<char>.Empty : new ReadOnlyMemory<char>(type.ToCharArray()),
                [Token.Member] = new ReadOnlyMemory<char>(member.ToCharArray()),
                [Token.Handle] = handle is null ? ReadOnlyMemory<char>.Empty : new ReadOnlyMemory<char>(handle.ToCharArray()),
            };
        }

        public SettingIdentifier([NotNull] IDictionary<Token, ReadOnlyMemory<char>> tokens)
        {
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        }        

        public ReadOnlyMemory<char> this[Token token] => _tokens.TryGetValue(token, out var t) ? t : default;

        [CanBeNull]
        [AutoEqualityProperty]
        public string Prefix => this[Token.Prefix].ToString();

        [CanBeNull]
        [AutoEqualityProperty]
        public string Scope => this[Token.Scope].ToString();

        [CanBeNull]
        [AutoEqualityProperty]
        public string Type => this[Token.Type].ToString();

        [NotNull]
        [AutoEqualityProperty]
        public string Member => this[Token.Member].ToString();

        [CanBeNull]
        [AutoEqualityProperty]
        public string Handle => this[Token.Handle].ToString();

        [NotNull]
        public static SettingIdentifier Parse([NotNull] string text) => new SettingIdentifier(SettingNameParser.Tokenize(text));        

        public override string ToString()
        {
            return
                new StringBuilder()
                    .Append(this[Token.Prefix].IsEmpty ? default : $"{Prefix}{SettingNameParser.Separator.Prefix}")
                    .Append(this[Token.Scope].IsEmpty ? default : $"{Scope}{SettingNameParser.Separator.Scope}")
                    .Append(this[Token.Type].IsEmpty ? default : $"{Type}{SettingNameParser.Separator.Type}")
                    .Append(this[Token.Member])
                    .Append(this[Token.Handle].IsEmpty ? default : $"{SettingNameParser.Separator.Member}{Handle}")
                    .ToString();
        }

        public static implicit operator string(SettingIdentifier settingIdentifier) => settingIdentifier?.ToString();

        public static implicit operator SoftString(SettingIdentifier settingIdentifier) => settingIdentifier?.ToString();

        public static bool operator ==(SettingIdentifier x, SettingIdentifier y) => AutoEquality<SettingIdentifier>.Comparer.Equals(x, y);

        public static bool operator !=(SettingIdentifier x, SettingIdentifier y) => !(x == y);

        #region IEquatable<SettingName>

        public bool Equals(SettingIdentifier other) => AutoEquality<SettingIdentifier>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is SettingIdentifier settingName && Equals(settingName);

        public override int GetHashCode() => AutoEquality<SettingIdentifier>.Comparer.GetHashCode(this);

        #endregion
    }    
}