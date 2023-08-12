using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules;

public class AttachDetails : IModule
{
    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        if (activity.Items.GetItem(LogEntry.PropertyNames.Details) is Item item)
        {
            entry = entry.SetItem(LogEntry.PropertyNames.Details, entry.Details().SetItem(item.Name, item.Value));
        }

        next(activity, entry);
    }

    public record Item(string Name, object Value);
}