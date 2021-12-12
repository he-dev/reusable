using System.Collections.Generic;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Pipelines;

public class EmptyPipeline : LoggerPipeline
{
    public override IEnumerator<ILoggerNode> GetEnumerator()
    {
        yield break;
    }
}