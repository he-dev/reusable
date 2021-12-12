using System.Collections.Generic;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Nodes;

namespace Reusable.Wiretap.Pipelines;

public class MinimalPipeline : LoggerPipeline
{
    public override IEnumerator<ILoggerNode> GetEnumerator()
    {
        yield return new InvokeEntryAction();
        yield return new GuessProperties();
        yield return new Echo();
    }
}