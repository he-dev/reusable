using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Validation
{
    public static class DuckValidationResultExtensions
    {
        [CanBeNull]
        public static T ThrowOrDefault<T>(this DuckValidationResult<T> duckValidationResult)
        {
            return
                duckValidationResult.Success
                    ? duckValidationResult
                    : throw DynamicException.Factory.CreateDynamicException
                    (
                        name: $"{typeof(T).ToPrettyString()}Validation{nameof(Exception)}",
                        message: 
                            $"Object of type '{typeof(T).ToPrettyString()}' does not meet one or more requirements.{Environment.NewLine}{Environment.NewLine}" +
                            $"{string.Join(Environment.NewLine, duckValidationResult.Select(x => x.ToString()))}"
                    );
        }
    }
}