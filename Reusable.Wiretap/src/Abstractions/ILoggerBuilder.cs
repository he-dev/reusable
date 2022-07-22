using JetBrains.Annotations;

namespace Reusable.Wiretap.Abstractions;

public interface ILoggerBuilder
{
    [Pure]
    [MustUseReturnValue]
    ILogger Build(string name);
}