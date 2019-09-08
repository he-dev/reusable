using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Flawless
{
    public interface IValidator<in T>
    {
        IEnumerable<IValidationResult> Validate(T obj, IImmutableContainer context);
    }

    public static class Validator
    {
        public static Validator<T> Validate<T>(Action<ValidationRuleBuilder<T>> configureBuilder)
        {
            var builder = new ValidationRuleBuilder<T>(default, (Expression<Func<T, IImmutableContainer, T>>)((x, ctx) => x));
            configureBuilder(builder);
            return new Validator<T>(builder.Build<T>());
        }
        
    }

    [PublicAPI]
    public class Validator<T> : IValidator<T>, IEnumerable<IValidator<T>>
    {
        private readonly IImmutableList<IValidator<T>> _validators;

        public Validator(IEnumerable<IValidator<T>> validators)
        {
            _validators = validators.ToImmutableList();
        }


        public static Validator<T> Empty { get; } = new Validator<T>(ImmutableList<IValidator<T>>.Empty);

        public Validator<T> Add(IValidator<T> rule)
        {
            return new Validator<T>(_validators.Add(rule));
        }

        public IEnumerable<IValidationResult> Validate(T obj, IImmutableContainer context)
        {
            try
            {
                return ValidationResultEnumerator(obj, context);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create
                (
                    $"UnexpectedValidation",
                    $"An unexpected error occured. See the inner exception for details.",
                    inner
                );
            }
        }

        private IEnumerable<IValidationResult> ValidationResultEnumerator(T obj, IImmutableContainer context)
        {
            var results =
                from validator in this
                from result in validator.Validate(obj, context)
                select result;
            
            foreach (var result in results)
            {
                yield return result;

                if (result is ValidationError)
                {
                    yield break;
                }
            }
        }

        public IEnumerator<IValidator<T>> GetEnumerator() => _validators.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_validators).GetEnumerator();
    }


    public interface IValidatorModule
    {
        //void Build(IValidator<T, object> rules)
    }

    public abstract class Model
    {
        public static Model Is { get; } = default;
    }
}