using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    using Token = SettingNameToken;

    /*

    Setting names are ordered by the usage frequency.

    Type.Property,Instance
    Property,Instance
    Namespace+Type.Property,Instance

    Type.Property
    Property
    Namespace+Type.Property

     */

    public enum SettingNameComplexity
    {
        /// <summary>
        /// Member
        /// </summary>
        Low,

        /// <summary>
        /// Type.Member
        /// </summary>
        Medium,

        /// <summary>
        /// Namespace+Type.Member
        /// </summary>
        High,
    }

    public interface ISettingNameFactory
    {
        [NotNull]
        SettingName CreateSettingName([NotNull] SettingName settingName, SettingNameConvention? settingNameConvention = default);        
    }

    public class SettingNameFactory : ISettingNameFactory
    {
        private readonly SettingNameConvention _settingNameConvention;

        private static readonly IList<IList<Token>> TokenCombinations = new IList<Token>[]
        {
            new[] { Token.Member },
            new[] { Token.Type, Token.Member },
            new[] { Token.Namespace, Token.Type, Token.Member },
        };

        public SettingNameFactory(SettingNameConvention settingNameConvention)
        {
            _settingNameConvention = settingNameConvention;
        }

        public SettingName CreateSettingName(SettingName settingName, SettingNameConvention? settingNameConvention = default)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            var actualSettingNameConvention = settingNameConvention ?? _settingNameConvention;

            var tokens = TokenCombinations[(int)actualSettingNameConvention.Complexity].AsEnumerable();
            tokens = settingName[Token.Instance].IsEmpty ? tokens : tokens.Append(Token.Instance);
            tokens = actualSettingNameConvention.IsRestricted ? tokens.Prepend(SettingNameToken.Assembly) : tokens;

            return new SettingName(tokens.ToDictionary(t => t, t => settingName[t]));
        }

        // public IEnumerable<SettingName> CreateSettingNames(SettingName settingName, SettingNameConvention settingNameConvention)
        // {
        //     if (settingName == null) throw new ArgumentNullException(nameof(settingName));
        //
        //     settingNameConvention = settingNameConvention.Merge(_settingNameConvention);
        //     
        //     return
        //         from tokens in settingNameConvention.Complexity.HasValue ? new[] { TokenCombinations[(int)settingNameConvention.Complexity] } : TokenCombinations
        //         let x = settingName[Token.Instance].IsEmpty ? tokens : tokens.Append(Token.Instance)
        //         let y = settingNameConvention.IsRestricted == true ? x.Prepend(SettingNameToken.Assembly) : x
        //         select CreateSettingName(y, settingName);
        // }

        // private static SettingName CreateSettingName(IEnumerable<Token> tokens, SettingName settingName)
        // {
        //     return new SettingName(tokens.ToDictionary(t => t, t => settingName[t]));
        // }
    }
}