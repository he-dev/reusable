using System.Collections.Generic;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap.Services.Properties;

namespace Reusable.Wiretap.Pipelines;

public class CommonPipeline : LoggerPipeline
{
    public override IEnumerator<ILoggerNode> GetEnumerator()
    {
        yield return new InvokePropertyService
        {
            Services = { new Timestamp<DateTimeUtc>() }
        };
        yield return new InvokeEntryAction();
        yield return new GuessProperties();
        yield return new FilterEntries();
        yield return new Echo();
    }
}