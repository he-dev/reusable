using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Validation
{
    public static class WeelidationResultExtensions
    {
        [CanBeNull]
        public static T ThrowIfInvalid<T>(this WeelidationResult<T> weelidationResult)
        {
            return
                weelidationResult.Success
                    ? weelidationResult
                    : throw DynamicException.Factory.CreateDynamicException
                    (
                        name: $"{typeof(T).ToPrettyString()}Validation",
                        message: 
                            $"Object of type '{typeof(T).ToPrettyString()}' does not meet one or more requirements.{Environment.NewLine}{Environment.NewLine}" +
                            $"{string.Join(Environment.NewLine, weelidationResult.Select(x => x.ToString()))}"
                    );
        }
    }
}