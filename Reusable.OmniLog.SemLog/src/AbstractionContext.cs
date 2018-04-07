using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.OmniLog.SemanticExtensions
{
    public interface IAbstractionContext
    {
        LogLevel LogLevel { get; }

        string LayerName { get; }

        string CategoryName { get; }

        object Dump { get; }
    }

    public class AbstractionContext : IAbstractionContext
    {
        public AbstractionContext(IAbstractionLayer layer, object dump, [CallerMemberName] string categoryName = null)
        {
            (LayerName, LogLevel) = layer;
            CategoryName = categoryName;
            Dump = dump;
        }

        public LogLevel LogLevel { get; }

        public string LayerName { get; }

        public string CategoryName { get; }

        public object Dump { get; }
    }
}
