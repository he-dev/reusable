using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Data
{
    internal static class ImmutableSessionScope<T>
    {
        [DebuggerStepThrough]
        public static string Key(LambdaExpression xItem) => $"{GetScopeName<T>()}.{GetMemberName(xItem)}";

        private static string GetScopeName<TScope>() => Regex.Replace(typeof(TScope).ToPrettyString(), "^I", string.Empty);

        private static string GetMemberName(LambdaExpression xItem)
        {
            return
                xItem.Body is MemberExpression me
                    ? me.Member.Name
                    : throw DynamicException.Create
                    (
                        $"NotMemberExpression",
                        $"Cannot use expression '{xItem}' because Get/Set expression must be member-expressions."
                    );
        }
    }
}