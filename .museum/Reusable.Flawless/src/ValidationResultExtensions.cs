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
        /// Throws a dynamic ValidationException when data contains either errors or warnings, otherwise it returns T.
        /// </summary>
        public static T ThrowOnFailure<T>(this ValidationResultCollection<T> results)
        {
            return results.Failures().Any() ? throw results.CreateValidationException() : results;
        }

        /// <summary>
        /// Throws a dynamic ValidationException when data contains errors, otherwise it returns T.
        /// </summary>
        public static T ThrowOnError<T>(this ValidationResultCollection<T> results)
        {
            return results.Errors().Any() ? throw results.CreateValidationException() : results;
        }

        private static Exception CreateValidationException<T>(this ValidationResultCollection<T> results)
        {
            return DynamicException.Create
            (
                $"{typeof(T).ToPrettyString()}Validation",
                $"Data does not meet one or more requirements:{Environment.NewLine}" +
                $"{results.Select(r => $"- {r.ToString()}").Join(Environment.NewLine)}"
            );
        }

        public static IEnumerable<ValidationSuccess> Successful<T>(this ValidationResultCollection<T> results)
        {
            return results.OfType<ValidationSuccess>();
        }

        public static IEnumerable<ValidationWarning> Warnings<T>(this ValidationResultCollection<T> results)
        {
            return results.OfType<ValidationWarning>();
        }

        public static IEnumerable<ValidationError> Errors<T>(this ValidationResultCollection<T> results)
        {
            return results.OfType<ValidationError>();
        }

        public static IEnumerable<IValidationFailure> Failures<T>(this ValidationResultCollection<T> results)
        {
            return results.OfType<IValidationFailure>();
        }
    }
}