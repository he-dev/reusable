using System.Collections;
using System.Collections.Generic;

namespace Reusable.Jumble;

public interface IFeatureAttributeSet
{
    // Contains some elements.
    bool OverlapsWith(IFeatureAttributeSet other);

    // Contains all elements.
    bool SupersetOf(IFeatureAttributeSet other);
}

public class FeatureTagSet<T> : IFeatureAttributeSet, IEnumerable<T>
{
    private HashSet<T> Values { get; set; } = new();

    public bool OverlapsWith(IFeatureAttributeSet other) => other is FeatureTagSet<T> tags && Values.Overlaps(tags.Values);

    public bool SupersetOf(IFeatureAttributeSet other) => other is FeatureTagSet<T> tags && Values.IsSupersetOf(tags.Values);
    
    public void Add(T tag) => Values.Add(tag);
    
    public IEnumerator<T> GetEnumerator() => Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class EmptyFeatureAttributeSet : IFeatureAttributeSet
{
    public bool OverlapsWith(IFeatureAttributeSet other) => false;

    public bool SupersetOf(IFeatureAttributeSet other) => false;
}