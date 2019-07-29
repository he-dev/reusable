using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFeatureOptionRepository
    {
        // Gets or sets feature options.
        [NotNull]
        FeatureOption this[FeatureIdentifier name] { get; set; }

        bool Remove(FeatureIdentifier name);

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

        public bool Remove(FeatureIdentifier name)
        {
            _lock.EnterWriteLock();
            try
            {
                return _options.Remove(name);
            }
            finally
            {
                _lock.ExitWriteLock();
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
}