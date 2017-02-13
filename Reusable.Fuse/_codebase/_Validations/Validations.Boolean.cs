namespace Reusable.Fuse
{
    public static class BooleanValidation
    {
        public static ISpecificationContext<bool> IsTrue(this ISpecificationContext<bool> specificationContext, string message = null)
        {
            return specificationContext.Check(
                value => value,
                message ?? $"\"{specificationContext.MemberName}\" must be \"{bool.TrueString}\".");
        }

        public static ISpecificationContext<bool> IsFalse(this ISpecificationContext<bool> specificationContext, string message = null)
        {
            return specificationContext.Check(
                value => !value,
                message ?? $"\"{specificationContext.MemberName}\" must be \"{bool.FalseString}\".");
        }
    }
}
