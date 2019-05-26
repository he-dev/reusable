using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Data
{
    public interface IKeyFactory
    {
        [NotNull]
        string CreateKey(LambdaExpression keyExpression);
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public abstract class KeyFactoryAttribute : Attribute, IKeyFactory
    {
        public abstract string CreateKey(LambdaExpression keyExpression);
    }

    public class MemberKeyFactoryAttribute : KeyFactoryAttribute
    {
        public override string CreateKey(LambdaExpression keyExpression)
        {
            return keyExpression.ToMemberExpression().Member.Name;
        }
    }

    [Obsolete]
    public class TypedKeyFactoryAttribute : KeyFactoryAttribute
    {
        public override string CreateKey(LambdaExpression keyExpression)
        {
            var memberExpression = keyExpression.ToMemberExpression();
            return $"{GetScopeName(memberExpression.Member.DeclaringType)}.{memberExpression.Member.Name}";
        }

        private string GetScopeName(Type type) => Regex.Replace(type.ToPrettyString(), $"^I|Namespace$", string.Empty);
    }

//    public class PrettyTypeStringAttribute : KeyFactoryAttribute
//    {
//        public override string CreateKey(LambdaExpression keyExpression)
//        {
//            throw new NotImplementedException();
//        }
//    }

    public class TypeMemberKeyFactoryAttribute : KeyFactoryAttribute
    {
        public override string CreateKey(LambdaExpression keyExpression)
        {
            var memberExpression = keyExpression.ToMemberExpression();
            var typeName = memberExpression.Member.DeclaringType.ToPrettyString();
            typeName = memberExpression.Member.DeclaringType.GetCustomAttributes<TypeNameFixAttribute>().Aggregate(typeName, (name, cleaner) => cleaner.Apply(name));
            return $"{typeName}.{memberExpression.Member.Name}";
        }
    }

    public interface ITypeNameCleaner
    {
        [NotNull]
        string Apply(string name);
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public abstract class TypeNameFixAttribute : Attribute, ITypeNameCleaner
    {
        public abstract string Apply(string name);
    }

    public class RemoveAttribute : TypeNameFixAttribute
    {
        private readonly IEnumerable<string> _patterns;

        public RemoveAttribute([RegexPattern] params string[] patterns) => _patterns = patterns;

        public override string Apply(string name)
        {
            return Regex.Replace(name, $"({_patterns.Join("|")})", string.Empty);
        }
    }

    public class TrimEndAttribute : RemoveAttribute
    {
        public TrimEndAttribute([RegexPattern] string prefixPattern) : base($"^{prefixPattern}") { }
    }

    public class TrimStartAttribute : RemoveAttribute
    {
        public TrimStartAttribute([RegexPattern] string suffixPattern) : base($"{suffixPattern}$") { }
    }

    public class KeyFactory : IKeyFactory
    {
        [NotNull] public static readonly IKeyFactory Default = new KeyFactory();

        public string CreateKey(LambdaExpression selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            var member = selector.ToMemberExpression().Member;
            return
                GetKeyFactory(member)
                    .FirstOrDefault(Conditional.IsNotNull)
                    ?.CreateKey(selector)
                ?? throw DynamicException.Create("KeyFactoryNotFound", $"Could not find key-factory on '{selector}'.");
        }

        [NotNull, ItemCanBeNull]
        private static IEnumerable<IKeyFactory> GetKeyFactory(MemberInfo member)
        {
            // Member's attribute has a higher priority and can override type's default factory.
            yield return member.GetCustomAttribute<KeyFactoryAttribute>();
            yield return member.DeclaringType?.GetCustomAttribute<KeyFactoryAttribute>();
        }
    }
}