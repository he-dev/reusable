using System.Collections;
using System.Collections.Generic;

namespace Reusable.Wiretap.Abstractions;

public abstract class LoggerPipeline : IEnumerable<ILoggerNode>
{
    public abstract IEnumerator<ILoggerNode> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class Empty : LoggerPipeline
    {
        private Empty() { }

        public override IEnumerator<ILoggerNode> GetEnumerator()
        {
            yield return LoggerNode.Empty.Instance;
        }

        public static readonly LoggerPipeline Instance = new Empty();
    }
}