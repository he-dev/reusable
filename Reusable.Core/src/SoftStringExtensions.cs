using JetBrains.Annotations;

namespace Reusable
{
    public static class SoftStringExtensions
    {
        [ContractAnnotation("value: null => true")]
        public static bool IsNullOrEmpty(this SoftString value)
        {
            return SoftString.IsNullOrEmpty(value);
        }

        [ContractAnnotation("value: null => false")]
        public static bool IsNotNullOrEmpty(this SoftString value)
        {
            return !SoftString.IsNullOrEmpty(value);
        }
    }
}