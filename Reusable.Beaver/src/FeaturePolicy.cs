using System;
using Reusable.Beaver.Policies;

namespace Reusable.Beaver
{
    public interface IFeaturePolicy
    {
        FeatureState State(FeatureContext context);
    }

    public static class FeaturePolicy
    {
        public static readonly IFeaturePolicy AlwaysOn = new AlwaysOn();

        public static readonly IFeaturePolicy AlwaysOff = new AlwaysOff();

        public static readonly IFeaturePolicy Once = new Once();

        public static IFeaturePolicy Ask(Func<FeatureContext, FeatureState> state) => new Ask(state);

        public static IFeaturePolicy Ask(Func<FeatureContext, bool> state) => new Ask(ctx => state(ctx) ? FeatureState.Enabled : FeatureState.Disabled);

        public static readonly IFeaturePolicy Remove = new Remove();

        public static readonly string Fallback = nameof(Fallback);
    }

    public interface IFinalizable
    {
        void Finally(FeatureContext context, FeatureState after);
    }
}