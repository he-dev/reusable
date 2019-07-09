using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Quickey
{
    // [Prefix:][Name.space+][Type.]Member[[Index]]
    // [UsePrefix("blub"), UseNamespace, UseType, UseMember, UseIndex?]

    public interface ISelectorNameFactory
    {
        [NotNull]
        SelectorName CreateSelectorName(Selector selector);
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class RenameAttribute : Attribute
    {
        private readonly string _name;

        public RenameAttribute(string name) => _name = name;

        public override string ToString() => _name;
    }

    [PublicAPI]
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public abstract class SelectorNameFactoryAttribute : Attribute, ISelectorNameFactory
    {
        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public abstract SelectorName CreateSelectorName(Selector selector);
        
        protected string Transform(string name, MemberInfo member)
        {
            return
                member
                    .GetCustomAttributes<SelectorNameTransformAttribute>(false) // Get only own factories and not inherited ones.
                    .Aggregate(name, (current, fix) => fix.Apply(current));
        }
    }

    public class UseSchemeAttribute : SelectorNameFactoryAttribute
    {
        private readonly string _prefix;

        public UseSchemeAttribute(string prefix)
        {
            _prefix = prefix;
            Suffix = ":";
        }

        public override SelectorName CreateSelectorName(Selector selector)
        {
            return new SelectorName(_prefix) { Suffix = Suffix };
        }
    }

    public class UseNamespaceAttribute : SelectorNameFactoryAttribute
    {
        public UseNamespaceAttribute()
        {
            Suffix = "+";
        }

        public override SelectorName CreateSelectorName(Selector selector)
        {
            var type = selector.DeclaringType;
            return new SelectorName(type.Namespace) { Suffix = Suffix };
        }
    }

    public class UseTypeAttribute : SelectorNameFactoryAttribute
    {
        public UseTypeAttribute()
        {
            Suffix = ".";
        }

        public override SelectorName CreateSelectorName(Selector selector)
        {
            var type = selector.DeclaringType;

            var typeName =
                type.GetCustomAttribute<RenameAttribute>()?.ToString() is string rename
                    ? rename
                    : Transform(type.ToPrettyString(), type);

            return new SelectorName(typeName) { Suffix = Suffix };
        }
    }

    public class UseMemberAttribute : SelectorNameFactoryAttribute
    {
        public override SelectorName CreateSelectorName(Selector selector)
        {
            var memberName =
                selector.Member.GetCustomAttribute<RenameAttribute>()?.ToString() is string rename
                    ? rename
                    : Transform(selector.Member.Name, selector.Member);

            return new SelectorName(memberName);
        }
    }
}