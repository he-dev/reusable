using System.Threading;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    [UseType, UseMember]
    [TrimStart("I"), TrimEnd("Properties")]
    [PlainSelectorFormatter]
    public interface IRequestProperties
    {
        CancellationToken CancellationToken { get; }
    }
}