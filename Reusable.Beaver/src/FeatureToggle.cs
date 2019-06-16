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
        bool IsOn(string name);

        [NotNull]
        FeatureOption Options(string name);

        Task<T> ExecuteAsync<T>(string name, Func<Task<T>> bodyWhenOn, Func<Task<T>> bodyWhenOff);

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

        public bool IsOn(string name)
        {
            var options = Options(name);
            return options.Contains(FeatureOption.Enable) && (CanExecuteCallback?.Invoke(options) ?? true);
        }

        public FeatureOption Options(string name)
        {
            return
                _options.TryGetValue(name, out var customOptions)
                    ? customOptions
                    : DefaultOptions(name);
        }

        private FeatureOption DefaultOptions(string name)
        {
            return GetDefaultOptionsCallback?.Invoke(name) ?? _defaultOptions;
        }

        public async Task<T> ExecuteAsync<T>(string name, Func<Task<T>> bodyWhenOn, Func<Task<T>> bodyWhenOff)
        {
            var options = Options(name);

            using (_logger.BeginScope().WithCorrelationHandle("Feature").AttachElapsed())
            {
                // Not catching exceptions because the caller should handle them.
                try
                {
                    if (IsOn(name))
                    {
                        if (options.Contains(FeatureOption.Warn) && !_defaultOptions.Contains(FeatureOption.Enable))
                        {
                            _logger.Log(Abstraction.Layer.Service().Decision($"Using feature '{name}'").Because("Enabled").Warning());
                        }

                        return await bodyWhenOn();
                    }
                    else
                    {
                        if (options.Contains(FeatureOption.Warn) && _defaultOptions.Contains(FeatureOption.Enable))
                        {
                            _logger.Log(Abstraction.Layer.Service().Decision($"Not using feature '{name}'").Because("Disabled").Warning());
                        }

                        return await bodyWhenOff();
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
            var newOptions = configure(Options(name));
            if (newOptions == DefaultOptions(name))
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