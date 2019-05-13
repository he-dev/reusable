using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Reusable.Extensions;

namespace Reusable.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class KeyFactoryAttribute : Attribute
    {
        public abstract string CreateKey(LambdaExpression keyExpression);
    }

    public class SimpleKeyFactoryAttribute : KeyFactoryAttribute
    {
        public override string CreateKey(LambdaExpression keyExpression)
        {
            return keyExpression.ToMemberExpression().Member.Name;
        }
    }

    public class TypedKeyFactoryAttribute : KeyFactoryAttribute
    {
        public override string CreateKey(LambdaExpression keyExpression)
        {
            var memberExpression = keyExpression.ToMemberExpression();
            return $"{GetScopeName(memberExpression.Member.DeclaringType)}.{memberExpression.Member.Name}";
        }

        private static string GetScopeName(Type type) => Regex.Replace(type.ToPrettyString(), "^I|Namespace$", string.Empty);
    }
}