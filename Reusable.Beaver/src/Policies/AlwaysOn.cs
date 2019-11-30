namespace Reusable.Beaver.Policies
{
    public class AlwaysOn : Flag
    {
        public AlwaysOn(string name) : base(name, true) { }
    }
}