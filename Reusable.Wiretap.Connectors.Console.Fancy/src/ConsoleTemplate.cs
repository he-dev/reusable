using System;
using Reusable.Fluorite;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Connectors;

public static class ConsoleTemplate
{
    public static Action<ILogEntry> Compose() => Compose("span");

    public static Action<ILogEntry> ComposeLine() => Compose("p");
    
    private static Action<ILogEntry> Compose(string elementName)
    {
        return entry => entry.Push(new ConsoleProperty.Template(HtmlElement.Create(elementName)));
    }
}