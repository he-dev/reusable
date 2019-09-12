using System;
using System.Collections.Generic;
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
            Message = message;
            Name = Regex.Replace(GetType().Name, "^Validation", string.Empty);
            Tags = tags.Prepend(Name).Select(t => t.ToLower()).ToList();
        }

        public string Expression { get; }

        public IEnumerable<string> Tags { get; }

        public string Message { get; }

        private string Name { get; }

        public override string ToString() => $"[{Tags.Join(", ")}] {Expression}".ConcatIfNotEmpty(Message, separator: " | ");

        public static implicit operator bool(ValidationResult result) => result is ValidationSuccess;
    }

    public static class ValidationResultFactory
    {
        public static IValidationResult Create<T>(string expression, IEnumerable<string> tags, string message) where T : IValidationResult
        {
            return (T)Activator.CreateInstance(typeof(T), expression, tags, message);
        }
    }

    [UsedImplicitly]
    public class ValidationSuccess : ValidationResult
    {
        public ValidationSuccess(string expression, IEnumerable<string> tags, string message) : base(expression, tags, message) { }
    }

    [UsedImplicitly]
    public class ValidationInconclusive : ValidationResult
    {
        public ValidationInconclusive(string expression, IEnumerable<string> tags, string message) : base(expression, tags, message) { }
    }

    [UsedImplicitly]
    public class ValidationWarning : ValidationResult, IValidationFailure
    {
        public ValidationWarning(string expression, IEnumerable<string> tags, string message) : base(expression, tags, message) { }
    }

    [UsedImplicitly]
    public class ValidationError : ValidationResult, IValidationFailure
    {
        public ValidationError(string expression, IEnumerable<string> tags, string message) : base(expression, tags, message) { }
    }
}