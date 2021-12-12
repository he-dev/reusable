using System.Collections;
using System.Collections.Generic;

namespace Reusable.Wiretap.Abstractions;

public abstract class LoggerPipeline : IEnumerable<ILoggerNode>
{
    public abstract IEnumerator<ILoggerNode> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}