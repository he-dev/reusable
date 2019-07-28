using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
    public interface IFeatureOptionRepository
    {
        // Gets or sets feature options.
        [NotNull]
        FeatureOption this[FeatureIdentifier name] { get; set; }

        // Saves current options as default.
        void SaveChanges(FeatureIdentifier name = default);
    }

    public class FeatureOptionRepository : IFeatureOptionRepository
    {
        private readonly Dictionary<FeatureIdentifier, FeatureOption> _options;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public FeatureOptionRepository()
        {
            _options = new Dictionary<FeatureIdentifier, FeatureOption>();
        }

        public FeatureOption this[FeatureIdentifier name]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _options.TryGetValue(name, out var option) ? option : FeatureOption.None;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    _options[name] = value.RemoveFlag(FeatureOption.Saved).SetFlag(FeatureOption.Dirty);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public void SaveChanges(FeatureIdentifier name = default)
        {
            _lock.EnterWriteLock();
            try
            {
                var names =
                    name is null
                        ? _options.Keys.ToList() // <-- prevents collection-modified-exception 
                        : new List<FeatureIdentifier> { name };

                foreach (var n in names)
                {
                    _options[n] = _options[n].RemoveFlag(FeatureOption.Dirty).SetFlag(FeatureOption.Saved);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public abstract class FeatureOptionRepositoryDecorator : IFeatureOptionRepository
    {
        protected FeatureOptionRepositoryDecorator(IFeatureOptionRepository instance)
        {
            Instance = instance;
        }

        protected IFeatureOptionRepository Instance { get; }

        public virtual FeatureOption this[FeatureIdentifier name]
        {
            get => Instance[name];
            set => Instance[name] = value;
        }

        public virtual void SaveChanges(FeatureIdentifier name = default) => Instance.SaveChanges();
    }

    // Provides default feature-options if not already configured. 
    public class FeatureOptionFallback : FeatureOptionRepositoryDecorator
    {
        private readonly FeatureOption _defaultOption;

        public FeatureOptionFallback(IFeatureOptionRepository options, FeatureOption defaultOption) : base(options)
        {
            _defaultOption = defaultOption;
        }

        public override FeatureOption this[FeatureIdentifier name]
        {
            get => Instance[name] is var option && option == FeatureOption.None ? _defaultOption : option;
            set => Instance[name] = value;
        }

        public class Enabled : FeatureOptionFallback
        {
            public Enabled(IFeatureOptionRepository options, FeatureOption other = default)
                : base(options, FeatureOption.Enabled | (other ?? FeatureOption.None)) { }
        }
    }

    // Locks feature option setter.
    public class FeatureOptionLock : FeatureOptionRepositoryDecorator
    {
        public FeatureOptionLock(IFeatureOptionRepository options) : base(options) { }

        public override FeatureOption this[FeatureIdentifier name]
        {
            get => Instance[name];
            set
            {
                if (Instance[name].Contains(FeatureOption.Locked))
                {
                    throw new InvalidOperationException($"Cannot set options for feature '{name}' because it's locked.");
                }

                Instance[name] = value;
            }
        }
    }

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

    [PublicAPI]
    public class FeatureSelection
    {
        public FeatureSelection(IFeatureToggle features, FeatureIdentifier name)
        {
            Features = features;
            Name = name;
        }

        private IFeatureToggle Features { get; }

        private FeatureIdentifier Name { get; }

        public FeatureOption Options
        {
            get => Features.Options[Name];
            set => Features.Options[Name] = value;
        }

        public void SaveChanges() => Features.Options.SaveChanges(Name);

        //public static implicit operator FeatureIdentifier(FeatureSelection selection) => selection.Name;
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
                    Options.Toggle(name);
                    if (Options[name].Contains(FeatureOption.ToggleOnce))
                    {
                        Options[name] = Options[name].RemoveFlag(FeatureOption.Toggle | FeatureOption.ToggleOnce);
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