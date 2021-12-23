using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Jumble.Policies;

[PublicAPI]
public class Dependent : FeaturePolicy, IEnumerable<IFeatureIdentifier>
{
    public Dependent(params IFeatureIdentifier[] ids) => Ids = ids.ToList();

    public List<IFeatureIdentifier> Ids { get; set; }

    public override FeatureState GetState(FeatureStateContext context)
    {
        return Ids.All(id => context.Features.IsEnabled(id, context.Parameter)) ? FeatureState.Enabled : FeatureState.Disabled;
    }

    public void Add(IFeatureIdentifier id) => Ids.Add(id);

    public IEnumerator<IFeatureIdentifier> GetEnumerator() => Ids.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}