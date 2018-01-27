using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    // ReSharper disable once UnusedTypeParameter - We need the T to be able to chain extensions and pass the T to them.
    public interface IValidationResult<T>
    {
        bool Success { get; }

        [NotNull]
        string Expression { get; }

        [CanBeNull]
        string Message { get; }
    }

    internal class ValidationResult<T> : IValidationResult<T>
    {
        // ReSharper disable once StaticMemberInGenericType - this is ok because it's common to all instances.
        private static readonly IDictionary<bool, string> ResultStrings = new Dictionary<bool, string>
        {
            [true] = "Passed",
            [false] = "Failed"
        };

        public ValidationResult(bool success, [NotNull] string expression, [CanBeNull] string message)
        {
            Success = success;
            Expression = expression;
            Message = message;
        }

        //[AutoEqualityProperty]
        public bool Success { get; }

        //[AutoEqualityProperty]
        public string Expression { get; }

        public string Message { get; }        

        public override string ToString()
        {
            return
                Success
                    ? $"{ResultStrings[Success]}: {Expression}"
                    : $"{ResultStrings[Success]}: {Expression} {Message.EncloseWith("()")}";
        }
    }

    //internal abstract partial class ValidationResult<T> : IEquatable<IValidationResult<T>>
    //{
    //    public bool Equals(IValidationResult<T> other) => AutoEquality<IValidationResult<T>>.Comparer.Equals(this, other);

    //    public override bool Equals(object obj) => Equals(obj as IValidationResult<T>);

    //    public override int GetHashCode() => AutoEquality<IValidationResult<T>>.Comparer.GetHashCode(this);

    //    public override string ToString() => $"Passed: {Expression}";
    //}

    //internal class PassedValidationResult<T> : ValidationResult<T>
    //{
    //    private PassedValidationResult(string rule) : base(true, rule) { }

    //    public static IValidationResult<T> Create(string rule) => new PassedValidationResult<T>(rule);

    //    public override string ToString() => $"Passed: {Expression}";
    //}

    //internal class FailedValidationResult<T> : ValidationResult<T>
    //{
    //    private FailedValidationResult(string rule) : base(false, rule) { }

    //    public static IValidationResult<T> Create(string rule) => new FailedValidationResult<T>(rule);

    //    public override string ToString() => $"Failed: {Expression}";
    //}
}