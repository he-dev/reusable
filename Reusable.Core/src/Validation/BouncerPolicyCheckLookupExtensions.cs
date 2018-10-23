using System;
using System.Linq;
using System.Linq.Custom;
using System.Text;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Validation
{
    public static class BouncerPolicyCheckLookupExtensions
    {
        [CanBeNull]
        public static T ThrowIfInvalid<T>([NotNull] this BouncerPolicyCheckLookup<T> checkLookup)
        {
            if (checkLookup == null) throw new ArgumentNullException(nameof(checkLookup));

            return
                checkLookup
                    ? checkLookup
                    : throw DynamicException.Create
                    (
                        $"{typeof(T).ToPrettyString()}Validation",
                        $"Object does not meet one or more requirements.{Environment.NewLine}{Environment.NewLine}" +
                        $"{checkLookup[false].Select(Func.ToString).Join(Environment.NewLine)}"
                    );
        }
    }
}