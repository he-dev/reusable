using System.Diagnostics;

namespace Reusable.SmartConfig.Data
{
    public interface IEntity
    {
        CaseInsensitiveString Name { get; set; }

        object Value { get; set; }
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class Entity : IEntity
    {
        public CaseInsensitiveString Name { [DebuggerStepThrough]get; [DebuggerStepThrough]set; }

        public object Value { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }

        private string DebuggerDisplay => ToString();

        public override string ToString() => Name.ToString();

        public static implicit operator string(Entity entity) => entity.ToString();
    }
}