using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Reusable.Beaver
{
    public static class FeatureCollectionExtensions
    {
        public static void Add(this IFeatureCollection features, string name, IFeaturePolicy policy, params string[] tags)
        {
            features.Add(new Feature(name, policy, tags));
        }

        public static void ForEach(this IEnumerable<Feature> features, IEnumerable<string> tags, Action<Feature> configure)
        {
            var tagSet = tags.ToImmutableHashSet(SoftString.Comparer);
            foreach (var feature in features.Where(f => tagSet.IsSubsetOf(f.Tags)))
            {
                configure(feature);
            }
        }
    }
}