using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Beaver
{
    [PublicAPI]
    public interface IFeatureToggle
    {
        bool CanExecute(string name);

        [NotNull]
        FeatureOption GetOptions(string name);

        Task<T> ExecuteAsync<T>(string name, Func<Task<T>> body, Func<Task<T>> fallback);

        [NotNull]
        IFeatureToggle Configure(string name, Func<FeatureOption, FeatureOption> configure);
    }

    public class FeatureToggle : IFeatureToggle
    {
        private readonly FeatureOption _defaultOptions;
        private readonly ILogger _logger;
        private readonly IDictionary<string, FeatureOption> _options = new Dictionary<string, FeatureOption>();

        public FeatureToggle(ILogger<FeatureToggle> logger, FeatureOption defaultOptions)
        {
            _logger = logger;
            _defaultOptions = defaultOptions;
        }

        [CanBeNull]
        public Func<FeatureOption, bool> CanExecuteCallback { get; set; }
        
        [CanBeNull]
        public Func<string, FeatureOption> GetDefaultOptionsCallback { get; set; }

        public bool CanExecute(string name)
        {
            var options = GetOptions(name);
            return options.Contains(FeatureOption.Enable) && (CanExecuteCallback?.Invoke(options) ?? true);
        }

        public FeatureOption GetOptions(string name)
        {
            return
                _options.TryGetValue(name, out var customOptions)
                    ? customOptions
                    : GetDefaultOptions(name);
        }

        private FeatureOption GetDefaultOptions(string name)
        {
            return GetDefaultOptionsCallback?.Invoke(name) ?? _defaultOptions;
        }

        public async Task<T> ExecuteAsync<T>(string name, Func<Task<T>> body, Func<Task<T>> fallback)
        {
            var options = GetOptions(name);
            var defaultOptions = GetDefaultOptions(name);

            using (_logger.BeginScope().CorrelationHandle("Feature").AttachElapsed())
            {
                _logger.Log(Abstraction.Layer.Service().Meta(new { FeatureName = name }).Trace());
                
                // Not catching exceptions because the caller should handle them.
                try
                {
                    if (CanExecute(name))
                    {
                        if (options.Contains(FeatureOption.Warn) && !defaultOptions.Contains(FeatureOption.Enable))
                        {
                            _logger.Log(Abstraction.Layer.Service().Decision($"Using feature '{name}'").Because("Enabled").Warning());
                        }

                        return await body();
                    }
                    else
                    {
                        if (options.Contains(FeatureOption.Warn) && defaultOptions.Contains(FeatureOption.Enable))
                        {
                            _logger.Log(Abstraction.Layer.Service().Decision($"Not using feature '{name}'").Because("Disabled").Warning());
                        }

                        return await fallback();
                    }
                }
                finally
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(name).Completed());
                }
            }
        }

        public IFeatureToggle Configure(string name, Func<FeatureOption, FeatureOption> configure)
        {
            var newOptions = configure(GetOptions(name));
            if (newOptions == GetDefaultOptions(name))
            {
                // Don't store default options.
                _options.Remove(name);
            }
            else
            {
                _options[name] = newOptions;
            }

            return this;
        }
    }
}