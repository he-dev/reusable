using System.Diagnostics;

namespace Reusable.ConfigWhiz.Data
{
    public interface IEntity
    {
        IIdentifier Id { get; set; }
        object Value { get; set; }
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class Entity : IEntity
    {
        public IIdentifier Id { [DebuggerStepThrough]get; [DebuggerStepThrough]set; }

        public object Value { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }

        private string DebuggerDisplay => Id.ToString();
    }
}