using Reusable.Wiretap.Abstractions;

namespace Reusable.Toggle;

public class Feature
{
    public Feature(IFeatureIdentifier id, IFeaturePolicy policy) => (Id, Policy) = (id, policy);

    public IFeatureIdentifier Id { get; }

    public IFeatureAttributeSet Attributes { get; set; } = new EmptyFeatureAttributeSet();

    public IFeaturePolicy Policy { get; internal set; }

    public override string ToString() => $"{Id} | {Policy}";

    #region Factories for constant features.

    public static Feature AlwaysEnabled(IFeatureIdentifier id) => new(id, FeaturePolicy.AlwaysEnabled);

    public static Feature AlwaysDisabled(IFeatureIdentifier id) => new(id, FeaturePolicy.AlwaysDisabled);

    #endregion

    // Meta-Feature used to control telemetry.
    public class Telemetry : Feature
    {
        public static IFeatureIdentifier DefaultId => FeatureName.From($"{nameof(Feature)}.{nameof(Telemetry)}");

        public Telemetry(ILogger<Telemetry> logger) : base(DefaultId, FeaturePolicy.AlwaysEnabled) => Logger = logger;

        public ILogger<Telemetry> Logger { get; }
    }
}