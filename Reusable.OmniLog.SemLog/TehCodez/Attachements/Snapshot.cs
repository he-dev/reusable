using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.SemanticExtensions.Attachements
{
    public class Snapshot : LogAttachement
    {
        private readonly ISnapshotSerializer _serializer;

        public Snapshot(ISnapshotSerializer serializer) : base(nameof(Snapshot))
        {
            _serializer = serializer;
        }

        public override object Compute(Log log)
        {
            if (log.TryGetValue(nameof(LogBag), out var bag) && bag is LogBag b && b.TryGetValue(Name.ToString(), out var obj))
            {
                return _serializer.SerializeObject(obj);
            }
            return null;
        }
    }

    [PublicAPI]
    public interface ISnapshotSerializer
    {
        object SerializeObject(object obj);
    }

    public class JsonSnapshotSerializer : ISnapshotSerializer
    {
        public JsonSnapshotSerializer()
        {
            Settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented,
                Converters = { new StringEnumConverter() }
            };
        }

        [NotNull]
        public JsonSerializerSettings Settings { get; set; }

        public object SerializeObject(object obj)
        {
            return obj is string ? obj : JsonConvert.SerializeObject(obj, Settings);
        }
    }
}