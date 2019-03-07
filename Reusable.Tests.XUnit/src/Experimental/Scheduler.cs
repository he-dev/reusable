using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Linq.Custom;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Extensions;
using Xunit;

namespace Reusable.Tests.XUnit.Experimental
{
    public class SchedulerTest
    {
        [Fact]
        public async Task Go()
        {
            //var cronString = "5,10/5,15-30,45-50/2 * * 1-22 JAN,MAR-SEP MON-WED,4,FRI 2017";

            //CronExpression.Parse("0,10/5,5,45-50/2,15-30 * * 1-22   JAN,MAR-SEP * 2017").Dump();

            var timestamp = new DateTime(2017, 3, 1, 8, 0, 0);
            //	for (int i = 0; i < 120; i++)
            //	{
            //		//Console.WriteLine($"{timestamp.AddSeconds(i)} {cronExpr.Contains(timestamp.AddSeconds(i))}");	
            //	}

            // seconds minutes hours day-of-month month day-of-week year

            //CronExpression.Parse("* 0/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 0, 0)).Dump();
            //CronExpression.Parse("* 0/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 1, 0)).Dump();
            //CronExpression.Parse("* 0/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 5, 0)).Dump();

            //CronExpression.Parse("* 1/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 5, 0)).Dump();
            //CronExpression.Parse("* 1/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 6, 0)).Dump();
            //CronExpression.Parse("* 1/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 10, 0)).Dump();
            //CronExpression.Parse("* 1/5 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 11, 0)).Dump();
            //CronExpression.Parse("* 0/6 * * * * *").Dump().Contains(new DateTime(2017, 5, 1, 10, 0, 0)).Dump();

            //var startedOn = DateTime.UtcNow;

            //Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

            var scheduler = Scheduler.CreateUtc();

            //Thread.Sleep(3300);

            //	scheduler.Schedule("0/1 * * * * * *", async schedule =>
            //	{
            //		Log(1, schedule);
            //		await Task.Delay(1100);
            //	}, maxDegreeOfParallelism: Scheduler.UnlimitedJobParallelism);
            //
            //	scheduler.Schedule("0/2 * * * * * *", async schedule =>
            //	{
            //		Log(2, schedule);
            //		await Task.Delay(1500);
            //
            //	}, maxDegreeOfParallelism: Scheduler.UnlimitedJobParallelism);

            var j3 = scheduler.Schedule(new Job("Single-job", new CronTrigger("0/3 * * * * * *"), async token =>
            {
                //Log(3, DateTime.Now);
                await Task.Delay(4000);
                Console.WriteLine("Job-3: Finished!");
            })
            {
                MaxDegreeOfParallelism = 1
            });

            Console.WriteLine("Press ENTER to disconnect...");
            Console.ReadLine();
            j3.Dispose();
            scheduler.Dispose();
            //await scheduler.Continuation;
        }
    }

    public static class Tick
    {
        public static IObservable<DateTime> EverySecond(IDateTime dateTime)
        {
            return
                Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Select(_ => dateTime.Now());
        }
    }

    public static class ObservableExtensions
    {
        public static IObservable<DateTime> FixMissingSeconds(this IObservable<DateTime> tick, IDateTime dateTime)
        {
            var last = dateTime.Now().TruncateMilliseconds();

            return tick.SelectMany(_ =>
            {
                var now = dateTime.Now().TruncateMilliseconds();
                var gap = (now - last).Ticks / TimeSpan.TicksPerSecond;

                // If we missed one second due to time inaccuracy, 
                // this makes sure to publish the missing second too
                // so that all jobs at that second can also be triggered.
                return
                    Enumerable
                        .Range(0, (int)gap)
                        .Select(second => last = last.AddSeconds(1));
            });
        }
    }

    public class Scheduler : IDisposable
    {
        private readonly IConnectableObservable<DateTime> _scheduler;

        private readonly IDisposable _disconnect;

