using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Essentials.Extensions;
using Reusable.Quickey.Tokens;

namespace Reusable.Quickey
{
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
            // Get only own formatters and not inherited ones.
            var formatters =
                from f in member.GetCustomAttributes<SelectorTokenFormatterAttribute>(inherit: false)
                where f.Token is null || GetType().IsInstanceOfType(f.Token)
                select f;

            var formatted = formatters.Aggregate(token, (current, formatter) => formatter.Format(current));

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
            return new SchemeToken(Format(context.Metadata.Member, _name));
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
            return new NamespaceToken(Format(context.Metadata.Member, _name ?? context.Metadata.Member.ReflectedType.Namespace));
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
            // Get the first non-ignored type.
            var types =
                from t in SelectorPath.Enumerate(context.Metadata.Member).OfType<Type>()
                let attribute = t.GetCustomAttribute<SelectorAttribute?>()
                where attribute is null || !attribute.Ignore
                select t;

            var type = types.First();

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
            return new MemberToken(Format(context.Metadata.Member, _name ?? context.Metadata.Member.Name));
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
                    ? new IndexToken(Format(context.Metadata.Member, parameter.Index))
                    : new IndexToken(string.Empty);
        }

        public class Parameter : ISelectorTokenFactoryParameter
        {
            public string Index { get; set; }
        }
    }
}