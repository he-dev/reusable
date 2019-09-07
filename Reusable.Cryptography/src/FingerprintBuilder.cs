using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;
using Reusable.Flawless;

namespace Reusable.Cryptography
{
    public delegate byte[] ComputeHashCallback(byte[] buffer);

    public delegate byte[] ComputeFingerprintCallback<in T>(T obj);

    [PublicAPI]
    public class FingerprintBuilder<T>
    {
//        private static readonly Validator<FingerprintBuilder<T>, object> FingerprintBuilderValidator =
//            Validator
//                .For<FingerprintBuilder<T>>()
//                .Accept(b => b.When(x => x._valueSelectors.Any()).Message($"{nameof(FingerprintBuilder<T>)} requires at least one value-selector."));

        private readonly SortedDictionary<string, Func<T, object>> _valueSelectors;

        public FingerprintBuilder([NotNull] IComparer<string> propertyComparer)
        {
            if (propertyComparer == null) throw new ArgumentNullException(nameof(propertyComparer));

            _valueSelectors = new SortedDictionary<string, Func<T, object>>(StringComparer.OrdinalIgnoreCase);
        }

        public FingerprintBuilder()
            : this(StringComparer.OrdinalIgnoreCase) { }

        public FingerprintBuilder<T> Add<TInput, TOutput>([NotNull] Expression<Func<T, TInput>> getMemberExpression, [NotNull] Expression<Func<TInput, TOutput>> transformValueExpression)
        {
            if (getMemberExpression == null) throw new ArgumentNullException(nameof(getMemberExpression));
            if (transformValueExpression == null) throw new ArgumentNullException(nameof(transformValueExpression));

            if (!(getMemberExpression.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException("Expression must be a member expression.");
            }

            if (_valueSelectors.ContainsKey(memberExpression.Member.Name))
            {
                throw new ArgumentException($"Member '{memberExpression.Member.Name}' has already been added.");
            }

            var getValue = getMemberExpression.Compile();
            var transform = transformValueExpression.Compile();

            _valueSelectors[memberExpression.Member.Name] = obj => transform(getValue(obj));

            return this;
        }

        public ComputeFingerprintCallback<T> Build([NotNull] ComputeHashCallback computeHashCallback)
        {
            if (computeHashCallback == null) throw new ArgumentNullException(nameof(computeHashCallback));

            //this.ValidateWith(FingerprintBuilderValidator).ThrowOnFailure();

            var binaryFormatter = new BinaryFormatter();

            return obj =>
            {
                using (var memory = new MemoryStream())
                {
                    foreach (var item in _valueSelectors)
                    {
                        var value = item.Value(obj);
                        if (!(value is null))
                        {
                            binaryFormatter.Serialize(memory, value);
                        }
                    }
                    return computeHashCallback(memory.ToArray());
                }
            };
        }
    }

    [PublicAPI]
    public class FingerprintBuilder
    {
        public static FingerprintBuilder<T> For<T>() => new FingerprintBuilder<T>();

        public static FingerprintBuilder<T> For<T>(T obj) => For<T>();
    }
}