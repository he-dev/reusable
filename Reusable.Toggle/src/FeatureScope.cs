using System;

namespace Reusable.Toggle.Mechanics;

public class FeatureScope : IDisposable
{
    public FeatureScope(Action @finally) => Finally = @finally;

    private Action Finally { get; }

    public void Dispose() => Finally();
}