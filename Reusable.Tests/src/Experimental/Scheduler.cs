using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xunit;

namespace Reusable.Tests.Experimental
{
    public class CronExpressionTest
    {
        [Fact]
        public void Can_parse_simple_fields()
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
        private static readonly DateTime Nowhere = DateTime.MinValue;

        public static IObservable<DateTime> TruncateMilliseconds(this IObservable<DateTime> ticks)
        {
            return ticks.Select(DateTimeExtensions.TruncateMilliseconds);
        }

        public static IObservable<DateTime> FixMissingSeconds(this IObservable<DateTime> ticks)
        {
            var last = Nowhere;

            return ticks.SelectMany(tick =>
            {
                if (tick.Millisecond > 0) throw new InvalidOperationException($"{nameof(FixMissingSeconds)} requires ticks without the millisecond part.");

                // We have to start somewhere so let it be one second before tick if we are currently nowhere.
                last = last == Nowhere ? tick.AddSeconds(-1) : last;

                // Calculates the gap between tick and last. In normal case it's 1.
                var gap = tick.DiffInSeconds(last);

                // If we missed one second due to time inaccuracy, 
                // this makes sure to publish the missing second too
                // so that all jobs at that second can also be triggered.
                return
                    Enumerable
                        .Range(0, gap)
                        .Select(_ => last = last.AddSeconds(1));
            });
        }
    }

//                #if DEBUG
//                 ticks
//                    .Do(timestamp => Console.WriteLine($" Tick: {timestamp:yyyy-MM-dd HH:mm:ss.fff}"))
//                    .Finally(() => Console.WriteLine("Disconnected!"))
//                #endif

    public class Scheduler : IDisposable
    {
        private readonly IConnectableObservable<DateTime> _scheduler;

        private readonly IDisposable _disconnect;

        public Scheduler(IObservable<DateTime> ticks)
        {
            // Not using .RefCount here because it should be ticking regardless of subscriptions.
            _scheduler = ticks.Publish();
            _disconnect = _scheduler.Connect();
        }

        public IDisposable Schedule(Job job, CancellationToken cancellationToken = default)
        {
            var unschedule =
                _scheduler
                    // .ToList the results so that all triggers have the chance to evaluate the tick.
                    .Select(tick => job.Triggers.Where(t => t.Matches(tick)).ToList())
                    //.Where(tick => job.Triggers.Where(t => t.Matches(tick)).ToList().Any())
                    .Where(triggers => triggers.Any())
                    .Subscribe(_ => job.Execute(cancellationToken));

            return Disposable.Create(() =>
            {
                job.Continuation.Wait(job.UnscheduleTimeout);
                unschedule.Dispose();
            });
        }

        public void Dispose()
        {
            // Stop ticking.
            _disconnect.Dispose();
        }
    }

    public static class SchedulerFactory
    {
        [NotNull]
        public static Scheduler CreateScheduler([NotNull] IDateTime dateTime)
        {
            if (dateTime == null) throw new ArgumentNullException(nameof(dateTime));

            var ticks = Tick.EverySecond(dateTime).TruncateMilliseconds().FixMissingSeconds();
            return new Scheduler(ticks);
        }
    }

