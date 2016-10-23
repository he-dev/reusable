using Reusable.Extensions;

namespace Reusable.FluentValidation.Validations
{
    public static class BooleanValidation
    {
        public static IValidationContext<bool> IsTrue(this IValidationContext<bool> context, string message = null)
        {
            return context.Validate(
                value => value,
                message ?? "{MemberName:dq} must be {TrueString:dq}.".Format(new { context.MemberName, bool.TrueString }));
        }

        public static IValidationContext<bool> IsFalse(this IValidationContext<bool> context, string message = null)
        {
            return context.Validate(
                value => !value,
                message ?? "{MemberName:dq} must be {FalseString:dq}.".Format(new { context.MemberName, bool.FalseString }));
        }
    }
}
