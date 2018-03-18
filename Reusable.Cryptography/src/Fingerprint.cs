using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;

namespace Reusable.Cryptography
{
    public class FingerprintBuilder<T>
    {
        private readonly Func<byte[], byte[]> _computeHash;

        private readonly SortedDictionary<string, Func<T, object>> _fingerprints;

        public FingerprintBuilder(Func<byte[], byte[]> computeHash)
        {
            _computeHash = computeHash ?? throw new ArgumentNullException(nameof(computeHash));
            _fingerprints = new SortedDictionary<string, Func<T, object>>(StringComparer.OrdinalIgnoreCase);
        }

        public static FingerprintBuilder<T> Create(Func<byte[], byte[]> computeHash)
        {
            return new FingerprintBuilder<T>(computeHash);
        }

        public FingerprintBuilder<T> For<TProperty>(Expression<Func<T, TProperty>> expression, Expression<Func<TProperty, TProperty>> fingerprint)
        {
            if (!(expression.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException("Expression must be a member expression");
            }

            if (_fingerprints.ContainsKey(memberExpression.Member.Name))
            {
                throw new ArgumentException($"Member {memberExpression.Member.Name} has already been added.");
            }

            var getValue = expression.Compile();
            var getFingerprint = fingerprint.Compile();

            _fingerprints[memberExpression.Member.Name] = obj =>
            {
                var value = getValue(obj);
                return value == null ? default : getFingerprint(getValue(obj));
            };

            return this;
        }

        public Func<T, byte[]> Build()
        {
            var binaryFormatter = new BinaryFormatter();

            return obj =>
            {
                using (var memory = new MemoryStream())
                {
                    foreach (var item in _fingerprints)
                    {
                        var value = item.Value(obj);
                        if (!(value is null))
                        {
                            binaryFormatter.Serialize(memory, value);
                        }
                    }
                    return _computeHash(memory.ToArray());
                }
            };
        }
    }

    public class FingerprintBuilder
    {
        public static FingerprintBuilder<T> Create<T>(Func<byte[], byte[]> computeHash, T obj)
        {
            return new FingerprintBuilder<T>(computeHash);
        }
    }

    public static class FingerprintBuilderExtensions
    {
        public static FingerprintBuilder<T> For<T, TProperty>(this FingerprintBuilder<T> builder, Expression<Func<T, TProperty>> expression)
        {
            return builder.For(expression, _ => _);
        }

        public static FingerprintBuilder<T> For<T>(this FingerprintBuilder<T> builder, Expression<Func<T, string>> expression, bool ignoreCase, bool ignoreWhiteSpace)
        {
            var format = (Func<string, string>)(input =>
            {
                if (ignoreCase)
                {
                    input = input.ToUpperInvariant();
                }

                if (ignoreWhiteSpace)
                {
                    input = input.Trim();
                }

                return input;
            });

            return builder.For(expression, input => format(input));
        }
    }

}
