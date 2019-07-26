using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    [PublicAPI]
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// Throws a dynamic ValidationException when data is not valid or returns T.
        /// </summary>
        public static T ThrowOnFailure<T>(this ValidationResultCollection<T> results)
        {
            return
                results.Failures().Any()
                    ? throw DynamicException.Create
                    (
                        $"{typeof(T).ToPrettyString()}Validation",
                        $"Data does not meet one or more requirements." +
                        $"{Environment.NewLine}{Environment.NewLine}" +
                        $"{results.Select(Functions.ToString).Join(Environment.NewLine)}"
                    )
                    : results;
        }
        
        public static T ThrowOnError<T>(this ValidationResultCollection<T> results)
        {
            return
                results.Errors().Any()
                    ? throw DynamicException.Create
                    (
                        $"{typeof(T).ToPrettyString()}Validation",
                        $"Data does not meet one or more requirements." +
                        $"{Environment.NewLine}{Environment.NewLine}" +
                        $"{results.Select(Functions.ToString).Join(Environment.NewLine)}"
                    )
                    : results;
        }

        public static IEnumerable<IValidationFailure> Failures<T>(this ValidationResultCollection<T> results)
        {
            return results.OfType<IValidationFailure>();
        }
        
        public static IEnumerable<ValidationError> Errors<T>(this ValidationResultCollection<T> results)
        {
            return results.OfType<ValidationError>();
        }
        
        public static IEnumerable<ValidationWarning> Warnings<T>(this ValidationResultCollection<T> results)
        {
            return results.OfType<ValidationWarning>();
        }
        
        public static IEnumerable<ValidationSuccess> Successful<T>(this ValidationResultCollection<T> results)
        {
            return results.OfType<ValidationSuccess>();
        }
    }
}