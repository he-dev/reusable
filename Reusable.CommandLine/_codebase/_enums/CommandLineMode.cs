namespace RapidCommandLine
{
    public enum CommandLineMode
    {
        Undefined,
        [Factory(typeof(ExplicitCommandFactory))]
        Explicit,
        [Factory(typeof(ImplicitCommandFactory))]
        Implicit,
    }
}