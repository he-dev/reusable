using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reusable.Extensions;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    using Token = SettingNameToken;

    public static class SettingNameExtensions
    {
        //private static readonly IList<IList<Token>> TokenCombinations = new IList<Token>[]
        //{
        //    new[] { Token.Member },
        //    new[] { Token.Type, Token.Member },
        //    new[] { Token.Namespace, Token.Type, Token.Member },
        //};

        private static readonly IImmutableDictionary<SettingNameStrength, IImmutableList<Token>> NamingConventions = new Dictionary<SettingNameStrength, IImmutableList<Token>>
        {
            [SettingNameStrength.Low] = new[] { Token.Member }.ToImmutableList(),
            [SettingNameStrength.Medium] = new[] { Token.Type, Token.Member }.ToImmutableList(),
            [SettingNameStrength.High] = new[] { Token.Namespace, Token.Type, Token.Member }.ToImmutableList(),
        }
        .ToImmutableDictionary();

        public static SettingName ModifySettingName
        (
            this SettingName settingName,
            (SettingNameStrength Strength, string Prefix, PrefixHandling PrefixHandling) convention
        )
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            var namingConventions = NamingConventions.Add(SettingNameStrength.Inherit, NamingConventions[SettingNameStrength.Medium]);

            var namingConvention = namingConventions[convention.Strength].ToDictionary(t => t, t => settingName[t]);

            if (!settingName[Token.Instance].IsEmpty)
            {
                namingConvention.Add(Token.Instance, settingName[Token.Instance]);
            }

            if (convention.PrefixHandling == PrefixHandling.Enable && convention.Prefix.IsNotNullOrEmpty())
            {
                namingConvention.Add(Token.Prefix, convention.Prefix.AsMemory());
            }

            return new SettingName(namingConvention);
        }

        public static IEnumerable<string> GetSettingNameParts
        (
            this IEnumerable<string> settingNameParts,
            SettingNameStrength strength
        )
        {
            return settingNameParts.Skip(3 - (int)strength);
        }
    }
}