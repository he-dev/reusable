using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Cryptography.Extensions;
using Reusable.MQLite;
using Reusable.MQLite.Models;


// ReSharper disable InconsistentNaming - it's test code so naming doesn't matter.

namespace Reusable.Tests.MQLite
{
    [TestClass]
    public class MessageRepositoryTest
    {
        private const string ConnectonString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";

        private static class QueueNames
        {
            public const string TestQueue1 = nameof(TestQueue1);
            public const string TestQueue2 = nameof(TestQueue2);
            public const string ControlQueue = nameof(ControlQueue);

            public static IEnumerable<string> Enumerate()
            {
                yield return TestQueue1;
                yield return TestQueue2;
                yield return ControlQueue;
            }
        }

        private static class TimeRanges
        {
            public static readonly Range<DateTime> Range_20550101_20550102 = new Range<DateTime>(min: (2055, 1, 1).ToDate().AtNoon(), max: (2055, 1, 2).ToDate().AtNoon());
            public static readonly Range<DateTime> Range_20550102_20550103 = new Range<DateTime>(min: (2055, 1, 2).ToDate().AtNoon(), max: (2055, 1, 3).ToDate().AtNoon());
            public static readonly Range<DateTime> Range_20550104_20550105 = new Range<DateTime>(min: (2055, 1, 4).ToDate().AtNoon(), max: (2055, 1, 5).ToDate().AtNoon());
        }

        private IMessageRepository _repository;

        [TestInitialize]
        public async Task TestInitialize()
        {
            _repository = new MessageRepository(ConnectonString);

            // make sure we're starting with empty queues
            foreach (var queueName in QueueNames.Enumerate())
            {
                var removedTimeRanges1 = await _repository.RemoveTimeRangesAsync(queueName, null, null, CancellationToken.None);
                Assert.AreEqual(0, await _repository.GetTimeRangeCountAsync(queueName, CancellationToken.None));
                Assert.AreEqual(0, await _repository.GetMessageCountAsync(queueName, CancellationToken.None));
            }
        }

