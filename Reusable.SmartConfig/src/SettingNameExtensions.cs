using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    using Token = SettingNameToken;

    public static class SettingNameExtensions
    {
        private static readonly IList<IList<Token>> TokenCombinations = new IList<Token>[]
        {
            new[] { Token.Member },
            new[] { Token.Type, Token.Member },
            new[] { Token.Namespace, Token.Type, Token.Member },
        };

        public static SettingName ModifySettingName
        (
            this SettingName settingName,
            SettingNameStrength strength,
            string prefix,
            PrefixHandling prefixHandling
        )
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            var tokens = TokenCombinations[(int)strength].ToDictionary(t => t, t => settingName[t]);

            if (!settingName[Token.Instance].IsEmpty)
            {
                tokens.Add(Token.Instance, settingName[Token.Instance]);
            }

            if (prefixHandling == PrefixHandling.Enable && prefix.IsNotNullOrEmpty())
            {
                tokens.Add(Token.Prefix, prefix.AsMemory());
            }

            return new SettingName(tokens);
        }
    }
}