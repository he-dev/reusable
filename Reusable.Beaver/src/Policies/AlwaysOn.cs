using Reusable.Beaver.Annotations;

namespace Reusable.Beaver.Policies
{
    /// <summary>
    /// Indicates that a feature is always enabled.
    /// </summary>
    [Beaver]
    public class AlwaysOn : Flag
    {
        public AlwaysOn() : base(true) { }
    }
}