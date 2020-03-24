using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Services;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This nodes serializes properties that are targeted for this node.
    /// </summary>
    public class SerializeProperty : LoggerNode
    {
        public ISerialize Serialize { get; set; } = new SerializeToJson();

        public override void Invoke(ILogEntry request)
        {
            foreach (var property in request.Where(LogProperty.CanProcess.With(this)).ToList())
            {
                var obj = Serialize.Invoke(property.Value);
                request.Push(property.Name, obj, m => m.ProcessWith<Echo>());
            }

            InvokeNext(request);
        }
    }
}