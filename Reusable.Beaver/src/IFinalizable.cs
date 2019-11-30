using JetBrains.Annotations;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFinalizable
    {
        void FinallyMain(Feature feature);
        void FinallyFallback(Feature feature);
        void FinallyIIf(Feature feature);
    }
}