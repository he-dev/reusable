using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    public static class ValidationContextExtensions
    {
        [CanBeNull]
        public static T ThrowIfNotValid<T>(this ValidationContext<T> context)
        {
            if (context)
            {
                return context.Object;
            }

            var requriements = context.Results.Aggregate(
                new StringBuilder(),
                (result, validation) => result.AppendLine($"{validation}")
            ).ToString();

            throw DynamicException.Factory.CreateDynamicException
            (
                name: $"{typeof(T).ToPrettyString()}Validation{nameof(Exception)}",
                message: $"Object of type '{typeof(T).ToPrettyString()}' does not meet one or more requirements.{Environment.NewLine}{Environment.NewLine}{requriements}",
                innerException: null
            );
        }
    }
}