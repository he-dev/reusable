namespace Reusable.Beaver.Policies
{
    /// <summary>
    /// Indicates that a feature is always disabled.
    /// </summary>
    public class AlwaysOff : Flag
    {
        public AlwaysOff() : base(false) { }
    }
}