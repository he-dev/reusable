namespace Reusable
{
    public static class SoftStringExtensions
    {
        public static bool IsNullOrEmpty(this SoftString softString)
        {
            return SoftString.IsNullOrEmpty(softString);
        }
    }
}