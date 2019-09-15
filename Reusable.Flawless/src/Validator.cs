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
        IImmutableSet<string> Tags { get; }

        IEnumerable<IValidationResult> Validate(T obj, IImmutableContainer context);
    }

    public static class Validator
    {
        public static IValidator<T> Validate<T>(Action<IValidatorBuilder<T, T, T>> configureRules)
        {
            var builder = new ValidatorBuilder<T, T, T>(default);
            configureRules(builder);
            return new Validator<T, T, T>(x => new[] { x }, x => x, (x, ctx) => true, builder.Build());
        }
    }

    // Treat everything as collections then it's easier to handle with linq.

    [PublicAPI]
    internal class Validator<T, TSource, TElement> : List<IValidator<TElement>>, IValidator<T>
    {
        private readonly Expression<Func<T, IEnumerable<TSource>>> _sourceSelector;
        private readonly Expression<Func<TSource, TElement>> _elementSelector;
        private readonly Expression<ValidationFunc<T, bool>> _condition;

        private readonly Func<T, IEnumerable<TSource>> _selectSource;
        private readonly Func<TSource, TElement> _selectElement;
        private readonly ValidationFunc<T, bool> _when;

        public Validator
        (
            Expression<Func<T, IEnumerable<TSource>>> sourceSelector,
            Expression<Func<TSource, TElement>> elementSelector,
            Expression<ValidationFunc<T, bool>> condition,
            IEnumerable<IValidator<TElement>> validators
        ) : base(validators)
        {
            _sourceSelector = sourceSelector;
            _elementSelector = elementSelector;
            _condition = condition;

            _selectSource = sourceSelector.Compile();
            _selectElement = elementSelector?.Compile();
            _when = condition.Compile();
        }

        public IImmutableSet<string> Tags { get; }

        public static Func<IEnumerable<IValidator<TElement>>, IValidator<T>> CreateFactory
        (
            Expression<Func<T, IEnumerable<TSource>>> sourceSelector,
            Expression<Func<TSource, TElement>> elementSelector,
            Expression<ValidationFunc<T, bool>> filter
        )
        {
            return validators => new Validator<T, TSource, TElement>(sourceSelector, elementSelector, filter, validators);
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
            if (_when(obj, context))
            {
                var source = _selectSource(obj);

                var results =
                    from element in source.Select(_selectElement)
                    from validator in this
                    from result in validator.Validate(element, context)
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
            else
            {
                //yield return ValidationResultFactory.Create<ValidationInconclusive>(_validateString, Tags, _createMessage(obj, context));
                yield return ValidationResultFactory.Create<ValidationInconclusive>(default, Tags, default);
            }
        }
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