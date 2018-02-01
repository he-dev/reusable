using System.Linq;
using System.Linq.Custom;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Extensions;

namespace Reusable.OmniLog.SemanticExtensions
{
    public  interface ILogCategory { }

    public interface IData : ILogCategory { }

    public interface IAction : ILogCategory { }

    public abstract class Category
    {
        public static IData Data => default;

        public static IAction Action => default;
    }

    internal class LogCategorySnapshot
    {
        [JsonProperty(Order = 1)]
        public string Name { get; set; }

        [JsonProperty(Order = 2)]
        public object Dump { get; set; }
    }
}