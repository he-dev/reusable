using System;
using Reusable.Extensions;
using Reusable.FeatureBuzz.Policies;

namespace Reusable.FeatureBuzz
{
    public interface IFeaturePolicy
    {
        FeatureState State(FeatureContext context);
    }

    public abstract class FeaturePolicy : IFeaturePolicy
    {
        public abstract FeatureState State(FeatureContext context);

        public override string ToString() => GetType().ToPrettyString();

        #region Helpers

        public static readonly IFeaturePolicy Enabled = new Enabled();

        public static readonly IFeaturePolicy Disabled = new Disabled();

        public static readonly IFeaturePolicy Once = new Once();

        public static IFeaturePolicy Ask(Func<FeatureContext, FeatureState> state) => new Lambda(state);

        public static IFeaturePolicy Ask(Func<FeatureContext, bool> state) => new Lambda(ctx => state(ctx) ? FeatureState.Enabled : FeatureState.Disabled);

        #endregion
    }

    public interface IFeaturePolicyFilter
    {
        void OnFeatureUsed(IFeatureCollection features, string name, object? parameter);
    }
}