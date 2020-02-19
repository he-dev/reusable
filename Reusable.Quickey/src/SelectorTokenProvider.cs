using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Reusable.Quickey
{
    public interface ISelectorTokenFactoryProvider
    {
        /// <summary>
        /// Enumerates selector-tokens for each member from last to first.
        /// </summary>
        IEnumerable<ISelectorTokenFactory> GetSelectorTokenFactories(MemberInfo member);

        //IEnumerable<IEnumerable<ISelectorTokenFactory>> GetSelectorTokenFactories(MemberInfo member);
    }

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class SelectorTokenFactoryProviderAttribute : Attribute, ISelectorTokenFactoryProvider
    {
        public static ISelectorTokenFactoryProvider Default { get; } = new SelectorTokenFactoryProviderAttribute();

        public IEnumerable<ISelectorTokenFactory> GetSelectorTokenFactories(MemberInfo member)
        {
            var tokenFactoryGroups =
                from m in SelectorPath.Enumerate(member)
                select m.GetCustomAttributes<SelectorTokenFactoryAttribute>(inherit: false);

            foreach (var tokenFactoryGroup in tokenFactoryGroups)
            {
                var any = false;
                foreach (var tokenFactory in tokenFactoryGroup)
                {
                    yield return tokenFactory;
                    any = true;
                }

                if (any)
                {
                    yield break;
                }
            }
        }
    }
}