using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFeatureOptionRepository
    {
        // Gets or sets feature options.
        [NotNull]
        FeatureOption this[FeatureIdentifier name] { get; set; }

        bool IsDirty(FeatureIdentifier name);

        bool TryGetOption(FeatureIdentifier name, out FeatureOption option);

        bool Remove(FeatureIdentifier name);

        // Saves current options as default.
        void SaveChanges(FeatureIdentifier name = default);
    }

    public class FeatureOptionRepository : IFeatureOptionRepository
    {
        private readonly Dictionary<FeatureIdentifier, FeatureOption> _options;
        private readonly HashSet<FeatureIdentifier> _dirty;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public FeatureOptionRepository()
        {
            _options = new Dictionary<FeatureIdentifier, FeatureOption>();
            _dirty = new HashSet<FeatureIdentifier>();
        }

        public FeatureOption this[FeatureIdentifier name]
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

        public bool IsDirty(FeatureIdentifier name)
        {
            using (_lock.Reader())
            {
                return _dirty.Contains(name);
            }
        }

        public bool TryGetOption(FeatureIdentifier name, out FeatureOption option)
        {
            using (_lock.Reader())
            {
                return _options.TryGetValue(name, out option);
            }
        }

        public bool Remove(FeatureIdentifier name)
        {
            using (_lock.Writer())
            {
                _dirty.Remove(name);
                return _options.Remove(name);
            }
        }

        public void SaveChanges(FeatureIdentifier name = default)
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

        public virtual FeatureOption this[FeatureIdentifier name]
        {
            get => Instance[name];
            set => Instance[name] = value;
        }

        public bool IsDirty(FeatureIdentifier name) => Instance.IsDirty(name);

        public bool TryGetOption(FeatureIdentifier name, out FeatureOption option) => Instance.TryGetOption(name, out option);

        public bool Remove(FeatureIdentifier name) => Instance.Remove(name);

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
            get => TryGetOption(name, out var option) ? option : _defaultOption;
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
}