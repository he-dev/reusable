using System;
using JetBrains.Annotations;
using Reusable.Data;

namespace Reusable.Beaver
{
    // Provides an API that does not require the name
    [PublicAPI]
    public class FeatureSelection
    {
        private readonly IFeatureOptionRepository _options;
        private readonly FeatureIdentifier _name;

        public FeatureSelection(IFeatureOptionRepository options, FeatureIdentifier name)
        {
            _options = options;
            _name = name;
        }

        public Option<Feature> Options
        {
            get => _options[_name];
            set => _options[_name] = value;
        }

        public FeatureSelection Update(Func<Option<Feature>, Option<Feature>> update)
        {
            _options.Update(_name, update);
            return this;
        }

        public FeatureSelection SaveChanges()
        {
            _options.SaveChanges(_name);
            return this;
        }
    }

    public static class FeatureSelectionExtensions
    {
        public static FeatureSelection Set(this FeatureSelection feature, Option<Feature> option)
        {
            return feature.Update(o => o.SetFlag(option));
        }

        public static FeatureSelection Remove(this FeatureSelection feature, Option<Feature> option)
        {
            return feature.Update(o => o.RemoveFlag(option));
        }

        public static FeatureSelection Toggle(this FeatureSelection feature, Option<Feature> option)
        {
            return feature.Update(o =>
                o.Contains(option)
                    ? o.RemoveFlag(option)
                    : o.SetFlag(option));
        }
    }
}