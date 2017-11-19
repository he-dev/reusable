using System;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.Flawless
{
    // We need the T to be able to chain extensions and pass the T to them.
    // ReSharper disable once UnusedTypeParameter
    public interface IValidation<T>
    {
        bool Success { get; }

        [NotNull]
        string Expression { get; }
    }

    public abstract class Validation<T> : IValidation<T>, IEquatable<Validation<T>>
    {
        protected Validation(bool success, string expression)
        {
            Success = success;
            Expression = expression;
        }

        [AutoEqualityProperty]
        public bool Success { get; }

        [AutoEqualityProperty]
        public string Expression { get; }

        public bool Equals(Validation<T> other) => AutoEquality<Validation<T>>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as Validation<T>);

        public override int GetHashCode() => AutoEquality<Validation<T>>.Comparer.GetHashCode(this);
    }

    internal class PassedValidation<T> : Validation<T>
    {
        private PassedValidation(string rule) : base(true, rule) { }

        public static IValidation<T> Create(string rule) => new PassedValidation<T>(rule);

        public override string ToString() => $"Passed: {Expression}";
    }

    internal class FailedValidation<T> : Validation<T>
    {
        private FailedValidation(string rule) : base(false, rule) { }

        public static IValidation<T> Create(string rule) => new FailedValidation<T>(rule);

        public override string ToString() => $"Failed: {Expression}";
    }
}