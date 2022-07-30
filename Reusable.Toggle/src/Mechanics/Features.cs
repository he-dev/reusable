using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Toggle.Mechanics;

public abstract class FeatureMechanics
{
    public abstract bool CanUse(IFeatureService features);

    protected virtual void Finally() { }

    public FeatureScope BeginUnitOfWork(IFeatureService features)
    {
        return
            CanUse(features)
                ? new FeatureScope(Finally)
                : throw new InvalidOperationException("You cannot use this feature.");
    }
}

public abstract class ConstantFeature : FeatureMechanics
{
    private readonly bool _canUse;

    protected ConstantFeature(bool canUse) => _canUse = canUse;

    public override bool CanUse(IFeatureService features) => _canUse;
}

public class AlwaysOn : ConstantFeature
{
    public AlwaysOn() : base(true) { }
}

public class AlwaysOff : ConstantFeature
{
    public AlwaysOff() : base(false) { }
}

public class DependentFeature : FeatureMechanics
{
    public DependentFeature(IEnumerable<string> dependencies) => Dependencies = dependencies;

    private IEnumerable<string> Dependencies { get; }

    public override bool CanUse(IFeatureService features) => Dependencies.All(n => features[n].CanUse(features));
}

public class CompositeFeature : FeatureMechanics
{
    public CompositeFeature(IEnumerable<FeatureMechanics> features) => Features = features;

    private IEnumerable<FeatureMechanics> Features { get; }

    public override bool CanUse(IFeatureService features) => Features.All(f => f.CanUse(features));
}

public class CountdownFeature : FeatureMechanics
{
    public CountdownFeature(int count) => Count = count;

    public int Count { get; private set; }

    public override bool CanUse(IFeatureService features) => Count > 0;

    protected override void Finally()
    {
        if (Count > 0)
        {
            Count--;
        }
    }
}