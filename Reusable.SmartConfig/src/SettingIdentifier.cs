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

        [Obsolete]
        public SettingIdentifier(UriString uri)
        {
            if (uri.IsRelative) throw new ArgumentException();
            if (uri.Scheme != "setting") throw new ArgumentException();

            var names = uri.Path.Decoded.ToString().Split('.');

            _tokens = new Dictionary<Token, ReadOnlyMemory<char>>
            {
                [Token.Prefix] = uri.Query.TryGetValue("prefix", out var prefix) ? new ReadOnlyMemory<char>(prefix.ToString().ToCharArray()) : ReadOnlyMemory<char>.Empty,
                [Token.Scope] = names.Length == 3 ? new ReadOnlyMemory<char>(names[names.Length - 3].ToCharArray()) : ReadOnlyMemory<char>.Empty,
                [Token.Type] = names.Length >= 2 ? new ReadOnlyMemory<char>(names[names.Length - 2].ToCharArray()) : ReadOnlyMemory<char>.Empty,
                [Token.Member] = names.Length >= 1 ? new ReadOnlyMemory<char>(names[names.Length - 1].ToCharArray()) : ReadOnlyMemory<char>.Empty,
                [Token.Handle] = uri.Query.TryGetValue("instance", out var instance) ? new ReadOnlyMemory<char>(instance.ToString().ToCharArray()) : ReadOnlyMemory<char>.Empty,
            };
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

        public static SettingIdentifier FromMetadata(SettingMetadata settingMetadata, string handle)
        {
            return new SettingIdentifier
            (
                prefix: settingMetadata.Prefix,
                scope: settingMetadata.Namespace,
                type: settingMetadata.TypeName,
                member: settingMetadata.MemberName,
                handle: handle
            );
        }

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

        //public static implicit operator SettingName(string settingName) => Parse(settingName);

        public static implicit operator string(SettingIdentifier settingIdentifier) => settingIdentifier?.ToString();

        public static implicit operator SoftString(SettingIdentifier settingIdentifier) => settingIdentifier?.ToString();

//        public static implicit operator UriString(SettingIdentifier settingIdentifier)
//        {
//            var path = new[]
//            {
//                settingIdentifier.Namespace?.Replace('.', '-'),
//                settingIdentifier.Type,
//                settingIdentifier.Member,
//            };
//
//            var query = (SoftString)new (SoftString Key, SoftString Value)[]
//            {
//                ("prefix", settingIdentifier.Prefix),
//                ("instance", settingIdentifier.Instance)
//            }
//            .Where(x => x.Value)
//            .Select(x => $"{x.Key.ToString()}={x.Value.ToString()}")
//            .Join("&");
//
//            return $"setting:{path.Where(Conditional.IsNotNullOrEmpty).Join(".")}{(query ? $"?{query.ToString()}" : string.Empty)}";
//        }

        public static bool operator ==(SettingIdentifier x, SettingIdentifier y) => AutoEquality<SettingIdentifier>.Comparer.Equals(x, y);

        public static bool operator !=(SettingIdentifier x, SettingIdentifier y) => !(x == y);

        #region IEquatable<SettingName>

        public bool Equals(SettingIdentifier other) => AutoEquality<SettingIdentifier>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is SettingIdentifier settingName && Equals(settingName);

        public override int GetHashCode() => AutoEquality<SettingIdentifier>.Comparer.GetHashCode(this);

        #endregion
    }

    /*

    Setting names are ordered by the usage frequency.

    Type.Property,Instance
    Property,Instance
    Namespace+Type.Property,Instance

    Type.Property
    Property
    Namespace+Type.Property

     */

    public enum SettingNameStrength
    {
        Inherit = -1,

        /// <summary>
        /// Member
        /// </summary>
        Low = 0,

        /// <summary>
        /// Type.Member
        /// </summary>
        Medium = 1,

        /// <summary>
        /// Namespace+Type.Member
        /// </summary>
        High = 2,
    }
    
    public enum PrefixHandling
    {
        Inherit = -1,
        Disable = 0,
        Enable = 1,
    }
}