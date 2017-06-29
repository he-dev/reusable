using System.Diagnostics;
using Reusable.ConfigWhiz.Paths;

namespace Reusable.ConfigWhiz.Data
{
    public interface IEntity
    {
        Identifier Identifier { get; set; }
        object Value { get; set; }
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class Entity : IEntity
    {
        public Identifier Identifier { [DebuggerStepThrough]get; [DebuggerStepThrough]set; }

        public object Value { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }

        private string DebuggerDisplay => Identifier.ToString($".{IdentifierLength.Unique}", IdentifierFormatter.Instance);
    }
}