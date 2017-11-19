// ReSharper disable InconsistentNaming
namespace Reusable.OmniLog.SemLog
{
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