using System.Text.RegularExpressions;
using Reusable.Extensions;

namespace Reusable.Validations
{
    public static class StringValidation
    {
        public static IValidationContext<string> IsNullOrEmpty(this IValidationContext<string> context, string message = null)
        {
            return context.Validate(
                value => string.IsNullOrEmpty(context.Value),
                message ?? "{MemberName:dq} must be null or empty.".Format(new { context.MemberName }));
        }

        public static IValidationContext<string> IsNotNullOrEmpty(this IValidationContext<string> context, string message = null)
        {
            return context.Validate(
                value => !string.IsNullOrEmpty(context.Value),
                message ?? "{MemberName:dq} must not be null or empty.".Format(new { context.MemberName }));
        }
        
        public static IValidationContext<string> IsMatch(this IValidationContext<string> context, string pattern, RegexOptions options = RegexOptions.None, string message = null)
        {
            return context.Validate(
                value => Regex.IsMatch(value, pattern, options),
                message ?? "{MemberName:dq} must match {pattern:dq}.".Format(new { context.MemberName, pattern }));
        }

        public static IValidationContext<string> IsNotMatch(this IValidationContext<string> context, string pattern, RegexOptions options = RegexOptions.None, string message = null)
        {
            return context.Validate(
                value => !Regex.IsMatch(value, pattern, options),
                message ?? "{MemberName:dq} must not match {pattern:dq}.".Format(new { context.MemberName, pattern }));
        }
    }    
}
