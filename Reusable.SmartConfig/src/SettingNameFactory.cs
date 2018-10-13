using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Annotations;
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

    public enum SettingNamePrefix
    {
        //Inherit = -1,
        None = 0,
        AssemblyName = 1
    }

    public interface ISettingNameFactory
    {
        [NotNull]
        SettingName CreateSettingName([NotNull] SettingName settingName, SettingNameConvention convention, [CanBeNull] string prefix);
    }

    public class SettingNameFactory : ISettingNameFactory
    {
        private static readonly IList<IList<Token>> TokenCombinations = new IList<Token>[]
        {
            new[] { Token.Member },
            new[] { Token.Type, Token.Member },
            new[] { Token.Namespace, Token.Type, Token.Member },
        };        
        
        public SettingName CreateSettingName(SettingName settingName, SettingNameConvention convention, string prefix)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            var tokens = TokenCombinations[(int)convention.Complexity].ToDictionary(t => t, t => settingName[t]);
            
            if (!settingName[Token.Instance].IsEmpty)
            {
                tokens.Add(Token.Instance, settingName[SettingNameToken.Instance]);
            }

            if (convention.PrefixHandling == PrefixHandling.Enable && prefix.IsNotNullOrEmpty())
            {
                tokens.Add(SettingNameToken.Prefix, prefix.AsMemory());
            } 

            return new SettingName(tokens);
        }        
    }
}