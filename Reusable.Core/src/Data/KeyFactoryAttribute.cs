using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
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
            typeName = memberExpression.Member.DeclaringType.GetCustomAttributes<TypeNameCleanerAttribute>().Aggregate(typeName, (name, cleaner) => cleaner.Clean(name));
            return $"{typeName}.{memberExpression.Member.Name}";
        }
    }

    public interface ITypeNameCleaner
    {
        [NotNull]
        string Clean(string name);
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public abstract class TypeNameCleanerAttribute : Attribute, ITypeNameCleaner
    {
        public abstract string Clean(string name);
    }

    public class RemoveInterfacePrefixAttribute : TypeNameCleanerAttribute
    {
        public override string Clean(string name)
        {
            return Regex.Replace(name, "^I", string.Empty);
        }
    }
}