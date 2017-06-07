using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Logging
{
    public interface IComputedProperty
    {
        string Name { get; }

        object Compute(LogEntry log);
    }

    public abstract class ComputedProperty : IComputedProperty
    {
        protected ComputedProperty(string name) => Name = name;

        protected ComputedProperty() => Name = GetType().Name;

        public string Name { get; }

        public abstract object Compute(LogEntry log);
    }
}
