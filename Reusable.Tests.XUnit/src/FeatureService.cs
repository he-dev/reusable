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

    public abstract class Option
    {
        // Disallow anyone else use this class.
        private protected Option() { }

        [NotNull]
        public abstract SoftString Name { get; }

        public abstract int Flag { get; }

        public abstract bool IsBit { get; }
    }

    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class Option<T> : Option, IEquatable<Option<T>>, IComparable<Option<T>>, IComparable where T : Option
    {
        protected const string Unknown = nameof(Unknown);

        private static readonly OptionComparer Comparer = new OptionComparer();

        protected static readonly ConcurrentDictionary<SoftString, IImmutableSet<Option>> Flags = new ConcurrentDictionary<SoftString, IImmutableSet<Option>>();

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

        #region Default options

        [NotNull]
        public static T None { get; }

        [NotNull]
        public static T All => Create(nameof(All), Bits.Select(o => o.Flag));

        [NotNull]
        public static Option<T> Max => Flags[Category].Cast<Option<T>>().OrderByDescending(o => o.Flag).First();

        #endregion

        [NotNull, ItemNotNull]
        public static IEnumerable<Option<T>> Bits => Flags[Category].Where(o => o.IsBit).Cast<Option<T>>();
        
        private static SoftString Category { [DebuggerStepThrough] get; } = typeof(T).Name;

        #region Option

        public override SoftString Name { [DebuggerStepThrough] get; }

        [AutoEqualityProperty]
        public override int Flag { [DebuggerStepThrough] get; }

        // Or IsPowerOfTwo
        public override bool IsBit => (Flag & (Flag - 1)) == 0;

        #endregion

        #region Factories

        [NotNull]
        public static T Create(SoftString name, Option<T> option = default)
        {
            var forbidden = new SoftString[] { nameof(None), nameof(All), nameof(Max) };
            if (name.In(forbidden, SoftString.Comparer))
            {
                throw new ArgumentException(paramName: nameof(name), message: $"You must not create options with the following, reserved names [{forbidden.Select(f => f.ToString()).Join(", ")}].");
            }

            var optionsUpdated = Flags.AddOrUpdate
            (
                typeof(T).Name,
                // There is always "None".
                t => ImmutableSortedSet<Option>.Empty.Add(Create(nameof(None), 0)),
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
        public static T CreateWithCallerName(Option<T> option = default, [CallerMemberName] string name = default)
        {
            return Create(name, option);
        }

        protected static T Create(SoftString name, IEnumerable<int> flags)
        {
            var flag = flags.Aggregate(0, (current, next) => current | next);
            return (T)Activator.CreateInstance(typeof(T), name, flag);
        }

        protected static T Create(SoftString name, params int[] flags)
        {
            return Create(name, flags.AsEnumerable());
        }

        [NotNull]
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
            if (value > Max)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(value), $"Value {value} is greater than the highest option.");
            }

            // Is this a known value?
            if (TryGetKnownOption(value, out var knownOption))
            {
                return (T)knownOption;
            }

            var newFlags = Bits.Where(o => (o.Flag & value) == o.Flag).Select(o => o.Flag);
            return Create(Unknown, newFlags);
        }

        private static bool TryGetKnownOption(int flag, out Option option)
        {
            if (Flags[Category].SingleOrDefault(o => o.Flag == flag) is var knownOption && !(knownOption is null))
            {
                option = knownOption;
                return true;
            }
            else
            {
                option = default;
                return false;
            }
        }

        #endregion

        [DebuggerStepThrough]
        public override string ToString() => $"{Category.ToString()}.{Name.ToString()}";

        public bool Contains(Option<T> option) => Contains(option.Flag);

        public bool Contains(int flags) => (Flag & flags) == flags;

        public int CompareTo(Option<T> other) => Comparer.Compare(this, other);

        public int CompareTo(object other) => Comparer.Compare(this, other);

        #region IEquatable

        public bool Equals(Option<T> other) => AutoEquality<Option<T>>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as Option<T>);

        public override int GetHashCode() => AutoEquality<Option<T>>.Comparer.GetHashCode(this);

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

        [NotNull]
        public static T operator |(Option<T> left, Option<T> right)
        {
            var newFlag = left.Flag | right.Flag;
            if (TryGetKnownOption(newFlag, out var knownOption))
            {
                return (T)knownOption;
            }

            return Create(Unknown, newFlag);
        }

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

            public int Compare(object left, object right)
            {
                return Compare(left as Option<T>, right as Option<T>);
            }
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