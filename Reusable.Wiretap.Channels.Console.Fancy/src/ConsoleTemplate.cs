using System;
using Reusable.Fluorite;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Channels;

public static class ConsoleTemplate
{
    public static Action<ILogEntry> Compose() => Compose("span");

    public static Action<ILogEntry> ComposeLine() => Compose("p");
    
    private static Action<ILogEntry> Compose(string elementName)
    {
        return entry => entry.Push<ITransientProperty>(LogProperty.Names.Template(), HtmlElement.Create(elementName));
    }
}