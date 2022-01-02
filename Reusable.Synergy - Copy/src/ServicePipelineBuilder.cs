using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials;

namespace Reusable.Synergy;

// Builds service pipeline.
public class ServicePipelineBuilder<T> : IEnumerable<IService<T>>
{
    private IService<T> First { get; } = new Service<T>.Empty();

    // Adds the specified service at the end of the pipeline.
    public ServicePipelineBuilder<T> Add(IService<T> last) => this.Also(b => b.First.Enumerate().Last().Next = last);

    public IEnumerator<IService<T>> GetEnumerator() => First.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IService<T> Build() => First;
}