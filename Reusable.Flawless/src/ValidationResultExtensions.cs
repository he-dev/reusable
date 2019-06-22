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
        public static T ThrowIfValidationFailed<T>(this (T Value, ILookup<bool, IValidationResult<T>> Results) lookup)
        {
            return
                lookup.Results[false].Any()
                    ? throw DynamicException.Create
                    (
                        $"{typeof(T).ToPrettyString()}Validation",
                        $"Object does not meet one or more requirements.{Environment.NewLine}{Environment.NewLine}" +
                        $"{lookup.Results[false].Select(Func.ToString).Join(Environment.NewLine)}"
                    )
                    : default(T);
        }
    }
}