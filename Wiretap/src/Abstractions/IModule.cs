using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Abstractions;

public interface IModule
{
    void Invoke(TraceContext context, LogAction next);
}

