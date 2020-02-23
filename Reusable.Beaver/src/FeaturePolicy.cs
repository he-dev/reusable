using System;
using Reusable.Beaver.Policies;
using Reusable.Extensions;

namespace Reusable.Beaver
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

        public static readonly IFeaturePolicy AlwaysOn = new AlwaysOn();

        public static readonly IFeaturePolicy AlwaysOff = new AlwaysOff();

        public static readonly IFeaturePolicy Once = new Once();

        public static IFeaturePolicy Ask(Func<FeatureContext, FeatureState> state) => new Lambda(state);

        public static IFeaturePolicy Ask(Func<FeatureContext, bool> state) => new Lambda(ctx => state(ctx) ? FeatureState.Enabled : FeatureState.Disabled);

        #endregion
    }

    public interface IFinalizable
    {
        void Finalize(FeatureContext context, FeatureState state);
    }
}