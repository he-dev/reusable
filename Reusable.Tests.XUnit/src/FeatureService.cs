using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
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

    public interface IOption
    {
        [NotNull]
        SoftString Name { get; }

        int Flag { get; }

        bool IsBit { get; }
    }

    public interface IOption<T>
        : IOption, IEquatable<IOption<T>>, IComparable<IOption<T>>, IComparable
        // Option type must be a type of itself.
        where T : class, IOption { }

    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class Option<T> : IOption<T> where T : class, IOption
    {
        protected const string CompositeName = "Composite";

        private static readonly OptionComparer Comparer = new OptionComparer();

        protected static readonly ConcurrentDictionary<SoftString, IImmutableSet<IOption>> Flags = new ConcurrentDictionary<SoftString, IImmutableSet<IOption>>();

        static Option()
        {
            // Always initialize "None".
            None = CreateWithCallerName();
        }

        protected Option(SoftString name, int flag)
        {
            if (GetType() != typeof(T)) throw DynamicException.Create("OptionTypeMismatch", "Option must be a type of itself.");

            Name = name;
            Flag = flag;
        }

        [DebuggerStepThrough]
        public override string ToString() => $"{Category.ToString()}.{Name.ToString()}";

        [NotNull]
        public static T None { get; }

        private static SoftString Category { [DebuggerStepThrough] get; } = typeof(T).Name;

        public SoftString Name { [DebuggerStepThrough] get; }

        [AutoEqualityProperty]
        public int Flag { [DebuggerStepThrough] get; }

        // Or IsPowerOfTwo
        public bool IsBit => (Flag & (Flag - 1)) == 0;

        #region Factories

        [NotNull]
        public static T Create(SoftString name, IOption<T> option = default)
        {
            var optionsUpdated = Flags.AddOrUpdate
            (
                typeof(T).Name,
                // There is always "None".
                t => ImmutableSortedSet<IOption>.Empty.Add(Create(nameof(None), 0)),
                (category, options) =>
                {
                    if (name == nameof(None))
                    {
                        return options;
                    }

                    if (options.Any(o => o.Name == name))
                    {
                        throw DynamicException.Create("DuplicateOption", $"The option '{name}' is defined more the once.");
                    }

                    var bitCount = options.Count(o => o.IsBit);
                    var newOption = Create(name, bitCount == 1 ? 1 : (bitCount - 1) << 1);
                    return options.Add(newOption);
                }
            );

            return (T)optionsUpdated.Last();
        }

        [NotNull]
        public static T CreateWithCallerName(IOption<T> option = default, [CallerMemberName] string name = default)
        {
            return Create(name, option);
        }

        protected static T Create(SoftString name, int value)
        {
            return (T)Activator.CreateInstance(typeof(T), name, value);
        }

        public static T FromName([NotNull] string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            return
                (T)Flags[Category]
                    .FirstOrDefault(o => o.Name == value)
                ?? throw DynamicException.Create("OptionOutOfRange", $"There is no such option as '{value}'.");
        }

        [NotNull]
        public static T FromValue(int value)
        {
            var options = Flags[Category];

            if (value > options.Max(o => o.Flag))
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(value), $"Value {value} is greater than the highest option.");
            }

            if (options.SingleOrDefault(o => o.Flag == value) is var option && !(option is null))
            {
                return (T)option;
            }

            var newFlag =
                options
                    .Cast<IOption<T>>()
                    .Where(o => o.IsBit && (o.Flag & value) == o.Flag)
                    .Aggregate((current, next) => current | next);

            return Create(CompositeName, newFlag);
        }

        #endregion

        public bool Contains(IOption<T> option) => Contains(option.Flag);

        public bool Contains(int flags) => (Flag & flags) == flags;

        public int CompareTo(IOption<T> other) => Comparer.Compare(this, other);

        public int CompareTo(object other) => Comparer.Compare(this, other);

        #region IEquatable

        public bool Equals(IOption<T> other) => AutoEquality<IOption<T>>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as IOption<T>);

        public override int GetHashCode() => AutoEquality<IOption<T>>.Comparer.GetHashCode(this);

        #endregion

        #region Operators

        public static implicit operator string(Option<T> option) => option?.ToString() ?? throw new ArgumentNullException(nameof(option));

        public static implicit operator int(Option<T> option) => option?.Flag ?? throw new ArgumentNullException(nameof(option));

        public static bool operator ==(Option<T> left, Option<T> right) => Comparer.Compare(left, right) == 0;

        public static bool operator !=(Option<T> left, Option<T> right) => !(left == right);

        public static bool operator <(Option<T> left, Option<T> right) => Comparer.Compare(left, right) < 0;

        public static bool operator <=(Option<T> left, Option<T> right) => Comparer.Compare(left, right) <= 0;

        public static bool operator >(Option<T> left, Option<T> right) => Comparer.Compare(left, right) > 0;

        public static bool operator >=(Option<T> left, Option<T> right) => Comparer.Compare(left, right) >= 0;

        public static T operator |(Option<T> left, Option<T> right) => Create(CompositeName, left.Flag | right.Flag);

        #endregion

        private class OptionComparer : IComparer<Option<T>>, IComparer
        {
            public int Compare(Option<T> left, Option<T> right)
            {
                if (ReferenceEquals(left, right)) return 0;
                if (ReferenceEquals(left, null)) return 1;
                if (ReferenceEquals(right, null)) return -1;
                return left.Flag - right.Flag;
            }

            public int Compare(object left, object right) => Compare(left as Option<T>, right as Option<T>);
        }
    }

    public class FeatureOption : Option<FeatureOption>
    {
        public FeatureOption(SoftString name, int value) : base(name, value) { }

        /// <summary>
        /// When set a feature is enabled.
        /// </summary>
        public static readonly FeatureOption Enable = CreateWithCallerName();

        /// <summary>
        /// When set a warning is logged when a feature is toggled.
        /// </summary>
        public static readonly FeatureOption Warn = CreateWithCallerName();

        /// <summary>
        /// When set feature usage statistics are logged.
        /// </summary>
        public static readonly FeatureOption Telemetry = CreateWithCallerName();

        public static readonly FeatureOption Default = CreateWithCallerName(Enable | Warn);
    }

    public class OptionTest
    {
        [Fact]
        public void Examples()
        {
            Assert.Equal(new[] { 0, 1, 2, 4 }, new[]
            {
                FeatureOption.None,
                FeatureOption.Enable,
                FeatureOption.Warn,
                FeatureOption.Telemetry
            }.Select(o => o.Flag));

            Assert.Equal(FeatureOption.Enable, FeatureOption.Enable);
            Assert.NotEqual(FeatureOption.Enable, FeatureOption.Telemetry);

            var fromName = FeatureOption.FromName("Warn");
            Assert.Equal(FeatureOption.Warn, fromName);

            var fromValue = FeatureOption.FromValue(3);
            Assert.Equal(FeatureOption.Enable | FeatureOption.Warn, fromValue);

            Assert.True(FeatureOption.None < FeatureOption.Enable);
            Assert.True(FeatureOption.Enable < FeatureOption.Telemetry);

            Assert.Throws<ArgumentOutOfRangeException>(() => FeatureOption.FromValue(1000));
        }
    }
}