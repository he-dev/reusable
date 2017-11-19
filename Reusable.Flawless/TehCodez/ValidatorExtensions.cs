using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.Flawless
{
    public static class ValidatorExtensions
    {
        public static IEnumerable<IValidation<T>> ValidateWith<T>([NotNull] this T obj, [NotNull] Validator<T> validator)
        {
            return validator.Validate(obj);
        }

        public static bool AllSuccess<T>([NotNull] this IEnumerable<IValidation<T>> validations)
        {
            if (validations == null) throw new ArgumentNullException(nameof(validations));

            return validations.All(v => v.Success);
        }

        //[ContractAnnotation("=> halt")]
        public static void ThrowIfNotValid<T>([NotNull] this IEnumerable<IValidation<T>> validations)
        {
            // Materialize the validation so that we don't enumerate them twice.
            validations = validations.ToList();

            if (validations.AllSuccess())
            {
                return;
            }

            var requriements = validations.Aggregate(
                new StringBuilder(),
                (result, validation) => result.AppendLine($"{validation}")
            ).ToString();

            throw DynamicException.Factory.CreateDynamicException
            (
                name: $"{typeof(T).Name}Validation{nameof(Exception)}",
                message: $"Object of type '{typeof(T).Name}' does not meet one or more requirements.{Environment.NewLine}{Environment.NewLine}{requriements}",
                innerException: null
            );
        }
    }
}