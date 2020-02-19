using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.Quickey.Tokens;

namespace Reusable.Quickey
{
    // [Prefix:][Name.space+][Type.]Member[[Index]]
    // [UsePrefix("global"), UseNamespace, UseType, UseMember, UseIndex?]


    public interface ISelectorTokenFactory
    {
        SelectorToken CreateSelectorToken(SelectorContext context);
    }

    public interface ISelectorTokenFactoryParameter { }

    [PublicAPI]
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
    public abstract class SelectorTokenFactoryAttribute : Attribute, ISelectorTokenFactory
    {
        public string? Prefix { get; set; }

        public string? Suffix { get; set; }

        protected string Format(MemberInfo member, string token)
        {
            var formatted =
                member
                    // Get only own filters and not inherited ones.
                    .GetCustomAttributes<SelectorTokenFormatterAttribute>(inherit: false)
                    .Cast<ISelectorTokenFormatter>()
                    .Aggregate(token, (current, formatter) => formatter.Format(current));

            return $"{Prefix}{formatted}{Suffix}";
        }

        public abstract SelectorToken CreateSelectorToken(SelectorContext context);
    }

    public class UseSchemeAttribute : SelectorTokenFactoryAttribute
    {
        private readonly string _name;

        public UseSchemeAttribute(string name)
        {
            _name = name;
            Suffix = ":";
        }

        public override SelectorToken CreateSelectorToken(SelectorContext context)
        {
            return new SchemeToken(Format(context.Member, _name));
        }
    }

    public class UseNamespaceAttribute : SelectorTokenFactoryAttribute
    {
        private readonly string? _name;

        public UseNamespaceAttribute(string? name = default)
        {
            _name = name;
            Suffix = "+";
        }

        public override SelectorToken CreateSelectorToken(SelectorContext context)
        {
            return new NamespaceToken(Format(context.Member, _name ?? context.Member.ReflectedType.Namespace));
        }
    }

    public class UseTypeAttribute : SelectorTokenFactoryAttribute
    {
        private readonly string? _name;

        public UseTypeAttribute(string? name = default)
        {
            _name = name;
            Suffix = ".";
        }

        public override SelectorToken CreateSelectorToken(SelectorContext context)
        {
            var type = context.Member.ReflectedType!;
            return new TypeToken(Format(type, _name ?? type?.ToPrettyString()));
        }
    }

    public class UseMemberAttribute : SelectorTokenFactoryAttribute
    {
        private readonly string? _name;

        public UseMemberAttribute(string? name = default)
        {
            _name = name;
        }

        public override SelectorToken CreateSelectorToken(SelectorContext context)
        {
            return new MemberToken(Format(context.Member, _name ?? context.Member.Name));
        }
    }

    public class UseIndexAttribute : SelectorTokenFactoryAttribute
    {
        public UseIndexAttribute()
        {
            Prefix = "[";
            Suffix = "]";
        }

        public override SelectorToken CreateSelectorToken(SelectorContext context)
        {
            return
                context.TokenParameters.OfType<Parameter>().SingleOrDefault() is {} parameter
                    ? new IndexToken(Format(context.Member, parameter.Index))
                    : new IndexToken(string.Empty);
        }

        public class Parameter : ISelectorTokenFactoryParameter
        {
            public string Index { get; set; }
        }
    }
}