using System;

namespace Reusable.Beaver.Policies
{
    /// <summary>
    /// This feature policy supports the null-pattern and is used for removing feature policies.
    /// </summary>
    public class Remove : FeaturePolicy
    {
        internal Remove() { }

        public override FeatureState State(FeatureContext context)
        {
            throw new NotSupportedException("This feature policy is to be used only for removing feature policies.");
        }
    }
}