using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using JetBrains.Annotations;
using Reusable.Essentials.Extensions;

namespace Reusable.Essentials;

public delegate byte[] ComputeHashCallback(byte[] buffer);

public delegate byte[] ComputeFingerprintCallback<in T>(T obj);

[PublicAPI]
public class FingerprintBuilder<T>
{
    // private static readonly ValidationRuleCollection<FingerprintBuilder<T>, object> FingerprintBuilderValidator =
    //     ValidationRuleCollection
    //         .For<FingerprintBuilder<T>>()
    //         .Accept(b => b.When(x => x._valueSelectors.Any()).Message($"{nameof(FingerprintBuilder<T>)} requires at least one value-selector."));

    private SortedDictionary<string, Func<T, object?>> ValueSelectors { get; } = new(SoftString.Comparer);

    private Func<byte[], byte[]> ComputeHash { get; set; } = CryptographyExtensions.ComputeSHA256;

    private JsonSerializerOptions SerializerConfiguration { get; set; } = new();

    public FingerprintBuilder<T> Add<TInput, TOutput>(Expression<Func<T, TInput>> getMemberExpression, Expression<Func<TInput, TOutput>> transformValueExpression)
    {
        if (getMemberExpression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must be a member expression.");
        }

        if (ValueSelectors.ContainsKey(memberExpression.Member.Name))
        {
            throw new ArgumentException($"Member '{memberExpression.Member.Name}' has already been added.");
        }

        var getValue = getMemberExpression.Compile();
        var transform = transformValueExpression.Compile();

        ValueSelectors[memberExpression.Member.Name] = obj => getValue(obj) is { } value ? transform(value) : default;

        return this;
    }

    public FingerprintBuilder<T> HashFunc(Func<byte[], byte[]> computeHash) => this.Also(_ => ComputeHash = computeHash);

    public FingerprintBuilder<T> SerializerOptions(JsonSerializerOptions serializerOptions) => this.Also(_ => SerializerConfiguration = serializerOptions);

    public ComputeFingerprintCallback<T> Build()
    {
        if (ValueSelectors.Any() == false) throw new InvalidOperationException("You need to specify at least one selector.");

        return obj =>
        {
            var serializable = ValueSelectors.ToDictionary(x => x.Key, x => x.Value(obj));
            using var memory = new MemoryStream();
            JsonSerializer.Serialize(memory, serializable, SerializerConfiguration);
            return ComputeHash(memory.ToArray());
        };
    }
}

[PublicAPI]
public class FingerprintBuilder
{
    public static FingerprintBuilder<T> For<T>() => new();

    public static FingerprintBuilder<T> For<T>(T obj, ComputeHashCallback computeHash) => For<T>();
}