using System;
using System.Text.RegularExpressions;

namespace Reusable.Fuse
{
    public static class StringValidation
    {
        public static ISpecificationContext<string> IsNullOrEmpty(this ISpecificationContext<string> specificationContext, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => string.IsNullOrEmpty(specificationContext.Value),
                message ?? $"\"{specificationContext.MemberName}\" must be null or empty.");
        }

        public static ISpecificationContext<string> IsNotNullOrEmpty(this ISpecificationContext<string> specificationContext, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => !string.IsNullOrEmpty(specificationContext.Value),
                message ?? $"\"{specificationContext.MemberName}\" must not be null or empty.");
        }
        
        public static ISpecificationContext<string> IsMatch(this ISpecificationContext<string> specificationContext, string pattern, RegexOptions options = RegexOptions.None, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => Regex.IsMatch(value, pattern, options),
                message ?? $"\"{specificationContext.MemberName}\" value \"({specificationContext.Value})\" must match \"{pattern}\".");
        }

        public static ISpecificationContext<string> IsNotMatch(this ISpecificationContext<string> specificationContext, string pattern, RegexOptions options = RegexOptions.None, string message = null)
        {
            return specificationContext.AssertIsTrue(
                value => !Regex.IsMatch(value, pattern, options),
                message ?? $"\"{specificationContext.MemberName}\" value \"({specificationContext.Value})\" must not match \"{pattern}\".");
        }
    }    
}
