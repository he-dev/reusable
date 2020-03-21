using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Utilities;

namespace Reusable.OmniLog.Nodes
{
    // when #Dump is Dictionary --> call Next() for each pair where Key: Identifier and Value: #Serializable
    // when #Dump is object --> call Next() for each property and its value where PropertyName: Identifier and Value: #Serializable
    // when #Dump is string --> call Next() once where Key.Name: Identifier and Value: #Dump as #Serializable
    /// <summary>
    /// Breaks a compound object into its component objects and create a log-entry for each one.
    /// </summary>
    public class DestructureNode : LoggerNode
    {
        public override void Invoke(ILogEntry request)
        {
            var dictionaries =
                from property in request.Where(LogProperty.CanProcess.With(this))
                select (property, property.Value.ToDictionary());

            var any = false;
            foreach (var (property, dictionary) in dictionaries.ToList())
            {
                //var copy = request.Copy();

                //copy.Add(LogProperty.Names.SnapshotName, name, LogProperty.Process.With<EchoNode>());
                //copy.Add(LogProperty.Names.Snapshot, value, LogProperty.Process.With<SerializerNode>());
                //
                request.Add(property.Name, dictionary, LogProperty.Process.With<SerializerNode>());

                //InvokeNext(copy);

                any = true;
            }

            // There wasn't anything to explode so just invoke the next node. 
            if (!any)
            {
                //InvokeNext(request);
            }

            InvokeNext(request);
        }
    }
}