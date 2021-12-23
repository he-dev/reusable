using Reusable.Essentials.Extensions;

namespace Reusable.Jumble;

public interface IFeaturePolicy
{
    FeatureState GetState(FeatureStateContext context);
}

public abstract class FeaturePolicy : IFeaturePolicy
{
    public abstract FeatureState GetState(FeatureStateContext context);

    public override string ToString() => GetType().ToPrettyString();

    #region Helpers

    public static readonly IFeaturePolicy AlwaysEnabled = new Constant(FeatureState.Enabled);

    public static readonly IFeaturePolicy AlwaysDisabled = new Constant(FeatureState.Disabled);

    #endregion

    private class Constant : FeaturePolicy
    {
        public Constant(FeatureState state) => State = state;

        private FeatureState State { get; }

        public override FeatureState GetState(FeatureStateContext context) => State;

        public override string ToString() => $"{base.ToString()} | {State}";
    }
}