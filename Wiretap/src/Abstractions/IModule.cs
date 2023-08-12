using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Abstractions;

public interface IModule
{
    void Invoke(IActivity activity, LogEntry entry, LogFunc next);
}

