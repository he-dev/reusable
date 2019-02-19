using System;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Exceptionizer;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    public static class ExpressValidationResultLookupExtensions
    {
        /// <summary>
        /// Throws validation-exception when validation failed.
        /// </summary>
        [CanBeNull]
        public static T Assert<T>([NotNull] this ExpressValidationResultLookup<T> checkLookup)
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