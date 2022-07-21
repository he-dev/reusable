using System.Diagnostics.Contracts;

namespace Reusable.Wiretap.Abstractions;

public interface ILoggerPipelineBuilder
{
    [Pure]
    ILoggerNode Build(string loggerName);
}