    public static class SchedulerExtensions
    {
        public static IDisposable Schedule
        (
            this Scheduler scheduler,
            string name,
            Func<CancellationToken, Task> action,
            Trigger trigger,
            Action<Job> configureJob = default,
            CancellationToken cancellationToken = default
        )
        {
            var job = new Job(name, new[] { trigger }, action)
            {
                MaxDegreeOfParallelism = 1
            };
            configureJob?.Invoke(job);
            return scheduler.Schedule(job, cancellationToken);
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime TruncateMilliseconds(this DateTime dateTime)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond), dateTime.Kind);
        }

        public static int Diff(this DateTime later, DateTime earlier, long ticksPerX)
        {
            if (later < earlier) throw new ArgumentException($"'{nameof(later)}' must be greater or equal '{nameof(earlier)}'.");

            return (int)((later - earlier).Ticks / ticksPerX);
        }

        public static int DiffInSeconds(this DateTime later, DateTime earlier)
        {
            return later.Diff(earlier, TimeSpan.TicksPerSecond);
        }
    }


    // Let it be a class because it'll get more properties later.
    public abstract class Trigger
    {
        public static IEnumerable<Trigger> Empty => Enumerable.Empty<Trigger>();

        public abstract bool Matches(DateTime tick);
        
        //public bool 
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
        private readonly int _count;
        
        private int _counter;

        public CountTrigger(int count)
        {
            _count = count;
        }

        public override bool Matches(DateTime tick)
        {
            if (_counter < _count)
            {
                _counter++;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    
    public class EveryTrigger : Trigger
    {
        private readonly ICounter _counter;

        public EveryTrigger(int count)
        {
            _counter = new InfiniteCounter(count);
        }

        public int Every => _counter.Length;

        public override bool Matches(DateTime tick)
        {
            return _counter.MoveNext() && _counter.Position == CounterPosition.Last;
        }
    }

    public class DegreeOfParallelism : Primitive<int>
    {
        private const int UnlimitedValue = -1;

        public DegreeOfParallelism(int value) : base(value) { }

        public static readonly DegreeOfParallelism Unlimited = new DegreeOfParallelism(UnlimitedValue);

        protected override void Validate(int value)
        {
            if (value == UnlimitedValue)
            {
                return;
            }

            if (value < 1)
            {
                throw new ArgumentException("Value must be positive.");
            }
        }

        public static implicit operator DegreeOfParallelism(int value) => new DegreeOfParallelism(value);
    }

    public class Job
    {
        private readonly List<Task> _tasks = new List<Task>();

        public Job(string name, IEnumerable<Trigger> trigger, Func<CancellationToken, Task> action)
        {
            Name = name;
            Triggers = trigger.ToList();
            Action = action;
        }

        public string Name { get; }

        public IEnumerable<Trigger> Triggers { get; }

        public Func<CancellationToken, Task> Action { get; }

        public Action<Job> OnMisfire { get; set; }

        public DegreeOfParallelism MaxDegreeOfParallelism { get; set; } = 1;

        public TimeSpan UnscheduleTimeout { get; set; }

        public Task Continuation => Task.WhenAll(_tasks).ContinueWith(_ => _tasks.Clear());

        public int Count => _tasks.Count;

        public bool Execute(CancellationToken cancellationToken)
        {
            if (CanExecute())
            {
                var jobTask = Action(cancellationToken);
                _tasks.Add(jobTask);
                jobTask.ContinueWith(_ => _tasks.Remove(jobTask), cancellationToken);
                return true;
            }
            else
            {
                OnMisfire?.Invoke(this);
                return false;
            }
        }

        private bool CanExecute()
        {
            return
                MaxDegreeOfParallelism.Equals(DegreeOfParallelism.Unlimited) ||
                Count < MaxDegreeOfParallelism;
        }
    }


    public class StaticMemberNotFoundException<T> : Exception
    {
        public StaticMemberNotFoundException(string missingMemberName)
            : base($"Type '{typeof(T)}' does not contain static member '{missingMemberName}'") { }
    }

    public class PrimitiveTest
    {
        [Fact]
        public void Supports_equality()
        {
            var userName = new UserName("Bob");
            Assert.Equal("Bob", userName);
        }

        [Fact]
        public void Throws_when_invalid_value()
        {
            Assert.ThrowsAny<Exception>(() => new UserName(null));
        }

        private class UserName : Primitive<string>
        {
            public UserName(string value) : base(value) { }

            protected override void Validate(string value)
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentException("Value must not be null or empty.");
            }
        }
    }

    public class JobTest
    {
        [Fact]
        public async Task Job_executes_no_more_than_specified_number_of_times()
        {
            var misfireCount = 0;
            var job = new Job("test", Enumerable.Empty<Trigger>(), async token => await Task.Delay(TimeSpan.FromSeconds(3), token))
            {
                OnMisfire = j => misfireCount++,
                MaxDegreeOfParallelism = 2
            };
            job.Execute(CancellationToken.None);
            job.Execute(CancellationToken.None);
            job.Execute(CancellationToken.None);
            Assert.Equal(2, job.Count);
            Assert.Equal(1, misfireCount);

            // Wait until all jobs are completed.
            await job.Continuation;

            Assert.Equal(0, job.Count);
        }
    }

    public class SchedulerTest
    {
        [Fact]
        public void Executes_job_according_to_triggers()
        {
            var job1ExecuteCount = 0;
            var job2ExecuteCount = 0;
            var misfireCount = 0;
            var subject = new Subject<DateTime>();
            var scheduler = new Scheduler(subject);

            var unschedule1 = scheduler.Schedule(new Job("test-1", new[] { new EveryTrigger(2),  }, async token =>
            {
                Interlocked.Increment(ref job1ExecuteCount);
                await Task.Delay(TimeSpan.FromSeconds(3), token);
            })
            {
                MaxDegreeOfParallelism = 2,
                OnMisfire = _ => Interlocked.Increment(ref misfireCount),
                UnscheduleTimeout = TimeSpan.FromSeconds(4)
            });

            var unschedule2 = scheduler.Schedule(new Job("test-2", new[] { new EveryTrigger(3) }, async token =>
            {
                Interlocked.Increment(ref job2ExecuteCount);
                await Task.Delay(TimeSpan.FromSeconds(3), token);
            })
            {
                MaxDegreeOfParallelism = 2,
                OnMisfire = _ => Interlocked.Increment(ref misfireCount),
                UnscheduleTimeout = TimeSpan.FromSeconds(4)
            });

            // Scheduler was just initialized and should not have executed anything yet.
            Assert.Equal(0, job1ExecuteCount);
            Assert.Equal(0, job2ExecuteCount);

            // Tick once.
            subject.OnNext(DateTime.Now);

            // Still nothing should be executed.
            Assert.Equal(0, job1ExecuteCount);
            Assert.Equal(0, job2ExecuteCount);

            // Now tick twice...
            subject.OnNext(DateTime.Now);
            subject.OnNext(DateTime.Now);

            // Unschedule the job. This blocking call waits until all tasks are completed.
            unschedule1.Dispose();
            unschedule2.Dispose();

            // Tick once again. Nothing should be executed anymore.
            subject.OnNext(DateTime.Now);

            // ...this should have matched the two triggers.
            Assert.Equal(1, job1ExecuteCount);
            Assert.Equal(1, job2ExecuteCount);

            Assert.Equal(0, misfireCount);
        }
    }

    public class ObservableExtensionsTest
    {
        [Fact]
        public void Returns_ticks_unchanged_when_no_gap()
        {
            var ticks = new[] { 0, 1, 2 }.Select(s => DateTime.Parse($"2019-01-01 10:00:0{s}")).ToList();
            Assert.Equal(ticks, ticks.ToObservable().TruncateMilliseconds().FixMissingSeconds().ToEnumerable().ToList());
        }

        [Fact]
        public void Fixes_tick_gap()
        {
            var expected = new[] { 0, 1, 2 }.Select(s => DateTime.Parse($"2019-01-01 10:00:0{s}")).ToList();
            var missing = new[] { 0, 2 }.Select(s => DateTime.Parse($"2019-01-01 10:00:0{s}")).ToList();
            Assert.Equal(expected.ToList(), missing.ToObservable().TruncateMilliseconds().FixMissingSeconds().ToEnumerable().ToList());
        }
    }
}