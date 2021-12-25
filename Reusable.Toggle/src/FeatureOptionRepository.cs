using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFeatureOptionRepository
    {
        // Gets or sets feature options.
        [NotNull]
        Option<Feature> this[Feature name] { get; set; }

        bool IsDirty(Feature name);

        bool TryGetOption(Feature name, out Option<Feature> option);

        bool Remove(Feature name);

        // Saves current options as default.
        void SaveChanges(Feature name = default);
    }

    public class FeatureOptionRepository : IFeatureOptionRepository
    {
        private readonly Dictionary<Feature, Option<Feature>> _options;
        private readonly HashSet<Feature> _dirty;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public FeatureOptionRepository()
        {
            _options = new Dictionary<Feature, Option<Feature>>();
            _dirty = new HashSet<Feature>();
        }

        public Option<Feature> this[Feature name]
        {
            get
            {
                using (_lock.Reader())
                {
                    return
                        _options.TryGetValue(name, out var option)
                            ? option
                            : throw new KeyNotFoundException($"Feature '{name}' is not configured.");
                }
            }
            set
            {
                using (_lock.Writer())
                {
                    _options[name] = value;
                    _dirty.Add(name);
                }
            }
        }

        public bool IsDirty(Feature name)
        {
            using (_lock.Reader())
            {
                return _dirty.Contains(name);
            }
        }

        public bool TryGetOption(Feature name, out Option<Feature> option)
        {
            using (_lock.Reader())
            {
                return _options.TryGetValue(name, out option);
            }
        }

        public bool Remove(Feature name)
        {
            using (_lock.Writer())
            {
                _dirty.Remove(name);
                return _options.Remove(name);
            }
        }

        public void SaveChanges(Feature name = default)
        {
            using (_lock.Writer())
            {
                if (name is null)
                {
                    _dirty.Clear();
                }
                else
                {
                    _dirty.Remove(name);
                }
            }
        }

        private class Locker
        {
            public static IDisposable EnterWriteLock(ReaderWriterLockSlim lockSlim)
            {
                lockSlim.EnterWriteLock();
                return Disposable.Create(lockSlim.ExitWriteLock);
            }
        }
    }

    public static class ReaderWriterLockSlimExtensions
    {
        public static IDisposable Reader(this ReaderWriterLockSlim lockSlim)
        {
            lockSlim.EnterReadLock();
            return Disposable.Create(lockSlim.ExitReadLock);
        }

        public static IDisposable Writer(this ReaderWriterLockSlim lockSlim)
        {
            lockSlim.EnterWriteLock();
            return Disposable.Create(lockSlim.ExitWriteLock);
        }
    }

    public abstract class FeatureOptionRepositoryDecorator : IFeatureOptionRepository
    {
        protected FeatureOptionRepositoryDecorator(IFeatureOptionRepository instance)
        {
            Instance = instance;
        }

        protected IFeatureOptionRepository Instance { get; }

        public virtual Option<Feature> this[Feature name]
        {
            get => Instance[name];
            set => Instance[name] = value;
        }

        public bool IsDirty(Feature name) => Instance.IsDirty(name);

        public bool TryGetOption(Feature name, out Option<Feature> option) => Instance.TryGetOption(name, out option);

        public bool Remove(Feature name) => Instance.Remove(name);

        public virtual void SaveChanges(Feature name = default) => Instance.SaveChanges();
    }

    // Provides default feature-options if not already configured. 
    public class FeatureOptionFallback : FeatureOptionRepositoryDecorator
    {
        private readonly Option<Feature> _defaultOption;

        public FeatureOptionFallback(IFeatureOptionRepository options, Option<Feature> defaultOption) : base(options)
        {
            _defaultOption = defaultOption;
        }

        public override Option<Feature> this[Feature name]
        {
            get => TryGetOption(name, out var option) ? option : _defaultOption;
            set => Instance[name] = value;
        }

        public class Enabled : FeatureOptionFallback
        {
            public Enabled(IFeatureOptionRepository options, Option<Feature> other = default)
                : base(options, Feature.Options.Enabled | (other ?? Option<Feature>.None)) { }
        }
    }

    // Locks feature option setter.
    public class FeatureOptionLock : FeatureOptionRepositoryDecorator
    {
        public FeatureOptionLock(IFeatureOptionRepository options) : base(options) { }

        public override Option<Feature> this[Feature name]
        {
            get => Instance[name];
            set
            {
                if (Instance[name].Contains(Feature.Options.Locked))
                {
                    throw new InvalidOperationException($"Cannot set options for feature '{name}' because it's locked.");
                }

                Instance[name] = value;
            }
        }
    }
}