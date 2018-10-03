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

    public enum SettingNameConvention
    {
        Simple, // Member
        Normal, // Type.Member
        Extended, // Namespace+Type.Member - Exact, Precise, Full, Complete, Long, Strong
        //SimpleRestricted, // Assembly:Member SimpleWithModule | SimpleWithAssembly | Simple
        //NormalRestricted, // Assembly:Type.Member       
        //ExtendedRestricted, // Assembly:Namespace+Type.Member
    }

    public class SettingNameGenerator : ISettingNameGenerator
    {
        private static readonly IList<IList<Token>> TokenCombinations = new IList<Token>[]
        {
            new[] { Token.Member },
            new[] { Token.Type, Token.Member },
            new[] { Token.Namespace, Token.Type, Token.Member },
            //new[] {Token.Assembly, Token.Member},
            //new[] {Token.Assembly, Token.Type, Token.Member},
            //new[] {Token.Assembly, Token.Namespace, Token.Type, Token.Member},
        };

        public IEnumerable<SettingName> GenerateSettingNames(SettingName settingName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            return
            (
                settingName[Token.Instance].IsEmpty
                    ? GenerateSettingNamesWithoutInstance(settingName)
                    : GenerateSettingNamesWithInstance(settingName)
            ).Distinct();
        }

        private static IEnumerable<SettingName> GenerateSettingNamesWithInstance(SettingName settingName)
        {
            return
                from tokens in TokenCombinations
                select CreateSettingName(tokens.Append(Token.Instance), settingName);
        }

        private static IEnumerable<SettingName> GenerateSettingNamesWithoutInstance(SettingName settingName)
        {
            return
                from tokens in TokenCombinations
                select CreateSettingName(tokens, settingName);
        }

        private static SettingName CreateSettingName(IEnumerable<Token> tokens, SettingName settingName)
        {
            return new SettingName(tokens.ToDictionary(t => t, t => settingName[t]));
        }
    }

    public class SettingNameFactory : ISettingNameFactory
    {
        private static readonly IList<IList<Token>> TokenCombinations = new IList<Token>[]
        {
            new[] { Token.Member },
            new[] { Token.Type, Token.Member },
            new[] { Token.Namespace, Token.Type, Token.Member },
        };

        public IEnumerable<SettingName> CreateSettingNames(SettingName settingName, SettingNameOption settingNameOption)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            return
                from tokens in settingNameOption.Convention.HasValue ? new[] { TokenCombinations[(int)settingNameOption.Convention] } : TokenCombinations
                let x = settingName[Token.Instance].IsEmpty ? tokens : tokens.Append(Token.Instance)
                let y = settingNameOption.IsRestricted == true ? x.Prepend(SettingNameToken.Assembly) : x
                select CreateSettingName(y, settingName);
        }

        private static SettingName CreateSettingName(IEnumerable<Token> tokens, SettingName settingName)
        {
            return new SettingName(tokens.ToDictionary(t => t, t => settingName[t]));
        }
    }
}