namespace Reusable.Fuse
{
    public static class BooleanValidation
    {
        public static ICurrent<bool> IsTrue(this ICurrent<bool> current, string message = null)
        {
            return current.Check(
                value => value,
                message ?? $"\"{current.MemberName}\" must be \"{bool.TrueString}\".");
        }

        public static ICurrent<bool> IsFalse(this ICurrent<bool> current, string message = null)
        {
            return current.Check(
                value => !value,
                message ?? $"\"{current.MemberName}\" must be \"{bool.FalseString}\".");
        }
    }
}
