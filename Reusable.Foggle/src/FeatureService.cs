using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Foggle
{
    [PublicAPI]
    public interface IFeatureService
    {
        Task<T> ExecuteAsync<T>(string name, Func<Task<T>> body, Func<Task<T>> bodyWhenDisabled);

        [NotNull]
        IFeatureService Configure(string name, Func<FeatureOption, FeatureOption> configure);
    }

    public class FeatureService : IFeatureService
    {
        private readonly FeatureOption _defaultOptions;
        private readonly ILogger _logger;
        private readonly IDictionary<string, FeatureOption> _options = new Dictionary<string, FeatureOption>();

        public FeatureService(ILogger<FeatureService> logger, FeatureOption defaultOptions)
        {
            _logger = logger;
            _defaultOptions = defaultOptions;
        }

        //public Func<(string Name, FeatureOption Options), Task> BeforeExecuteAsync { get; set; }

        //public Func<(string Name, FeatureOption Options), Task> AfterExecuteAsync { get; set; }

        public async Task<T> ExecuteAsync<T>(string name, Func<Task<T>> body, Func<Task<T>> bodyWhenDisabled)
        {
            var options =
                _options.TryGetValue(name, out var customOptions)
                    ? customOptions
                    : _defaultOptions;

            using (_logger.BeginScope().WithCorrelationHandle("Feature").AttachElapsed())
            {
                // Not catching exceptions because the caller should handle them.
                try
                {
                    if (options.Contains(FeatureOption.Enable))
                    {
                        if (options.Contains(FeatureOption.Warn) && !_defaultOptions.Contains(FeatureOption.Enable))
                        {
                            _logger.Log(Abstraction.Layer.Service().Decision($"Using feature '{name}'").Because("Enabled").Warning());
                        }

                        //await (BeforeExecuteAsync?.Invoke((name, options)) ?? Task.CompletedTask);

                        return await body();
                    }
                    else
                    {
                        if (options.Contains(FeatureOption.Warn) && _defaultOptions.Contains(FeatureOption.Enable))
                        {
                            _logger.Log(Abstraction.Layer.Service().Decision($"Not using feature '{name}'").Because("Disabled").Warning());
                        }

                        return await bodyWhenDisabled();
                    }
                }
                finally
                {
                    //await (AfterExecuteAsync?.Invoke((name, options)) ?? Task.CompletedTask);
                    _logger.Log(Abstraction.Layer.Service().Routine(name).Completed());
                }
            }
        }

        public IFeatureService Configure(string name, Func<FeatureOption, FeatureOption> configure)
        {
            _options[name] =
                _options.TryGetValue(name, out var options)
                    ? configure(options)
                    : configure(_defaultOptions);

            return this;
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property)]
    public class TagAttribute : Attribute, IEnumerable<string>
    {
        private readonly string[] _names;

        public TagAttribute(params string[] names) => _names = names;

        public IEnumerator<string> GetEnumerator() => _names.Cast<string>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}