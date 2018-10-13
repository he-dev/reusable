using System;
using System.Linq;
using System.Linq.Custom;
using System.Text;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Validation
{
    public static class WeelidationResultExtensions
    {
        [CanBeNull]
        public static T ThrowIfInvalid<T>([NotNull] this WeelidationResult<T> weelidationResult)
        {
            if (weelidationResult == null) throw new ArgumentNullException(nameof(weelidationResult));

            return
                weelidationResult
                    ? weelidationResult
                    : throw DynamicException.Create
                    (
                        $"{typeof(T).ToPrettyString()}Validation",
                        $"Object of type '{typeof(T).ToPrettyString()}' does not meet one or more requirements.{Environment.NewLine}{Environment.NewLine}" +
                        $"{weelidationResult[false].Select(Func.ToString).Join(Environment.NewLine)}"
                    );
        }
    }
}