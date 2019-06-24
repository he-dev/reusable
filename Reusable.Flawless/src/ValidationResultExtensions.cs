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
        /// Throws a dynamic ValidationException when data is not valid or returns T.
        /// </summary>
        public static T ThrowIfNotValid<T>(this ValidationResultCollection<T> results)
        {
            return
                results.All(r => r is Information)
                    ? results
                    : throw DynamicException.Create
                    (
                        $"{typeof(T).ToPrettyString()}Validation",
                        $"Data does not meet one or more requirements." +
                        $"{Environment.NewLine}{Environment.NewLine}" +
                        $"{results.Select(Func.ToString).Join(Environment.NewLine)}"
                    );
        }
    }
}