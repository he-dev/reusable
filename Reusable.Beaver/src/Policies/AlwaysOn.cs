namespace Reusable.Beaver.Policies
{
    /// <summary>
    /// Indicates that a feature is always enabled.
    /// </summary>
    public class AlwaysOn : Flag
    {
        public AlwaysOn() : base(true) { }
    }
}