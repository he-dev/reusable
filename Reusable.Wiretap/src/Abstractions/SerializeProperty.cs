using JetBrains.Annotations;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Abstractions;

/// <summary>
/// This nodes takes care of serializable properties.
/// </summary>
[PublicAPI]
public abstract class SerializeProperty : LoggerMiddleware
{
    protected SerializeProperty(string propertyName) => PropertyName = propertyName;

    public override string Name => $"{GetType().ToPrettyString()}.{PropertyName}";

    private string PropertyName { get; set; }

    public override void Invoke(ILogEntry entry)
    {
        if (entry.TryPeek(PropertyName, out var property) && property is ILogProperty<ITransientProperty>)
        {
            entry.Push<IRegularProperty>(property.Name, Serialize(property.Value));
        }

        Next?.Invoke(entry);
    }

    protected abstract object Serialize(object value);
}