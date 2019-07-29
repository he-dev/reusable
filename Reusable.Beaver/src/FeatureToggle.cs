using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Quickey;

namespace Reusable.Beaver
{
    [PublicAPI]
    public class FeatureIdentifier : IEquatable<FeatureIdentifier>, IEquatable<string>
    {
        public FeatureIdentifier([NotNull] string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        [AutoEqualityProperty]
        public string Name { get; }

        public string Description { get; set; }

        public override string ToString() => Name;

        public override int GetHashCode() => AutoEquality<FeatureIdentifier>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => obj is FeatureIdentifier fn && Equals(fn);

        public bool Equals(FeatureIdentifier featureIdentifier) => AutoEquality<FeatureIdentifier>.Comparer.Equals(this, featureIdentifier);

        public bool Equals(string name) => Equals(this, new FeatureIdentifier(name));

        public static implicit operator FeatureIdentifier(string name) => new FeatureIdentifier(name);

        public static implicit operator FeatureIdentifier(Selector selector) => new FeatureIdentifier(selector.ToString());

        public static implicit operator string(FeatureIdentifier featureIdentifier) => featureIdentifier.ToString();
    }

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

        public FeatureOption Options
        {
            get => _options[_name];
            set => _options[_name] = value;
        }

        public FeatureSelection Update(Func<FeatureOption, FeatureOption> update)
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
        public static FeatureSelection Set(this FeatureSelection feature, FeatureOption option)
        {
            return feature.Update(o => o.SetFlag(option));
        }

        public static FeatureSelection Remove(this FeatureSelection feature, FeatureOption option)
        {
            return feature.Update(o => o.RemoveFlag(option));
        }

        public static FeatureSelection Toggle(this FeatureSelection feature, FeatureOption option)
        {
            return feature.Update(o =>
                o.Contains(option)
                    ? o.RemoveFlag(option)
                    : o.SetFlag(option));
        }
    }

    [PublicAPI]
    public interface IFeatureToggle
    {
        IFeatureOptionRepository Options { get; }

        Task<T> ExecuteAsync<T>(FeatureIdentifier name, Func<Task<T>> body, Func<Task<T>> fallback);
    }

    public class FeatureToggle : IFeatureToggle
    {
        public FeatureToggle(IFeatureOptionRepository options)
        {
            Options = options;
        }

        public IFeatureOptionRepository Options { get; }

        public async Task<T> ExecuteAsync<T>(FeatureIdentifier name, Func<Task<T>> body, Func<Task<T>> fallback)
        {
            // Not catching exceptions because the caller should handle them.

            return await (this.IsEnabled(name) ? body : fallback)().ConfigureAwait(false);
        }
    }

    public class FeatureToggler : IFeatureToggle
    {
        private readonly IFeatureToggle _featureToggle;

        public FeatureToggler(IFeatureToggle featureToggle)
        {
            _featureToggle = featureToggle;
        }

        public IFeatureOptionRepository Options => _featureToggle.Options;

        public Task<T> ExecuteAsync<T>(FeatureIdentifier name, Func<Task<T>> body, Func<Task<T>> fallback)
        {
            try
            {
                return _featureToggle.ExecuteAsync(name, body, fallback);
            }
            finally
            {
                if (Options[name].Contains(FeatureOption.Toggle))
                {
                    this.With(name, f => f.Toggle(FeatureOption.Enabled));
                    if (Options[name].Contains(FeatureOption.ToggleOnce))
                    {
                        this.With(name, f => f.Remove(FeatureOption.Toggle).Remove(FeatureOption.ToggleOnce));
                        if (Options[name].Contains(FeatureOption.ToggleReset))
                        {
                            Options.Remove(name);
                        }
                        Options.SaveChanges(name);
                    }
                }
            }
        }
    }

    public class FeatureTelemetry : IFeatureToggle
    {
        private readonly ILogger _logger;
        private readonly IFeatureToggle _featureToggle;

        public FeatureTelemetry(ILogger<FeatureTelemetry> logger, IFeatureToggle featureToggle)
        {
            _logger = logger;
            _featureToggle = featureToggle;
        }

        public IFeatureOptionRepository Options => _featureToggle.Options;

        public async Task<T> ExecuteAsync<T>(FeatureIdentifier name, Func<Task<T>> body, Func<Task<T>> fallback)
        {
            if (Options[name].Contains(FeatureOption.Telemetry))
            {
                using (_logger.BeginScope().CorrelationHandle(nameof(FeatureTelemetry)).AttachElapsed())
                {
                    _logger.Log(Abstraction.Layer.Service().Meta(new { FeatureName = name }).Trace());

                    if (_featureToggle.IsDirty(name))
                    {
                        _logger.Log(Abstraction.Layer.Service().Meta(new { CustomFeatureOptions = _featureToggle.Options[name] }));
                    }

                    return await _featureToggle.ExecuteAsync(name, body, fallback);
                }
            }
            else
            {
                return await _featureToggle.ExecuteAsync(name, body, fallback);
            }
        }
    }
}