using JetBrains.Annotations;
using Reusable.Beaver.Policies;
using Reusable.Extensions;

namespace Reusable.Beaver
{
    [PublicAPI]
    public static class FeatureToggleHelpers
    {
        public static FeatureState State(this IFeatureToggle toggle, string name, object? parameter = default) => toggle[name].Map(f => f.Policy.State(new FeatureContext(toggle, f, parameter)));

        public static bool IsEnabled(this IFeatureToggle toggle, string name, object? parameter = default) => toggle[name].Map(f => f.Policy.IsEnabled(new FeatureContext(toggle, f, parameter)));

        public static bool IsLocked(this IFeatureToggle toggle, string name) => toggle[name].Policy is Lock;

        public static IFeatureToggle Enable(this IFeatureToggle toggle, string name) => toggle.Pipe(t => t[name].Policy = FeaturePolicy.AlwaysOn);

        public static IFeatureToggle Disable(this IFeatureToggle toggle, string name) => toggle.Pipe(t => t[name].Policy = FeaturePolicy.AlwaysOff);

        public static IFeatureToggle Lock(this IFeatureToggle toggle, string name) => toggle.Pipe(t => t[name].Policy = t[name].Policy.Lock());

        public static IFeatureToggle Telemetry(this IFeatureToggle toggle, string name, IFeaturePolicy policy)
        {
            return toggle.Pipe(t => t.Add(new Feature.Telemetry(name, policy)));
        }
    }
}