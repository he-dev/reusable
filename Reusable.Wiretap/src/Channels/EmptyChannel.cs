using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Channels;

public class EmptyChannel : Channel
{
    public override void Invoke(ILogEntry entry) => Next?.Invoke(entry);
}