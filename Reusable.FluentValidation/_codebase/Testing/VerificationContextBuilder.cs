using System;
using System.Diagnostics;

namespace Reusable.Testing
{
    public static class VerificationContextBuilder
    {
        [DebuggerStepThrough]
        public static IValidationContext<T> Verify<T>(this T value, string memberName)
        {
            return new ValidationContext<T, VerificationException>(value, memberName);
        }

        [DebuggerStepThrough]
        public static IValidationContext<T> Verify<T>(this T value)
        {
            return Verify(value, "value");
        }

        [DebuggerStepThrough]
        public static IValidationContext<T> Cast<T>(this IValidationContext<object> context)
        {
            return new ValidationContext<T, VerificationException>((T)context.Value, context.MemberName);
        }

        //[DebuggerStepThrough]
        //public static IValidationContext<T2> And<T1, T2>(this IValidationContext<T1> context, Func<T1, T2> argumentSelector)
        //{
        //    return new SpecificationContext<T2>(argumentSelector(context.Value), "<anonymous>", ctx => new VerificationException(ctx.Message));
        //}

        [DebuggerStepThrough]
        public static IValidationContext<T2> And<T1, T2>(this IValidationContext<T1> context, Func<T1, T2> argumentSelector, string memberName)
        {
            return Verify(argumentSelector(context.Value), memberName);
        }
    }
}