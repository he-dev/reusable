using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials;
using Reusable.Octopus.Abstractions;

namespace Reusable.Octopus;

public interface IControllerFilter
{
    bool Matches(IResourceController controller);
}

public class ControllerFilterByName : IControllerFilter
{
    public ControllerFilterByName(string name) => Name = name;

    private string Name { get; }

    public bool Matches(IResourceController controller) => SoftString.Comparer.Equals(Name, controller.Name);
}

public class ControllerFilterByTag : IControllerFilter
{
    public ControllerFilterByTag(params string[] tags) => Tags = tags.ToHashSet(SoftString.Comparer);

    private ISet<string> Tags { get; }
    
    public bool Matches(IResourceController controller) => Tags.Overlaps(controller.Tags);
}