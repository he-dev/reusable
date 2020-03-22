using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// Turns objects into dictionaries and adds the result to the log.
    /// </summary>
    public class Destructure : LoggerNode
    {
        public override void Invoke(ILogEntry request)
        {
            var dictionaries =
                from property in request.Where(LogProperty.CanProcess.With(this))
                select (property, property.Value.ToDictionary());

            foreach (var (property, dictionary) in dictionaries.ToList())
            {
                request.Push(property.Name, dictionary, LogProperty.Process.With<SerializeProperty>());
            }

            InvokeNext(request);
        }
    }
}