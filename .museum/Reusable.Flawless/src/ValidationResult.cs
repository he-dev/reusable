﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public interface IValidationResult
    {
        [NotNull]
        string Expression { get; }

        [NotNull, ItemNotNull]
        IEnumerable<string> Tags { get; }

        [CanBeNull]
        string Message { get; }
    }

    public interface IValidationFailure : IValidationResult { }

    public abstract class ValidationResult : IValidationResult
    {
        protected ValidationResult(string expression, IEnumerable<string> tags, string message)
        {
            Expression = expression;
            Tags = tags.Prepend(Name).Select(t => $"#{t.Trim('#').ToLower()}");
            Message = message;
        }

        public string Expression { get; }

        public IEnumerable<string> Tags { get; }

        public string Message { get; }

        private string Name => Regex.Replace(GetType().Name, "^Validation", string.Empty);

        public override string ToString() => $"[{Tags.Join(" ")}] {Expression}".ConcatIfNotEmpty(Message, separator: " | ");

        public static implicit operator bool(ValidationResult result) => result is ValidationSuccess;
    }

    public class ValidationSuccess : ValidationResult
    {
        public ValidationSuccess(string expression, IEnumerable<string> tags, string message) : base(expression, tags, message) { }
    }

    public class ValidationWarning : ValidationResult, IValidationFailure
    {
        public ValidationWarning(string expression, IEnumerable<string> tags, string message) : base(expression, tags, message) { }
    }

    public class ValidationError : ValidationResult, IValidationFailure
    {
        public ValidationError(string expression, IEnumerable<string> tags, string message) : base(expression, tags, message) { }
    }

    public delegate IValidationFailure CreateValidationFailureCallback(string expression, IEnumerable<string> tags, string message);

    public static class ValidationFailureFactory
    {
        public static IValidationFailure CreateWarning(string expression, IEnumerable<string> tags, string message) => new ValidationWarning(expression, tags, message);

        public static IValidationFailure CreateError(string expression, IEnumerable<string> tags, string message) => new ValidationError(expression, tags, message);
    }
}