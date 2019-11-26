using System;

namespace Reusable.Beaver
{
    public interface IFeaturePolicy
    {
        Feature Name { get; }
        bool IsEnabled(Feature feature);
    }

    public interface IFinalizable
    {
        void FinallyMain(Feature feature);
        void FinallyFallback(Feature feature);
        void FinallyIIf(Feature feature);
    }

    public class Flag : IFeaturePolicy
    {
        public Flag(string name, bool value) => (Name, Value) = (name, value);
        public Feature Name { get; }
        public bool Value { get; }
        public bool IsEnabled(Feature feature) => Value;
    }

    public class AlwaysOn : Flag
    {
        public AlwaysOn(string name) : base(name, true) { }
    }

    public class AlwaysOff : Flag
    {
        public AlwaysOff(string name) : base(name, false) { }
    }

    public class Once : IFeaturePolicy, IFinalizable
    {
        public Once(string name) => Name = name;
        public Feature Name { get; }
        public bool IsEnabled(Feature feature) => true;
        public void FinallyMain(Feature feature) { }
        public void FinallyFallback(Feature feature) { }
        public void FinallyIIf(Feature feature) => feature.Toggle?.Remove(Name);
    }

    public class Ask : IFeaturePolicy
    {
        private readonly Func<Feature, bool> _isEnabled;
        public Ask(string name, Func<Feature, bool> isEnabled) => (Name, _isEnabled) = (name, isEnabled);
        public Feature Name { get; }
        public bool IsEnabled(Feature feature) => _isEnabled(feature);
    }

    public class Lock : IFeaturePolicy, IFinalizable
    {
        private readonly IFeaturePolicy _policy;
        public Lock(IFeaturePolicy policy) => _policy = policy;
        public Feature Name => _policy.Name;
        public bool IsEnabled(Feature feature) => _policy.IsEnabled(feature);
        public void FinallyMain(Feature feature) => (_policy as IFinalizable)?.FinallyMain(feature);
        public void FinallyFallback(Feature feature) => (_policy as IFinalizable)?.FinallyFallback(feature);
        public void FinallyIIf(Feature feature) => (_policy as IFinalizable)?.FinallyIIf(feature);
    }

    public static class FeaturePolicyExtensions
    {
        public static Lock Lock(this IFeaturePolicy policy) => new Lock(policy);
    }
}