

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
}