using System;
using System.Linq;
using System.Linq.Custom;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// Throws validation-exception when validation failed.
        /// </summary>
        public static T ThrowIfValidationFailed<T>(this ValidationResultCollection<T> results)
        {
            return
                results.All(r => r is Information)
                    ? results
                    : throw DynamicException.Create
                    (
                        $"{typeof(T).ToPrettyString()}Validation",
                        $"Object does not meet one or more requirements.{Environment.NewLine}{Environment.NewLine}" +
                        $"{results.Select(Func.ToString).Join(Environment.NewLine)}"
                    );
        }
    }
}