using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Flawless
{
//    [PublicAPI]
//    public static class Validator
//    {
//        public static Validator<T, TContext> For<T, TContext>()
//        {
//            return Validator<T, TContext>.Empty;
//        }
//
//        public static Validator<T, object> For<T>()
//        {
//            return For<T, object>();
//        }
//    }

    public interface IValidator<T> 
    {
        ValidationResultCollection<T> Validate(T obj, IImmutableContainer context);
    }

    [PublicAPI]
    public class Validator<T> : IValidator<T>, IEnumerable<IValidationRule<T>>
    {
        private readonly IImmutableList<IValidationRule<T>> _rules;

        public Validator(IImmutableList<IValidationRule<T>> rules)
        {
            _rules = rules;
        }

        public static Validator<T> Empty { get; } = new Validator<T>(ImmutableList<IValidationRule<T>>.Empty);

        public Validator<T> Add(IValidationRule<T> rule)
        {
            return new Validator<T>(_rules.Add(rule));
        }

        public ValidationResultCollection<T> Validate(T obj, IImmutableContainer context)
        {
            try
            {
                return new ValidationResultCollection<T>(obj, Evaluate(this, obj, context).ToImmutableList());
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

        private static IEnumerable<IValidationResult> Evaluate(Validator<T> rules, T obj, IImmutableContainer context)
        {
            foreach (var result in rules.Select(r => r.Evaluate(obj, context)))
            {
                yield return result;

                if (result is ValidationError)
                {
                    yield break;
                }
            }
        }

        public IEnumerator<IValidationRule<T>> GetEnumerator() => _rules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_rules).GetEnumerator();
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