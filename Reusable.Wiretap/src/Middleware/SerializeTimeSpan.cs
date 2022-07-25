using System;
using JetBrains.Annotations;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Middleware;

[PublicAPI]
public class SerializeTimeSpan : SerializeProperty
{
    public SerializeTimeSpan(string propertyName) : base(propertyName) { }

    public Func<TimeSpan, object> GetValue { get; set; } = timeSpan => Math.Round(timeSpan.TotalMilliseconds, 3);

    protected override object Serialize(object value)
    {
        return
            value is TimeSpan timeSpan
                ? GetValue(timeSpan)
                : throw new ArgumentException($"Value must be a {nameof(TimeSpan)}, but is {value.GetType().ToPrettyString()}");
    }
}