using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Flawless.ExpressionVisitors;

namespace Reusable.Flawless
{
    internal class ValidationRule<T> : IValidator<T>
    {
        private readonly ValidationFunc<T, bool> _validate;
        private readonly ValidationFunc<T, string> _createMessage;
        //private readonly string _whenString;
        private readonly string _validateString;
        private readonly bool _required;

        public ValidationRule
        (
            Expression<ValidationFunc<T, bool>> validate,
            Expression<ValidationFunc<T, string>> message,
            bool required,
            IImmutableSet<string> tags
        )
        {
//            if (tags == null) throw new ArgumentNullException(nameof(tags));
//            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
//            if (message == null) throw new ArgumentNullException(nameof(message));
//            if (createValidationFailure == null) throw new ArgumentNullException(nameof(createValidationFailure));

            Tags = tags;
            _validate = validate.Compile();
            _createMessage = message.Compile();
            //_whenString = ValidationParameterPrettifier.Prettify<T>(when).ToString();
            _validateString = ValidationParameterPrettifier.Prettify<T>(validate).ToString();
            //_selector = selector;
            //_select = selector.Compile();
            _required = required;
        }

        public IImmutableSet<string> Tags { get; }

        public IEnumerable<IValidationResult> Validate(T obj, IImmutableContainer context)
        {
            if (_validate(obj, context))
            {
                yield return ValidationResultFactory.Create<ValidationSuccess>(_validateString, Tags, _createMessage(obj, context));
            }
            else
            {
                if (_required)
                {
                    yield return ValidationResultFactory.Create<ValidationError>(_validateString, Tags, _createMessage(obj, context));
                }
                else
                {
                    yield return ValidationResultFactory.Create<ValidationWarning>(_validateString, Tags, _createMessage(obj, context));
                }
            }
        }

        //public override string ToString() => $"When: {_whenString}{Environment.NewLine}Validate: {_validateString}";
        public override string ToString() => $"Validate: {_validateString}";

        public static implicit operator string(ValidationRule<T> rule) => rule?.ToString();
    }
}