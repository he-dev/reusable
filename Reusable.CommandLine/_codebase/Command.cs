namespace Reusable.Shelly
{
    public abstract class Command
    {
        //[Parameter]        
        //public bool Test { get; set; }

        public abstract void Execute();
    }
}