

namespace Reusable.OmniLog.SemanticExtensions
{
    // ReSharper disable InconsistentNaming - we want the IO to be inconsistent
    public enum Layer
    {
        // Trace
        Presentation,

        // Debug
        Application,

        // Trace
        IO,
        Database,
        Network,
        External,
        
        // Information
        Business,
    }

    internal enum Category
    {
        Events,
        Properties,
        Fields,
        Variables,
        Arguments,
        Objects
    }

    //public enum Event
    //{
    //    Started,
    //    Finished,
    //    Failed,
    //}
}