using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Reusable.OmniLog.SemanticExtensions
{
    public class LogTransactionMerge : ILogScopeMerge
    {
        private readonly IStateSerializer _serializer;

        /// <summary>
        /// Creates a new LogTransactionMerge attachement. Uses JsonStateSerializer by default.
        /// </summary>
        public LogTransactionMerge(IStateSerializer serializer = null)
        {
            _serializer = serializer ?? new JsonStateSerializer();
        }

        public SoftString Name => "Transaction";

        [NotNull]
        public JsonSerializerSettings Settings { get; set; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter() }
        };

        public KeyValuePair<SoftString, object> Merge(IEnumerable<KeyValuePair<SoftString, object>> items)
        {
            var json = _serializer.SerializeObject(items.Select(item => item.Value).Reverse());
            return new KeyValuePair<SoftString, object>(Name, json);
        }
    }
}