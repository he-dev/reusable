using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Beaver
{
    public interface IFeatureOptionRepository
    {
        [NotNull]
        FeatureOption this[string name] { get; set; }

        bool IsDirty(string name);

        void SaveAsDefaults();
    }

    public class FeatureOptionRepository : IFeatureOptionRepository
    {
        private readonly IDictionary<string, FeatureOption> _options;
        private readonly IDictionary<string, bool> _dirty;

        public FeatureOptionRepository(IEqualityComparer<string> comparer = default)
        {
            _options = new Dictionary<string, FeatureOption>(comparer ?? SoftString.Comparer);
            _dirty = new Dictionary<string, bool>(comparer ?? SoftString.Comparer);
        }

        public FeatureOption this[string name]
        {
            get => _options.TryGetValue(name, out var option) ? option : FeatureOption.None;
            set
            {
                if (value == FeatureOption.None)
                {
                    // Don't store empty options.
                    _options.Remove(name);
                }
                else
                {
                    _options[name] = value;
                }

                _dirty[name] = true;
            }
        }

        public bool IsDirty(string name) => _dirty.TryGetValue(name, out var isDirty) && isDirty;

        public void SaveAsDefaults() => _dirty.Clear();
    }

    public abstract class FeatureOptionRepositoryDecorator : IFeatureOptionRepository
    {
        protected FeatureOptionRepositoryDecorator(IFeatureOptionRepository instance)
        {
            Instance = instance;
        }

        protected IFeatureOptionRepository Instance { get; }

        public virtual FeatureOption this[string name]
        {
            get => Instance[name];
            set => Instance[name] = value;
        }

        public virtual bool IsDirty(string name) => Instance.IsDirty(name);

        public virtual void SaveAsDefaults() => Instance.SaveAsDefaults();
    }

    public class FeatureOptionFallback : FeatureOptionRepositoryDecorator
    {
        private readonly FeatureOption _defaultOption;

        public FeatureOptionFallback(IFeatureOptionRepository options, FeatureOption defaultOption) : base(options)
        {
            _defaultOption = defaultOption;
        }

        public override FeatureOption this[string name]
        {
            get => Instance[name] is var option && option == FeatureOption.None ? _defaultOption : option;
            set => Instance[name] = value;
        }
    }

    public class FeatureOptionLock : FeatureOptionRepositoryDecorator
    {
        private readonly IImmutableSet<string> _lockedFeatures;

        public FeatureOptionLock(IFeatureOptionRepository options, IEnumerable<string> lockedFeatures, IEqualityComparer<string> comparer = default) : base(options)
        {
            _lockedFeatures = lockedFeatures.ToImmutableHashSet(comparer ?? SoftString.Comparer);
        }

        public override FeatureOption this[string name]
        {
            get => Instance[name];
            set
            {
                if (_lockedFeatures.Contains(name))
                {
                    throw new InvalidOperationException($"Cannot set feature '{name}' option because it's locked.");
                }

                Instance[name] = value;
            }
        }
    }

    [PublicAPI]
    public interface IFeatureToggle
    {
        IFeatureOptionRepository Options { get; }

        Task<T> ExecuteAsync<T>(string name, Func<Task<T>> body, Func<Task<T>> fallback);
    }

    public class FeatureToggle : IFeatureToggle
    {
        public FeatureToggle(IFeatureOptionRepository options)
        {
            Options = options;
        }

        public IFeatureOptionRepository Options { get; }

        public async Task<T> ExecuteAsync<T>(string name, Func<Task<T>> body, Func<Task<T>> fallback)
        {
            // Not catching exceptions because the caller should handle them.
            return
                this.IsEnabled(name)
                    ? await body().ConfigureAwait(false)
                    : await fallback().ConfigureAwait(false);
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

        public async Task<T> ExecuteAsync<T>(string name, Func<Task<T>> body, Func<Task<T>> fallback)
        {
            using (_logger.BeginScope().CorrelationHandle("Feature").AttachElapsed())
            {
                _logger.Log(Abstraction.Layer.Service().Meta(new { FeatureName = name }).Trace());

                if (_featureToggle.Options.IsDirty(name))
                {
                    _logger.Log(Abstraction.Layer.Service().Decision("Using custom feature options.").Because("Customized by user.").Meta(new { Options = _featureToggle.Options[name] }));
                }

                return await _featureToggle.ExecuteAsync(name, body, fallback);
            }
        }
    }
}