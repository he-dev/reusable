using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Reusable.Quickey
{
    public interface ISelectorTokenProvider
    {
        /// <summary>
        /// Enumerates selector-names for each member from last to first.
        /// </summary>
        [NotNull, ItemNotNull]
        IEnumerable<IImmutableList<SelectorToken>> GetSelectorTokens(MemberInfo member);

        IEnumerable<IImmutableList<ISelectorTokenFactory>> GetSelectorTokenFactories(MemberInfo member);
    }

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class SelectorTokenProviderAttribute : Attribute, ISelectorTokenProvider
    {
        public static ISelectorTokenProvider Default { get; } = new SelectorTokenProviderAttribute();

        public IEnumerable<IImmutableList<SelectorToken>> GetSelectorTokens(MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            return
                from selectorTokenFactories in GetSelectorTokenFactories(member)
                where selectorTokenFactories.Any()
                let selectorTokens = selectorTokenFactories.Select(f => f.CreateSelectorToken(member, default)).ToImmutableList()
                where selectorTokens.Any()
                select selectorTokens;
        }

        // Get own attributes or inherited.
        public IEnumerable<IImmutableList<ISelectorTokenFactory>> GetSelectorTokenFactories(MemberInfo member)
        {
            return
                from m in member.Path()
                where m.IsDefined(typeof(SelectorTokenFactoryAttribute))
                select m.GetCustomAttributes<SelectorTokenFactoryAttribute>().Cast<ISelectorTokenFactory>().ToImmutableList();
        }
    }
}