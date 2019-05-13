using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Data
{
    public static class ImmutableSessionKey<TNamespace>
    {
        [DebuggerStepThrough]
        public static string Create(LambdaExpression keyExpression)
        {
            var keyFactory = keyExpression.ToMemberExpression().Member.GetCustomAttribute<KeyFactoryAttribute>(inherit: true) ?? new TypedKeyFactoryAttribute();
            return keyFactory.CreateKey(keyExpression);
        }
        
        [DebuggerStepThrough]
        public static string Create<TMember>(Expression<System.Func<TNamespace, TMember>> selectMember)
        {
            return Create((LambdaExpression)selectMember);
        }
    }
}