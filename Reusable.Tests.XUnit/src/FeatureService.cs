using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Tests.XUnit.Features;
using Xunit;

namespace Reusable.Tests.XUnit
{
    using static FeatureOptions;


    public interface IFeatureService
    {
        Task<T> ExecuteAsync<T>(string name, Func<Task<T>> body, Func<Task<T>> bodyWhenDisabled);

        [NotNull]
        IFeatureService Configure(string name, Func<FeatureOptions, FeatureOptions> configure);
    }

    public class FeatureService : IFeatureService
    {
        private readonly FeatureOptions _defaultOptions;
        private readonly ILogger _logger;
        private readonly IDictionary<string, FeatureOptions> _options = new Dictionary<string, FeatureOptions>();

        public FeatureService(ILogger<FeatureService> logger, FeatureOptions defaultOptions = Enabled | Warn | Telemetry)
        {
            _logger = logger;
            _defaultOptions = defaultOptions;
        }

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
                    if (options.HasFlag(Enabled))
                    {
                        if (options.HasFlag(Warn) && !_defaultOptions.HasFlag(Enabled))
                        {
                            _logger.Log(Abstraction.Layer.Service().Decision($"Using feature '{name}'").Because("Enabled").Warning());
                        }

                        return await body();
                    }
                    else
                    {
                        if (options.HasFlag(Warn) && _defaultOptions.HasFlag(Enabled))
                        {
                            _logger.Log(Abstraction.Layer.Service().Decision($"Not using feature '{name}'").Because("Disabled").Warning());
                        }

                        return await bodyWhenDisabled();
                    }
                }
                finally
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(name).Completed());
                }
            }
        }

        public IFeatureService Configure(string name, Func<FeatureOptions, FeatureOptions> configure)
        {
            _options[name] =
                _options.TryGetValue(name, out var options)
                    ? configure(options)
                    : configure(_defaultOptions);

            return this;
        }
    }

    public static class FeatureServiceHelpers
    {
        public static async Task ExecuteAsync(this IFeatureService features, string name, Func<Task> body, Func<Task> bodyWhenDisabled)
        {
            await features.ExecuteAsync
            (
                name,
                () => ExecuteAsync(body),
                () => ExecuteAsync(bodyWhenDisabled)
            );

            async Task<object> ExecuteAsync(Func<Task> b)
            {
                await b();
                return default;
            }
        }

        public static async Task ExecuteAsync(this IFeatureService features, string name, Func<Task> body)
        {
            await features.ExecuteAsync(name, body, () => Task.FromResult<object>(default));
        }


        public static void Execute(this IFeatureService features, string name, Action body, Action bodyWhenDisabled)
        {
            features.ExecuteAsync
            (
                name,
                () => ExecuteAsync(body),
                () => ExecuteAsync(bodyWhenDisabled)
            ).GetAwaiter().GetResult();

            Task<object> ExecuteAsync(Action b)
            {
                b();
                return Task.FromResult(default(object));
            }
        }

        public static void Execute(this IFeatureService features, string name, Action body)
        {
            features.Execute(name, body, () => { });
        }

        [NotNull]
        public static IFeatureService Configure(this IFeatureService features, IEnumerable<string> names, Func<FeatureOptions, FeatureOptions> configure)
        {
            foreach (var name in names)
            {
                features.Configure(name, configure);
            }

            return features;
        }

        public static IEnumerable<string> ToFeatureNames(this IEnumerable<(Type Feature, PropertyInfo Property)> features, IKeyFactory keyFactory = default)
        {
            return
                from t in features
                // x.Member
                let l = Expression.Lambda(
                    Expression.Property(
                        Expression.Constant(null, t.Feature),
                        t.Property.Name
                    )
                )
                select (keyFactory ?? KeyFactory.Default).CreateKey(l);
        }

        public static IEnumerable<(Type Feature, PropertyInfo Property)> GetFeatures(this IEnumerable<Type> features, params string[] tags)
        {
            if (!tags.Any()) throw new ArgumentException("You need to specify at least one tag.");

            return features.GetFeatures(tags.AsEnumerable());
        }

        public static IEnumerable<(Type Feature, PropertyInfo Property)> GetFeatures(this IEnumerable<Type> features, IEnumerable<string> tags)
        {
            tags = tags.Distinct(SoftString.Comparer);

            return
                from f in features
                let featureTags = f.GetTags()
                from p in f.GetProperties()
                let propertyTags = p.GetTags().Concat(featureTags).Distinct(SoftString.Comparer)
                where propertyTags.Matches(tags)
                select (f, p);
        }

        private static IEnumerable<string> GetTags(this MemberInfo member)
        {
            return
                member
                    .GetCustomAttributes<TagAttribute>()
                    .SelectMany(t => t);
        }

        private static bool Matches(this IEnumerable<string> propertyTags, IEnumerable<string> otherTags)
        {
            return
                !otherTags
                    .Except(propertyTags, SoftString.Comparer)
                    .Any();
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

    [Flags]
    public enum FeatureOptions
    {
        None = 0,

        /// <summary>
        /// When set a feature is enabled.
        /// </summary>
        Enabled = 1 << 0,

        /// <summary>
        /// When set a warning is logged when a feature is toggled.
        /// </summary>
        Warn = 1 << 1,

        /// <summary>
        /// When set feature usage statistics are logged.
        /// </summary>
        Telemetry = 1 << 2, // For future use
    }

    public static class Tags
    {
        public const string Database = nameof(Database);
        public const string SaveChanges = nameof(SaveChanges);
    }

    public class FeatureServiceTest
    {
        [Fact]
        public void Can_create_key_from_type_and_member()
        {
            Assert.Equal("Demo.Greeting", From<IDemo>.Select(x => x.Greeting));
        }

        [Fact]
        public void Can_configure_features_by_tags()
        {
            var features = new FeatureService(Logger<FeatureService>.Null);

            var names = new[] { typeof(IDemo), typeof(IDatabase) }.GetFeatures("io").ToFeatureNames();
            features.Configure(names, o => o ^ Enabled);

            var bodyCounter = 0;
            var otherCounter = 0;
            features.Execute(From<IDemo>.Select(x => x.Greeting), () => bodyCounter++, () => otherCounter++);
            features.Execute(From<IDemo>.Select(x => x.ReadFile), () => bodyCounter++, () => otherCounter++);
            features.Execute(From<IDatabase>.Select(x => x.Commit), () => bodyCounter++, () => otherCounter++);

            Assert.Equal(1, bodyCounter);
            Assert.Equal(2, otherCounter);
        }
    }

    public class FeatureServiceDemo
    {
        private readonly FeatureService _features = new FeatureService(Logger<FeatureService>.Null);

        public async Task Start()
        {
            SayHallo();

            _features.Configure(nameof(SayHallo), o => o ^ Enabled);
            //_features.Configure(Use<IDemo>.Namespace, x => x.Greeting, o => o ^ Enabled);
            _features.Configure(From<IDemo>.Select(x => x.Greeting), o => o ^ Enabled);

            SayHallo();
        }

        private void SayHallo()
        {
            _features.Execute(nameof(SayHallo), () => Console.WriteLine("Hallo"), () => Console.WriteLine("You've disabled it!"));
        }
    }

    /*
     Example settings:
     
     {
        "Service.Feature1": "Enabled, LogWhenEnabled, LogWhenDisabled"
        "Service.Feature2": {
            "Options": "Enabled, LogWhenEnabled, LogWhenDisabled",
        }
     }
     
     */

    namespace Features
    {
        [TypeMemberKeyFactory]
        [RemovePrefix("I")]
        public interface IDemo : INamespace
        {
            object Greeting { get; }

            [Tag("io")]
            object ReadFile { get; }
        }

        [TypeMemberKeyFactory]
        [RemovePrefix("I")]
        public interface IDatabase : INamespace
        {
            [Tag("io")]
            object Commit { get; }
        }
    }
}