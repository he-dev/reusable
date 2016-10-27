using System;
using System.Diagnostics;

namespace Reusable
{
    public static class ValidationContextBuilder
    {
        [DebuggerNonUserCode]
        public static IValidationContext<T> Validate<T>(this T value)
        {
            return Validate(value, "value");
        }

        [DebuggerNonUserCode]
        public static IValidationContext<T> Validate<T>(this T value, string memberName)
        {
            return new ValidationContext<T, ValidationException>(value, memberName);
        }

        //[DebuggerStepThrough]
        //public static SpecificationContext<TArg> MemberName<TArg>(this SpecificationContext<TArg> context, string memberName)
        //{
        //    return new SpecificationContext<TArg>(context.Value, memberName);
        //}

        //public static SpecificationContext<TArg> As<TArg>(this SpecificationContext<TArg> context, Action<Exception> createException)
        //{
        //    return new SpecificationContext<TArg>(context.Value, context.MemberName, createException);
        //}

        //public static SpecificationContext<TArg2> And<TArg, TArg2>(this SpecificationContext<TArg> context, TArg2 value, string memberName = "")
        //{
        //    return new SpecificationContext<TArg2>(value, memberName);
        //}

        //[DebuggerStepThrough]
        //public static IValidationContext<T2> And<T1, T2>(this IValidationContext<T1> context, Func<T1, T2> selectNewValue)
        //{
        //    if (context == null)throw new ValidationException($"'{nameof(context)}' must not be null.");
        //    if (selectNewValue == null)throw new ValidationException($"'{nameof(selectNewValue)}' must not be null.");

        //    return Validate(selectNewValue(context.Value));
        //}

        [DebuggerStepThrough]
        public static IValidationContext<T2> And<T1, T2>(this IValidationContext<T1> context, Func<T1, T2> selectNewValue, string memberName)
        {
            if (context == null) throw new ValidationException($"'{nameof(context)}' must not be null.");
            if (selectNewValue == null) throw new ValidationException($"'{nameof(selectNewValue)}' must not be null.");

            return Validate(selectNewValue(context.Value), memberName);
        }

        //[DebuggerStepThrough]
        //public static IValidationContext<T> OnFailure<T>(this IValidationContext<T> context, Func<IValidationContext<T>, Exception> createException)
        //{
        //    if (context == null) throw new ValidationException($"'{nameof(context)}' must not be null.");
        //    if (createException == null) throw new ValidationException($"'{nameof(createException)}' must not be null.");

        //    return new SpecificationContext<T>(context.Value, context.MemberName, createException);
        //}
    }

    
}
