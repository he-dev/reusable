using System;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Services.Properties;

public class Lambda : PropertyService
{
    public Lambda(Action<ILogEntry> propertyAction) => PropertyAction = propertyAction;

    private Action<ILogEntry> PropertyAction { get; }

    public override void Invoke(ILogEntry entry) => PropertyAction(entry);
}