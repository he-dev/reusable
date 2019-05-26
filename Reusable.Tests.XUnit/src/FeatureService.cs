using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Diagnostics;
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

    [PublicAPI]
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

    [PublicAPI]
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

        public static IEnumerable<string> Keys(this IEnumerable<FeatureInfo> features, IKeyFactory keyFactory = default)
        {
            return
                from t in features
                // () => x.Member
                let l = Expression.Lambda(
                    Expression.Property(
                        Expression.Constant(null, t.Category),
                        t.Property.Name
                    )
                )
                select (keyFactory ?? KeyFactory.Default).CreateKey(l);
        }

        public static IEnumerable<string> Tags(this FeatureInfo feature)
        {
            return
                feature
                    .Category
                    .Tags()
                    .Concat(feature.Property.Tags())
                    .Distinct(SoftString.Comparer);
        }

        private static IEnumerable<string> Tags(this MemberInfo member)
        {
            return
                member
                    .GetCustomAttributes<TagAttribute>()
                    .SelectMany(t => t);
        }

        public static bool IsSubsetOf<T>(this IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer = default)
        {
            return
                !second
                    .Except(first, comparer ?? EqualityComparer<T>.Default)
                    .Any();
        }

        public static IEnumerable<FeatureInfo> WhereTags(this IEnumerable<FeatureInfo> features, params string[] tags)
        {
            if (!tags.Any()) throw new ArgumentException("You need to specify at least one tag.");

            return
                features
                    .Where(f => f.Tags().IsSubsetOf(tags, SoftString.Comparer));
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

    public readonly struct FeatureInfo
    {
        public FeatureInfo(Type category, PropertyInfo feature)
        {
            Category = category;
            Property = feature;
        }

        [NotNull]
        public Type Category { get; }

        [NotNull]
        public PropertyInfo Property { get; }
    }

    public class FeatureCollection : IEnumerable<FeatureInfo>
    {
        private readonly IList<Type> _categories = new List<Type>();

        [NotNull]
        public static FeatureCollection Empty => new FeatureCollection();

        [NotNull]
        public FeatureCollection Add<T>() where T : INamespace
        {
            _categories.Add(typeof(T));
            return this;
        }

        public IEnumerator<FeatureInfo> GetEnumerator()
        {
            return
            (
                from f in _categories
                from p in f.GetProperties()
                select new FeatureInfo(f, p)
            ).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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

            var names = FeatureCollection.Empty.Add<IDemo>().Add<IDatabase>().WhereTags("io").Keys();

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
        [TrimStart("I")]
        public interface IDemo : INamespace
        {
            object Greeting { get; }

            [Tag("io")]
            object ReadFile { get; }
        }

        [TypeMemberKeyFactory]
        [TrimStart("I")]
        public interface IDatabase : INamespace
        {
            [Tag("io")]
            object Commit { get; }
        }
    }


    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class Option : IEquatable<Option>, IComparable<Option>, IComparable
    {
        private static readonly OptionComparer Comparer = new OptionComparer();

        private static readonly ConcurrentDictionary<SoftString, int> Flags = new ConcurrentDictionary<SoftString, int>();

        public Option(SoftString category, SoftString name, int flag)
        {
            Category = category;
            Name = name;
            Flag = flag;
        }

        private string DebuggerDisplay => ToString();


        [AutoEqualityProperty]
        public SoftString Category { [DebuggerStepThrough] get; }

        public SoftString Name { [DebuggerStepThrough] get; }

        [AutoEqualityProperty]
        public int Flag { [DebuggerStepThrough] get; }

        public static Option Create(string category, string name)
        {
            return new Option(category, name, NextFlag(category));
        }

        [NotNull]
        public static T Create<T>(string name) where T : Option
        {
            return (T)Activator.CreateInstance(typeof(T), name, NextFlag(typeof(T).Name));
        }

        private static int NextFlag(string category)
        {
            return Flags.AddOrUpdate(category, t => 0, (k, flag) => flag == 0 ? 1 : flag << 1);
        }

        public static Option Parse([NotNull] string value, params Option[] options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (options.Select(o => o.Category).Distinct().Count() > 1) throw new ArgumentException("All options must have the same category.");

            return options.FirstOrDefault(o => o.Name == value) ?? throw DynamicException.Create("OptionOutOfRange", $"There is no such option as '{value}'.");
        }

        public static Option FromValue(int value, params Option[] options)
        {
            if (options.Select(o => o.Category).Distinct().Count() > 1) throw new ArgumentException("All options must have the same category.");

            return
                options
                    .Where(o => (o.Flag & value) == o.Flag)
                    .Aggregate((current, next) => new Option(options.First().Category, "Custom", current.Flag | next.Flag));
        }

        public bool Contains(params Option[] options) => Contains(options.Aggregate((current, next) => current.Flag | next.Flag).Flag);

        public bool Contains(int flags) => (Flag & flags) == flags;

        [DebuggerStepThrough]
        public override string ToString() => $"{Category.ToString()}.{Name.ToString()}";

        #region IEquatable

        public bool Equals(Option other) => AutoEquality<Option>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as Option);

        public override int GetHashCode() => AutoEquality<Option>.Comparer.GetHashCode(this);

        #endregion

        public int CompareTo(Option other) => Comparer.Compare(this, other);

        public int CompareTo(object other) => Comparer.Compare(this, other);

        public static implicit operator string(Option option) => option?.ToString() ?? throw new ArgumentNullException(nameof(option));

        public static implicit operator int(Option option) => option?.Flag ?? throw new ArgumentNullException(nameof(option));

        public static implicit operator Option(string value) => Parse(value);

        public static implicit operator Option(int value) => FromValue(value);

        #region Operators

        public static bool operator ==(Option left, Option right) => Comparer.Compare(left, right) == 0;
        public static bool operator !=(Option left, Option right) => !(left == right);

        public static bool operator <(Option left, Option right) => Comparer.Compare(left, right) < 0;
        public static bool operator <=(Option left, Option right) => Comparer.Compare(left, right) <= 0;

        public static bool operator >(Option left, Option right) => Comparer.Compare(left, right) > 0;
        public static bool operator >=(Option left, Option right) => Comparer.Compare(left, right) >= 0;

        public static Option operator |(Option left, Option right) => new Option(left.Category, "Custom", left.Flag | right.Flag);

        #endregion

        private class OptionComparer : IComparer<Option>, IComparer
        {
            public int Compare(Option left, Option right)
            {
                if (ReferenceEquals(left, right)) return 0;
                if (ReferenceEquals(left, null)) return 1;
                if (ReferenceEquals(right, null)) return -1;
                return left.Flag - right.Flag;
            }

            public int Compare(object left, object right) => Compare(left as Option, right as Option);
        }
    }

    public class FeatureOption : Option
    {
        public FeatureOption(string name, int value) : base(nameof(FeatureOption), name, value) { }
    }

    [PublicAPI]
    public static class FeatureOptionsNew
    {
        public static readonly FeatureOption None = Option.Create<FeatureOption>(nameof(None));

        /// <summary>
        /// When set a feature is enabled.
        /// </summary>
        public static readonly FeatureOption Enable = Option.Create<FeatureOption>(nameof(Enable));

        /// <summary>
        /// When set a warning is logged when a feature is toggled.
        /// </summary>
        public static readonly FeatureOption Warn = Option.Create<FeatureOption>(nameof(Warn));

        /// <summary>
        /// When set feature usage statistics are logged.
        /// </summary>
        public static readonly FeatureOption Telemetry = Option.Create<FeatureOption>(nameof(Warn));
    }

    public class OptionTest
    {
        [Fact]
        public void Examples()
        {
            Assert.Equal(new[] { 0, 1, 2, 4 }, new[]
            {
                FeatureOptionsNew.None,
                FeatureOptionsNew.Enable,
                FeatureOptionsNew.Warn,
                FeatureOptionsNew.Telemetry
            }.Select(o => o.Flag));

            Assert.Equal(FeatureOptionsNew.Enable, FeatureOptionsNew.Enable);
            Assert.NotEqual(FeatureOptionsNew.Enable, FeatureOptionsNew.Telemetry);

            var oParsed = Option.Parse("Warn", FeatureOptionsNew.Enable, FeatureOptionsNew.Warn, FeatureOptionsNew.Telemetry);
            Assert.Equal(FeatureOptionsNew.Warn, oParsed);

            var oFromValue = Option.FromValue(3, FeatureOptionsNew.Enable, FeatureOptionsNew.Warn, FeatureOptionsNew.Telemetry);
            Assert.Equal(FeatureOptionsNew.Enable | FeatureOptionsNew.Warn, oFromValue);

            Assert.True(FeatureOptionsNew.None < FeatureOptionsNew.Enable);
            Assert.True(FeatureOptionsNew.Enable < FeatureOptionsNew.Telemetry);
        }
    }
}