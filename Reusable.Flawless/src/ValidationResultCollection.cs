using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Reusable.Flawless
{
    public class ValidationResultCollection<T> : IEnumerable<IValidationResult>
    {
        private readonly IImmutableList<IValidationResult> _results;

        public ValidationResultCollection(T value, IEnumerable<IValidationResult> results)
        {
            Value = value;
            _results = results.ToImmutableList();
        }

        public T Value { get; }

        public IEnumerator<IValidationResult> GetEnumerator() => _results.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator T(ValidationResultCollection<T> results) => results.Value;
    }
}