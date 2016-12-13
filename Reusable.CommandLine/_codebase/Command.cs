namespace Reusable.Shelly
{
    public abstract class Command
    {
        internal static Command Null { get; }

        public CommandLine CommandLine { get; internal set; }

        protected ILogger Logger => CommandLine?.Logger ?? Shelly.Logger.Empty;

        //[Parameter]
        //[Names("debug", "dbg")]
        //public bool IsDebug { get; set; }

        public abstract void Execute();
    }
}