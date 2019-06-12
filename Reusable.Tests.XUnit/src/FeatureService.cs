using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Bcpg.Sig;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Tests.XUnit.Features;
using Xunit;

namespace Reusable.Tests.XUnit
{
    //using static FeatureOptions;

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
        public static IFeatureService Configure(this IFeatureService features, IEnumerable<string> names, Func<FeatureOption, FeatureOption> configure)
        {
            foreach (var name in names)
            {
                features.Configure(name, configure);
            }

            return features;
        }

        public static IEnumerable<string> Keys(this IEnumerable<FeatureInfo> features)
        {
            return
                from t in features
                // () => x.Member
                let l =
                    Expression.Lambda(
                        Expression.Property(
                            Expression.Constant(null, t.Category),
                            t.Property.Name
                        )
                    )
                select l.GetKeys().Join(string.Empty);
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

    // [Flags]
    // public enum FeatureOptions
    // {
    //     None = 0,
    //
    //     /// <summary>
    //     /// When set a feature is enabled.
    //     /// </summary>
    //     Enabled = 1 << 0,
    //
    //     /// <summary>
    //     /// When set a warning is logged when a feature is toggled.
    //     /// </summary>
    //     Warn = 1 << 1,
    //
    //     /// <summary>
    //     /// When set feature usage statistics are logged.
    //     /// </summary>
    //     Telemetry = 1 << 2, // For future use
    // }

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
            var features = new FeatureService
            (
                Logger<FeatureService>.Null,
                defaultOptions: FeatureOption.Enable | FeatureOption.Warn | FeatureOption.Telemetry
            );

            var names = FeatureCollection.Empty.Add<IDemo>().Add<IDatabase>().WhereTags("io").Keys();

            features.Configure(names, o => o ^ FeatureOption.Enable);

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
        private readonly FeatureService _features = new FeatureService
        (
            Logger<FeatureService>.Null,
            defaultOptions: FeatureOption.Enable | FeatureOption.Warn | FeatureOption.Telemetry
        );

        public async Task Start()
        {
            SayHallo();

            //_features.Configure(nameof(SayHallo), o => o.Reset(FeatureOption.Enable));
            //_features.Configure(Use<IDemo>.Namespace, x => x.Greeting, o => o ^ Enabled);
            _features.Configure(From<IDemo>.Select(x => x.Greeting).Index("Morning"), o => o.Reset(FeatureOption.Enable));

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
        [UseType, UseMember]
        [TrimStart("I")]
        public interface IDemo : INamespace
        {
            object Greeting { get; }

            [Tag("io")]
            object ReadFile { get; }
        }

        [UseType, UseMember]
        [TrimStart("I")]
        public interface IDatabase : INamespace
        {
            [Tag("io")]
            object Commit { get; }
        }
    }

    [PublicAPI]
    public abstract class Option
    {
        protected const string Unknown = nameof(Unknown);

        protected static readonly OptionComparer Comparer = new OptionComparer();

        public static readonly IImmutableList<SoftString> ReservedNames =
            ImmutableList<SoftString>
                .Empty
                .Add(nameof(Option<Option>.None))
                .Add(nameof(Option<Option>.All))
                .Add(nameof(Option<Option>.Max));

        // Disallow anyone else to use this class.
        // This way we can guarantee that it is used only by the Option<T>.
        private protected Option() { }

        [NotNull]
        public abstract SoftString Name { get; }

        public abstract int Flag { get; }

        /// <summary>
        /// Returns True if Option is power of two.
        /// </summary>
        public abstract bool IsBit { get; }

        protected class OptionComparer : IComparer<Option>, IComparer
        {
            public int Compare(Option left, Option right)
            {
                if (ReferenceEquals(left, right)) return 0;
                if (ReferenceEquals(left, null)) return 1;
                if (ReferenceEquals(right, null)) return -1;
                return left.Flag - right.Flag;
            }

            public int Compare(object left, object right)
            {
                return Compare(left as Option, right as Option);
            }
        }
    }

    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class Option<T> : Option, IEquatable<Option<T>>, IComparable<Option<T>>, IComparable, IFormattable where T : Option
    {
        // ReSharper disable once StaticMemberInGenericType - this is correct
        private static readonly ConstructorInfo Constructor;

        private static IImmutableSet<T> _options;

        static Option()
        {
            Constructor =
                typeof(T).GetConstructor(new[] { typeof(SoftString), typeof(int) })
                ?? throw DynamicException.Create
                (
                    "ConstructorNotFound",
                    $"{typeof(T).ToPrettyString()} must provide a constructor with the following signature: " +
                    $"ctor({typeof(SoftString).ToPrettyString()}, {typeof(int).ToPrettyString()})"
                );

            // Always initialize "None".
            _options = ImmutableSortedSet<T>.Empty.Add(Create(nameof(None), 0));
        }

        protected Option(SoftString name, int flag)
        {
            if (GetType() != typeof(T)) throw DynamicException.Create("OptionTypeMismatch", "Option must be a type of itself.");

            Name = name;
            Flag = flag;
        }

        #region Default options

        [NotNull]
        public static T None => _options.First();

        [NotNull]
        public static T Max => _options.Last();

        [NotNull]
        public static IEnumerable<T> All => _options;

        #endregion

        [NotNull, ItemNotNull]
        public static IEnumerable<T> Bits => _options.Where(o => o.IsBit);

        #region Option

        public override SoftString Name { [DebuggerStepThrough] get; }

        [AutoEqualityProperty]
        public override int Flag { [DebuggerStepThrough] get; }

        public override bool IsBit => (Flag & (Flag - 1)) == 0;

        #endregion

        [NotNull, ItemNotNull]
        public IEnumerable<T> Flags => Bits.Where(f => (Flag & f.Flag) > 0);

        #region Factories

        [NotNull]
        public static T Create(SoftString name, T option = default)
        {
            if (name.In(_options.Select(o => o.Name).Concat(ReservedNames)))
            {
                throw DynamicException.Create("DuplicateOption", $"The option '{name}' is defined more the once.");
            }

            var bitCount = _options.Count(o => o.IsBit);
            var newOption = Create(name, bitCount == 1 ? 1 : (bitCount - 1) << 1);
            _options = _options.Add(newOption);

            return newOption;
        }

        [NotNull]
        public static T CreateWithCallerName(T option = default, [CallerMemberName] string name = default)
        {
            return Create(name, option);
        }

        private static T Create(SoftString name, IEnumerable<int> flags)
        {
            var flag = flags.Aggregate(0, (current, next) => current | next);
            return (T)Constructor.Invoke(new object[] { name, flag });
        }

        public static T Create(SoftString name, params int[] flags)
        {
            return Create(name, flags.AsEnumerable());
        }

        [NotNull]
        public static T FromName([NotNull] string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            return
                _options.FirstOrDefault(o => o.Name == value)
                ?? throw DynamicException.Create("OptionOutOfRange", $"There is no such option as '{value}'.");
        }

        [NotNull]
        public static T FromValue(int value)
        {
            if (value > Max.Flag)
            {
                throw new ArgumentOutOfRangeException(paramName: nameof(value), $"Value {value} is greater than the highest option.");
            }

            // Is this a known value?
            if (TryGetKnownOption(value, out var knownOption))
            {
                return knownOption;
            }

            var newFlags = Bits.Where(o => (o.Flag & value) == o.Flag).Select(o => o.Flag);
            return Create(Unknown, newFlags);
        }

        private static bool TryGetKnownOption(int flag, out T option)
        {
            if (_options.SingleOrDefault(o => o.Flag == flag) is var knownOption && !(knownOption is null))
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

        public T Set(Option<T> option) => this | option;

        public T Reset(Option<T> option) => this ^ option;

        [DebuggerStepThrough]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (SoftString.Comparer.Equals(format, "names"))
            {
                return Flags.Select(o => $"{o.Name.ToString()}").Join(", ");
            }

            if (SoftString.Comparer.Equals(format, "flags"))
            {
                return Flags.Select(o => $"{o.Flag}").Join(", ");
            }

            if (SoftString.Comparer.Equals(format, "names+flags"))
            {
                return Flags.Select(o => $"{o.Name.ToString()}[{o.Flag}]").Join(", ");
            }

            return ToString();
        }

        public override string ToString() => $"{this:names}";

        public bool Contains(T option) => Contains(option.Flag);

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
        public static T operator |(Option<T> left, Option<T> right) => GetKnownOrCreate(left.Flag | right.Flag);

        [NotNull]
        public static T operator ^(Option<T> left, Option<T> right) => GetKnownOrCreate(left.Flag ^ right.Flag);

        private static T GetKnownOrCreate(int flag)
        {
            return
                TryGetKnownOption(flag, out var knownOption)
                    ? knownOption
                    : Create(Unknown, flag);
        }

        #endregion
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
            var enableWarn = FeatureOption.Enable | FeatureOption.Warn;
            Assert.Equal(enableWarn, fromValue);

            var names = $"{enableWarn:names}";
            var flags = $"{enableWarn:flags}";
            var namesAndFlags = $"{enableWarn:names+flags}";
            var @default = $"{enableWarn}";

            Assert.True(FeatureOption.None < FeatureOption.Enable);
            Assert.True(FeatureOption.Enable < FeatureOption.Telemetry);

            Assert.Throws<ArgumentOutOfRangeException>(() => FeatureOption.FromValue(1000));
            //Assert.ThrowsAny<DynamicException>(() => FeatureOption.Create("All", 111111));
        }
    }

//    public class UriKey<T> : Key<T, UriString>
//    {
//        public override UriString Value => throw new NotImplementedException();
//    }
}