        public Scheduler(IObservable<DateTime> generator)
        {
            _scheduler =
                generator
                #if DEBUG
                    .Do(timestamp => Console.WriteLine($" Tick: {timestamp:yyyy-MM-dd HH:mm:ss.fff}"))
                    .Finally(() => Console.WriteLine("Disconnected!"))
                #endif
                    .Publish();
            //.RefCount(); // Not using this because of missing handling of restarts.
            _disconnect = _scheduler.Connect();
        }

        public static Scheduler CreateUtc()
        {
            var dateTime = new DateTimeUtc();
            var generator = Tick.EverySecond(dateTime).FixMissingSeconds(dateTime);
            return new Scheduler(generator);
        }

        public IDisposable Schedule(Job job, CancellationToken cancellationToken = default)
        {
            var unschedule =
                _scheduler
                    .Where(job.Trigger.Matches)
                    .Subscribe(timestamp => job.Execute(cancellationToken));

            return Disposable.Create(() =>
            {
                job.Continuation.Wait();
                unschedule.Dispose();
            });
        }

        public void Dispose()
        {
            // Stop ticking.
            _disconnect.Dispose();
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime TruncateMilliseconds(this DateTime dateTime)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond), dateTime.Kind);
        }
    }

    public static class SchedulerExtensions
    {
        public static IDisposable Schedule(this Scheduler scheduler, string cronExpression, Func<CancellationToken, Task> execute, int maxDegreeOfParallelism = 1, CancellationToken cancellationToken = default)
        {
            return scheduler.Schedule(new Job("Job", new CronTrigger(cronExpression), execute)
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            }, cancellationToken);
        }
    }

    public abstract class Trigger
    {
        public abstract bool Matches(DateTime tick);
    }

    public class CronTrigger : Trigger
    {
        private readonly CronExpression _cronExpression;

        public CronTrigger(string cronExpression)
        {
            _cronExpression = CronExpression.Parse(cronExpression);
        }

        public string Schedule => _cronExpression.ToString();

        public override bool Matches(DateTime tick)
        {
            return _cronExpression.Contains(tick);
        }
    }

    public class CountTrigger : Trigger
    {
        public CountTrigger(IInfiniteCounter counter)
        {
            Counter = counter;
        }

        public IInfiniteCounter Counter { get; }

        public override bool Matches(DateTime tick)
        {
            var (_, state) = Counter.Next();
            return state == InfiniteCounterState.First;
        }
    }

    public interface IInfiniteCounter : IEnumerable<(int Value, InfiniteCounterState State)>
    {
        int Min { get; }

        int Max { get; }

        int Length { get; }

        int Current { get; }

        (int Value, InfiniteCounterState State) Next();

        void Reset();
    }

    public class InfiniteCounter : IInfiniteCounter
    {
        private int _current;

        public InfiniteCounter(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public InfiniteCounter(int max) : this(0, max) { }

        public int Min { get; }

        public int Max { get; }

        public int Length => Max - Min;

        public int Current => _current + Min;

        private bool IsLast => _current == Length;

        public (int Value, InfiniteCounterState State) Next()
        {
            if (IsLast)
            {
                Reset();
            }

            return
            (
                Current,
                _current++ == 0
                    ? InfiniteCounterState.First
                    : IsLast
                        ? InfiniteCounterState.Last
                        : InfiniteCounterState.Intermediate
            );
        }

        public void Reset()
        {
            _current = 0;
        }

        public IEnumerator<(int Value, InfiniteCounterState State)> GetEnumerator()
        {
            while (true)
            {
                yield return Next();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public enum InfiniteCounterState
    {
        First,
        Intermediate,
        Last,
    }

    public class Job
    {
        private readonly List<Task> _tasks = new List<Task>();

        public const int UnlimitedParallelism = -1;

        public Job()
        { }

        public Job(string name, Trigger trigger, Func<CancellationToken, Task> executeAsync)
        {
            Name = name;
            Trigger = trigger;
            ExecuteAsync = executeAsync;
        }

        public string Name { get; }

        public Trigger Trigger { get; }

        public Func<CancellationToken, Task> ExecuteAsync { get; }

        //public Action<Job> Misfire { get; set; }

        public int MaxDegreeOfParallelism { get; set; } = 1;

        public Task Continuation => Task.WhenAll(_tasks);

        internal void Execute(CancellationToken cancellationToken)
        {
            if (CanExecute())
            {
                var jobTask = ExecuteAsync(cancellationToken);
                _tasks.Add(jobTask);
                jobTask.ContinueWith(_ => _tasks.Remove(jobTask), cancellationToken);
            }
            else
            {
                //$"'{Name}' is alredy running!".Dump();
            }
        }

        public bool IsRunning(out int count)
        {
            return (count = _tasks.Count) > 0;
        }

        private bool CanExecute()
        {
            return
                MaxDegreeOfParallelism == UnlimitedParallelism ||
                IsRunning(out var taskCount) == false ||
                MaxDegreeOfParallelism > taskCount;
        }
    }

    public enum CronExtension
    {
        None,
        First,
        Second,
        Third,
        Fourth,
        Fifth,
        Last,
        Weekday,
        L = Last,
        W = Weekday
    }

    public interface ICronField
    {
        bool Contains(DateTime dateTime);
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class CronField : ICronField
    {
        private const string Pattern = @"(?<min>{fieldPattern})(?:-(?<max>{fieldPattern}))?(?:\/(?<step>\d+))?((?:#(?<nth>[1-5]))|(?<ext>L|W))?";

        protected CronField(int? min, int? max, int? step, CronExtension extension)
        {
            var checkRange = min.HasValue && max.HasValue;
            if (checkRange)
            {
                var isValidRange = min.Value < max.Value;
                if (!isValidRange)
                {
                    throw new ArgumentException($"{nameof(min)} must be less then {nameof(max)}.");
                }
            }
            Min = min;
            Max = max;
            Step = step;
            Extension = extension;
        }

        public static CronField Any = new CronAny();

        public int? Min { get; }

        public int? Max { get; }

        public int? Step { get; }

        public CronExtension? Extension { get; }

        private string DebuggerDisplay => ToString();

        private static readonly ConcurrentDictionary<Type, string> FieldPatternCache = new ConcurrentDictionary<Type, string>();

        internal static bool TryParse<T>(string input, out CronField cronField) where T : CronField
        {
            if (input == Any)
            {
                cronField = Any;
                return true;
            }

            var fieldValues = DuckObject<T>.Quack<IReadOnlyDictionary<string, int>>(duck => duck.ValueMap);
            var fieldPattern = FieldPatternCache.GetOrAdd(typeof(T), t => fieldValues.Select(x => Regex.Escape(x.Key)).OrderByDescending(x => x).Join("|"));

            var match = Regex.Match(input, Pattern.Format(new { fieldPattern }), RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var min = default(int?);
                var max = default(int?);
                var step = default(int?);
                var extension = CronExtension.None;

                if (fieldValues.TryGetValue(match.Groups["min"].Value, out var _min))
                {
                    min = _min;
                }
                else
                {
                    cronField = default;
                    return false;
                }

                if (match.Groups["max"].Success && fieldValues.TryGetValue(match.Groups["max"].Value, out var _max))
                {
                    max = _max;
                }

                if (match.Groups["step"].Success && int.TryParse(match.Groups["step"].Value, out var _step))
                {
                    step = _step;
                }

                if (match.Groups["ext"].Success && Enum.TryParse<CronExtension>(match.Groups["ext"].Value, out var ext))
                { }

                cronField = (T)Activator.CreateInstance(typeof(T), min, max, step, extension);
                return true;
            }

            cronField = default;
            return false;
        }

        protected abstract int GetValue(DateTime point);

        public virtual bool Contains(DateTime timestamp)
        {
            var value = GetValue(timestamp);

            var isExact = Min.HasValue && !Max.HasValue && !Step.HasValue;
            if (isExact && value != Min.Value)
            {
                return false;
            }

            var isRange = Min.HasValue && Max.HasValue;

            if (isRange && !(value > Min.Value && value < Max.Value))
            {
                return false;
            }

            if (value > Min.Value)
            {
                value -= Min.Value;
            }

            if (Step.HasValue && value % Step.Value != 0)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            var min = Min;
            var max = Max.HasValue && Max > Min ? $"-{Max}" : string.Empty;
            var step = Step.HasValue ? $"/{Step}" : string.Empty;

            return $"{min}{max}{step}";
        }

        public static implicit operator string(CronField cronField) => cronField?.ToString();
    }

    public static class CronExpressionExtensions
    {
        // Replaces multiple whitespaces by a single one, trims the string and makes it uppercase.
        public static string NormalizeCronExpression(this string input)
        {
            return Regex.Replace(input, @"\s+", " ").Trim().ToUpper();
        }
    }

    public class CronAny : CronField
    {
        public CronAny()
            : base(null, null, null, CronExtension.None)
        { }

        protected override int GetValue(DateTime point) => -1;

        public override bool Contains(DateTime dateTime) => true;

        public override string ToString() => "*";
    }

    public class CronSecond : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronSecond()
        {
            ValueMap =
                Enumerable
                    .Range(0, 60)
                    .ToDictionary(x => x.ToString(), x => x);
        }

        public CronSecond(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension)
        { }

        protected override int GetValue(DateTime point) => point.Second;
    }

    public class CronMinute : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronMinute()
        {
            ValueMap =
                Enumerable
                    .Range(0, 60)
                    .ToDictionary(x => x.ToString(), x => x);
        }

        public CronMinute(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension)
        { }

        protected override int GetValue(DateTime point) => point.Minute;
    }

    public class CronHour : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronHour()
        {
            ValueMap =
                Enumerable
                    .Range(0, 24)
                    .ToDictionary(x => x.ToString(), x => x);
        }

        public CronHour(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension)
        { }

        protected override int GetValue(DateTime point) => point.Hour;
    }

    public class CronDayOfMonth : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronDayOfMonth()
        {
            ValueMap =
                Enumerable
                    .Range(1, 31)
                    .ToDictionary(x => x.ToString(), x => x);
        }

        public CronDayOfMonth(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension)
        { }

        protected override int GetValue(DateTime point) => point.Day;
    }

    public class CronMonth : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronMonth()
        {
            ValueMap =
                Enumerable
                    .Range(0, 12)
                    .Select(x => (key: x.ToString(), value: x))
                    .Concat(new[]
                        {
                            "JAN", "FEB", "MAR", "APR", "MAI", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"
                        }
                        .Select((dayOfWeek, index) => (key: dayOfWeek, value: index)))
                    .ToDictionary(x => x.key, x => x.value + 1, StringComparer.OrdinalIgnoreCase);
        }

        public CronMonth(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension)
        { }

        protected override int GetValue(DateTime point) => point.Month;
    }

    public class CronDayOfWeek : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronDayOfWeek()
        {
            ValueMap =
                Enumerable
                    .Range(1, 7)
                    .Select(x => (key: x.ToString(), value: x))
                    .Concat(new[]
                        {
                            "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"
                        }
                        .Select((dayOfWeek, index) => (key: dayOfWeek, value: index)))
                    .ToDictionary(x => x.key, x => x.value + 1, StringComparer.OrdinalIgnoreCase);
        }

        public CronDayOfWeek(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension)
        { }

        protected override int GetValue(DateTime point) => (int)point.DayOfWeek + 1;
    }

    public class CronYear : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronYear()
        {
            ValueMap =
                Enumerable
                    .Range(1970, 130)
                    .ToDictionary(x => x.ToString(), x => x);
        }

        public CronYear(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension)
        { }

        protected override int GetValue(DateTime point) => point.Year;
    }

    internal delegate bool TryParseFunc(string value, out CronField result);

    public class CronExpression : IEnumerable<IEnumerable<CronField>>
    {
        private static readonly IEnumerable<TryParseFunc> TryParseCronFields = new TryParseFunc[]
        {
            CronField.TryParse<CronSecond>,
            CronField.TryParse<CronMinute>,
            CronField.TryParse<CronHour>,
            CronField.TryParse<CronDayOfMonth>,
            CronField.TryParse<CronMonth>,
            CronField.TryParse<CronDayOfWeek>,
            CronField.TryParse<CronYear>,
        };

        private readonly IEnumerable<IEnumerable<CronField>> _cronFields;

        private CronExpression(IEnumerable<IEnumerable<CronField>> cronFields)
        {
            _cronFields = cronFields;
        }

        public static bool TryParse(string input, out CronExpression result)
        {
            var cronFields =
                input
                    .NormalizeCronExpression()
                    .Split(' ')
                    .Select(x => x.Split(','))
                    .Zip(
                        TryParseCronFields,
                        (fieldValues, tryParseCronField) => fieldValues
                            .Select(value =>
                                tryParseCronField(value, out var cronField)
                                    ? cronField
                                    : default
                            )
                    )
                    .Select(g => g
                                 .Where(Conditional.IsNotNull)
                                 .OrderBy(x => x.Min.HasValue ? x.Min.Value : 0)
                                 .ThenBy(x => x.Max.HasValue ? x.Max.Value : 0)
                                 .ThenBy(x => x.Step.HasValue ? x.Step.Value : 0)
                                 .ToList()
                    )
                    .ToList();

            if (cronFields.Any())
            {
                result = new CronExpression(cronFields);
                return true;
            }

            result = default;
            return false;
        }

        public static CronExpression Parse(string input)
        {
            return TryParse(input, out var result) ? result : throw new ArgumentException("Invalid Cron-Expression.");
        }

        public bool Contains(DateTime timestamp) => this.All(x => x.Any(f => f.Contains(timestamp)));

        public IEnumerator<IEnumerable<CronField>> GetEnumerator() => _cronFields.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => this.Select(g => g.Select(f => f.ToString()).Join(",")).Join(" ");

        // LINQPad
        private object ToDump() => ToString();
    }

    // -----------------------

    public class DuckObject<T> : DynamicObject
    {
        private static readonly DuckObject<T> Duck = new DuckObject<T>();

        public static TValue Quack<TValue>(Func<dynamic, dynamic> quack)
        {
            return (TValue)quack(Duck);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            throw new InvalidOperationException($"Cannot use an indexer on '{typeof(T)}' because static types do not have them.");
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var member = typeof(T).GetMember(binder.Name).SingleOrDefault();
            switch (member?.MemberType)
            {
                case MemberTypes.Field:
                    result = typeof(T).InvokeMember(binder.Name, BindingFlags.GetField, null, null, null);
                    break;
                case MemberTypes.Property:
                    result = typeof(T).InvokeMember(binder.Name, BindingFlags.GetProperty, null, null, null);
                    break;
                default:
                    throw new StaticMemberNotFoundException<T>(binder.Name);
            }
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var member = typeof(T).GetMember(binder.Name).SingleOrDefault();
            switch (member?.MemberType)
            {
                case MemberTypes.Method:
                    result = typeof(T).InvokeMember(binder.Name, BindingFlags.InvokeMethod, null, null, args);
                    break;
                default:
                    throw new StaticMemberNotFoundException<T>(binder.Name);
            }
            return true;
        }
    }

    public class DuckObject
    {
        private static readonly ConcurrentDictionary<Type, dynamic> Cache = new ConcurrentDictionary<Type, dynamic>();

        public static TValue Quack<TValue>(Type type, Func<dynamic, dynamic> quack)
        {
            var duck = Cache.GetOrAdd(type, t => Activator.CreateInstance(typeof(DuckObject<>).MakeGenericType(type)));
            return (TValue)quack(duck);
        }
    }

    public class StaticMemberNotFoundException<T> : Exception
    {
        public StaticMemberNotFoundException(string missingMemberName)
            : base($"Type '{typeof(T)}' does not contain static member '{missingMemberName}'")
        { }
    }
}