        [TestMethod]
        public async Task FullSimulation()
        {
            _repository.EnqueueAsync(QueueNames.TestQueue1, )
            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task Enqueue_EmptyRange_Allowed_1()
        {
            var enqueueCount =
                await _repository.EnqueueAsync(
                    QueueNames.TestQueue1,
                    TimeRanges.Range_20550101_20550102,
                    new List<NewMessage>(),
                    EnqueueOptions.AllowEmptyTimeRange,
                    CancellationToken.None
                );

            Assert.AreEqual(1, enqueueCount);
            Assert.AreEqual(1, await _repository.GetTimeRangeCountAsync(QueueNames.TestQueue1, CancellationToken.None));
            Assert.AreEqual(0, await _repository.GetTimeRangeCountAsync(QueueNames.TestQueue2, CancellationToken.None));
            Assert.AreEqual(0, await _repository.GetTimeRangeCountAsync(QueueNames.ControlQueue, CancellationToken.None));
        }

        [TestMethod]
        public async Task Enqueue_EmptyRange_NotAllowed_0()
        {
            var enqueueCount =
                await _repository.EnqueueAsync(
                    QueueNames.TestQueue1,
                    TimeRanges.Range_20550101_20550102,
                    new List<NewMessage>(),
                    EnqueueOptions.None,
                    CancellationToken.None
                );

            Assert.AreEqual(0, enqueueCount);
            Assert.AreEqual(0, await _repository.GetTimeRangeCountAsync(QueueNames.TestQueue1, CancellationToken.None));
            Assert.AreEqual(0, await _repository.GetTimeRangeCountAsync(QueueNames.TestQueue2, CancellationToken.None));
            Assert.AreEqual(0, await _repository.GetTimeRangeCountAsync(QueueNames.ControlQueue, CancellationToken.None));
        }

        [TestMethod]
        public void GetMessageCountAsync_UnchangedQueues_140()
        {
            Assert.AreEqual(1, _repository.GetMessageCountAsync(QueueNames.TestQueue1, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(4, _repository.GetMessageCountAsync(QueueNames.TestQueue2, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(0, _repository.GetMessageCountAsync(QueueNames.ControlQueue, CancellationToken.None).GetAwaiter().GetResult());
        }

        [TestMethod]
        public void PeekTimeRangeAsync_UnchangedQueues_120()
        {
            Assert.AreEqual(1, _repository.GetLastTimeRangeAsync(QueueNames.TestQueue1, 10, CancellationToken.None).GetAwaiter().GetResult().Count);
            Assert.AreEqual(2, _repository.GetLastTimeRangeAsync(QueueNames.TestQueue2, 10, CancellationToken.None).GetAwaiter().GetResult().Count);
            Assert.AreEqual(0, _repository.GetLastTimeRangeAsync(QueueNames.ControlQueue, 10, CancellationToken.None).GetAwaiter().GetResult().Count);
        }

        [TestMethod]
        public void PeekTimeRangeAsync_MaxCount_110()
        {
            Assert.AreEqual(1, _repository.GetLastTimeRangeAsync(QueueNames.TestQueue1, 1, CancellationToken.None).GetAwaiter().GetResult().Count);
            Assert.AreEqual(1, _repository.GetLastTimeRangeAsync(QueueNames.TestQueue2, 1, CancellationToken.None).GetAwaiter().GetResult().Count);
            Assert.AreEqual(0, _repository.GetLastTimeRangeAsync(QueueNames.ControlQueue, 1, CancellationToken.None).GetAwaiter().GetResult().Count);
        }

        [TestMethod]
        public void EnqueueAsync_Q2Changed_160()
        {
            _repository.EnqueueAsync(
                QueueNames.TestQueue2,
                new Range<DateTime>(
                    min: (2050, 1, 1).ToDate().AtNoon(),
                    max: (2050, 1, 2).ToDate().AtNoon()
                ),
                new[]
                {
                    new NewMessage{ Body = "foo".ToBytes(), Fingerprint = "foo".ToBytes() },
                    new NewMessage{ Body = "bar".ToBytes(), Fingerprint = "bar".ToBytes() }
                },
                EnqueueOptions.None,
                CancellationToken.None
            ).GetAwaiter().GetResult();

            Assert.AreEqual(1, _repository.GetMessageCountAsync(QueueNames.TestQueue1, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(6, _repository.GetMessageCountAsync(QueueNames.TestQueue2, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(0, _repository.GetMessageCountAsync(QueueNames.ControlQueue, CancellationToken.None).GetAwaiter().GetResult());
        }

        [TestMethod]
        public void EnqueueAsync_EmptyTimeRange_160()
        {
            _repository.EnqueueAsync(
                QueueNames.TestQueue1,
                new Range<DateTime>(
                    min: (2050, 1, 1).ToDate().AtNoon(),
                    max: (2050, 1, 2).ToDate().AtNoon()
                ),
                new List<NewMessage>(),
                EnqueueOptions.AllowEmptyTimeRange,
                CancellationToken.None
            ).GetAwaiter().GetResult();

            _repository.EnqueueAsync(
                QueueNames.TestQueue2,
                new Range<DateTime>(
                    min: (2050, 1, 1).ToDate().AtNoon(),
                    max: (2050, 1, 2).ToDate().AtNoon()
                ),
                new List<NewMessage>(),
                EnqueueOptions.UniqueInPending,
                CancellationToken.None
            ).GetAwaiter().GetResult();

            Assert.AreEqual(1, _repository.GetMessageCountAsync(QueueNames.TestQueue1, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(4, _repository.GetMessageCountAsync(QueueNames.TestQueue2, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(0, _repository.GetMessageCountAsync(QueueNames.ControlQueue, CancellationToken.None).GetAwaiter().GetResult());
        }

        [TestMethod]
        public void EnqueueAsync_Duplicates_330()
        {
            _repository.EnqueueAsync(
                QueueNames.TestQueue1,
                new Range<DateTime>(
                    min: (2050, 1, 1).ToDate().AtNoon(),
                    max: (2050, 1, 2).ToDate().AtNoon()
                ),
                new[]
                {
                    new NewMessage { Body = "new-1".ToBytes(), Fingerprint = "new-1".ToBytes() },
                    new NewMessage { Body = "new-2".ToBytes(), Fingerprint = "new-2".ToBytes() }
                },
                EnqueueOptions.AllowEmptyTimeRange,
                CancellationToken.None
            ).GetAwaiter().GetResult();

            _repository.EnqueueAsync(
                QueueNames.TestQueue2,
                new Range<DateTime>(
                    min: (2050, 1, 1).ToDate().AtNoon(),
                    max: (2050, 1, 2).ToDate().AtNoon()
                ),
                new[]
                {
                    new NewMessage { Body = "new-1".ToBytes(), Fingerprint = "new-1".ToBytes() },
                    new NewMessage { Body = "new-2".ToBytes(), Fingerprint = "new-2".ToBytes() },
                    new NewMessage { Body = "new-4".ToBytes(), Fingerprint = "new-4".ToBytes() }
                },
                EnqueueOptions.UniqueInPending,
                CancellationToken.None
            ).GetAwaiter().GetResult();

            Assert.AreEqual(3, _repository.GetMessageCountAsync(QueueNames.TestQueue1, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(5, _repository.GetMessageCountAsync(QueueNames.TestQueue2, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(0, _repository.GetMessageCountAsync(QueueNames.ControlQueue, CancellationToken.None).GetAwaiter().GetResult());
        }

        [TestMethod]
        public void PeekMessageAsync_2_120()
        {
            var m1 = _repository.PeekAsync(QueueNames.TestQueue1, 2, CancellationToken.None).GetAwaiter().GetResult();
            var m2 = _repository.PeekAsync(QueueNames.TestQueue2, 2, CancellationToken.None).GetAwaiter().GetResult();
            var m3 = _repository.PeekAsync(QueueNames.ControlQueue, 2, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, m1.Count);
            Assert.AreEqual(2, m2.Count);
            Assert.AreEqual(0, m3.Count);

            Assert.AreEqual("new-1", Encoding.UTF8.GetString(m1[0].Body));

            Assert.AreEqual("new-1", Encoding.UTF8.GetString(m2[0].Body));
            Assert.AreEqual("new-2", Encoding.UTF8.GetString(m2[1].Body));
        }

        [TestMethod]
        public void DequeueAsync_2_020()
        {
            var m1 = _repository.PeekAsync(QueueNames.TestQueue1, 2, CancellationToken.None).GetAwaiter().GetResult();
            var m2 = _repository.PeekAsync(QueueNames.TestQueue2, 2, CancellationToken.None).GetAwaiter().GetResult();

            _repository.DequeueAsync(QueueNames.TestQueue1, m1.Select(m => m.Id), CancellationToken.None).GetAwaiter().GetResult();
            _repository.DequeueAsync(QueueNames.TestQueue2, m2.Select(m => m.Id), CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, _repository.GetMessageCountAsync(QueueNames.TestQueue1, CancellationToken.None).GetAwaiter().GetResult());
            Assert.AreEqual(2, _repository.GetMessageCountAsync(QueueNames.TestQueue2, CancellationToken.None).GetAwaiter().GetResult());
        }
    }

    public static class SqlHelper2
    {
        public static Task<int> ExecuteNonQueryAsync(this SqlConnection connection, string query, Action<SqlCommand> body, CancellationToken cancellationToken)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                body(command);
                return command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public static Task<int> ExecuteNonQueryAsync(this SqlConnection connection, string query, CancellationToken cancellationToken)
        {
            return connection.ExecuteNonQueryAsync(query, _ => { }, cancellationToken);
        }

        public static int ExecuteNonQuery(this SqlConnection connection, string query, Action<SqlCommand> body)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                body(command);
                return command.ExecuteNonQuery();
            }
        }

        public static int ExecuteNonQuery(this SqlConnection connection, string query)
        {
            return connection.ExecuteNonQuery(query, _ => { });
        }
    }

    public static class TupleExtensions
    {
        public static DateTime ToDate(this (int year, int month, int day) date)
        {
            return new DateTime(date.year, date.month, date.day).AsMidnight();
        }

        public static DateTime ToDateTime(this (int year, int month, int day, int hour, int minute, int second) dateTime)
        {
            return new DateTime(dateTime.year, dateTime.month, dateTime.day, dateTime.hour, dateTime.minute, dateTime.second);
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime AsMidnight(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        }

        public static DateTime AtNoon(this DateTime dateTime)
        {
            return dateTime.AsMidnight().AddHours(12);
        }

        public static DateTime AsEndOfDay(this DateTime dateTime)
        {
            return dateTime.AsMidnight().AddHours(23).AddMinutes(59).AddSeconds(59);
        }
    }

    // ----------------------------------

    public static class SqlHelper3
    {
        private const string ForeignKeyColumnsQuery = @"
SELECT  
	obj.name AS [fk_name],
    fk_schema.name AS [fk_schema],
    fk_table.name AS [fk_table],
    fk_column.name AS [fk_column],
	pk_schema.name AS [pk_schema],
    pk_table.name AS [pk_table],
    pk_column.name AS [pk_column]
FROM 
	sys.foreign_key_columns fkc
	INNER JOIN sys.objects obj ON obj.object_id = fkc.constraint_object_id
	
	INNER JOIN sys.tables fk_table ON fk_table.object_id = fkc.parent_object_id
	INNER JOIN sys.schemas fk_schema ON fk_table.schema_id = fk_schema.schema_id
	INNER JOIN sys.columns fk_column ON fk_column.column_id = parent_column_id AND fk_column.object_id = fk_table.object_id

	INNER JOIN sys.tables pk_table ON pk_table.object_id = fkc.referenced_object_id
	INNER JOIN sys.schemas pk_schema ON pk_table.schema_id = pk_schema.schema_id
	INNER JOIN sys.columns pk_column ON pk_column.column_id = referenced_column_id AND pk_column.object_id = pk_table.object_id
";

        public static IList<AlterTableConstraint> GetAlterTableConstraints(this SqlConnection connection, Func<ConstraintInfo, bool> predicate = null)
        {
            return
                connection
                    .Query<ConstraintInfo>(ForeignKeyColumnsQuery)
                    .Where((predicate ?? (_ => true)))
                    .GroupBy(x => x.fk_name)
                    .Select(AlterTableConstraint.Create)
                    .ToList();
        }

        public static IDisposable ToggleForeignKeys(this SqlConnection connection, IEnumerable<AlterTableConstraint> alterTableConstraints)
        {
            foreach (var alterTableConstraint in alterTableConstraints)
            {
                connection.ExecuteNonQuery(alterTableConstraint.Drop);
            }

            return Disposable.Create(() =>
            {
                foreach (var alterTableConstraint in alterTableConstraints)
                {
                    connection.ExecuteNonQuery(alterTableConstraint.Add);
                    connection.ExecuteNonQuery(alterTableConstraint.Check);
                }
            });
        }
    }

    public class AlterTableConstraint : IGrouping<string, ConstraintInfo>
    {
        private readonly IEnumerable<ConstraintInfo> _contraintInfos;

        private AlterTableConstraint(IGrouping<string, ConstraintInfo> cig)
        {
            Key = cig.Key;
            _contraintInfos = cig;

            var ci = cig.First();

            var fk_table = $"[{ci.fk_schema}].[{ci.fk_table}]";
            var pk_table = $"[{ci.pk_schema}].[{ci.pk_table}]";

            var constraint = $"[{ci.fk_name}]";
            var fk = string.Join(", ", cig.Select(x => $"[{x.fk_column}]"));
            var pk = string.Join(", ", cig.Select(x => $"[{x.pk_column}]"));

            Drop = $"ALTER TABLE {fk_table} DROP CONSTRAINT {constraint}";
            Add = $"ALTER TABLE {fk_table} WITH CHECK ADD CONSTRAINT {constraint} FOREIGN KEY({fk}) REFERENCES {pk_table}({pk})";
            Check = $"ALTER TABLE {fk_table} CHECK CONSTRAINT {constraint}";
        }

        public string Key { get; }

        public string Drop { get; }
        public string Add { get; }
        public string Check { get; }

        // LINQPad
        private object ToDump()
        {
            return new
            {
                Key,
                Drop,
                Add,
                Check
            };
        }

        public static AlterTableConstraint Create(IGrouping<string, ConstraintInfo> cig)
        {
            return new AlterTableConstraint(cig);
        }

        public IEnumerator<ConstraintInfo> GetEnumerator()
        {
            return _contraintInfos.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")] // Using sql naming convention here to avoid mapping.
    public class ConstraintInfo
    {
        public string fk_name { get; set; }

        public string fk_schema { get; set; }
        public string fk_table { get; set; }
        public string fk_column { get; set; }

        public string fk_full => $"[{fk_schema}].[{fk_table}].[{fk_column}]";

        public string pk_schema { get; set; }
        public string pk_table { get; set; }
        public string pk_column { get; set; }

        public string pk_full => $"[{pk_schema}].[{pk_table}].[{pk_column}]";
    }
}
