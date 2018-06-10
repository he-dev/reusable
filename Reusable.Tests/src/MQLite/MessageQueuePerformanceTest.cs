using System;
using System.Data;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Cryptography;
using Reusable.Cryptography.Extensions;
using Reusable.Data;
using Reusable.MQLite;
using Reusable.Tests.MQLite;
using Reusable.Utilities.SqlClient;

namespace Emerald.Tests.MessageDatabase
{
    [TestClass]
    public class MessageQueuePerformanceTest
    {
        private IMessageRepository _repository;

        [TestInitialize]
        public void TestInitialize()
        {
            var connectonString = "Data Source=(local);Initial Catalog=TestDb;Integrated Security=SSPI;";

            _repository = new MessageRepository(connectonString);

            var timeRanges =
                new DataTable()
                    .AddColumn("Id", typeof(long))
                    .AddColumn("QueueId", typeof(int))
                    .AddColumn("StartsOn", typeof(DateTime))
                    .AddColumn("EndsOn", typeof(DateTime));

            var messages =
                new DataTable()
                    //.AddColumn("Id", typeof(long))
                    .AddColumn("TimeRangeId", typeof(int))
                    .AddColumn("Body", typeof(byte[]))
                    .AddColumn("BodyHash", typeof(string))
                    .AddColumn("DeletedOn", typeof(DateTime));

            timeRanges
                .AddRow(1, 2, (2000, 1, 3).ToDate(), (2000, 1, 4).ToDate());

            var rowCount = 100_000; // 1_000_000;

            for (var i = 0; i < rowCount; i++)
            {
                var body = $"new-{i}".ToBytes();
                var bodyHash = body.ComputeSHA1().ToHexString();
                messages.AddRow(1, body, bodyHash, DBNull.Value); // (2000, 1, 1).ToDate().AtNoon().AddDays(i));
            }

            SqlHelper.Execute(connectonString, connection =>
            {
                var tableNames = new[] { "TimeRange", "Message" };
                var alterTableConstraints = connection.GetAlterTableConstraints(ci => tableNames.Contains(ci.fk_table, StringComparer.OrdinalIgnoreCase));

                using (connection.ToggleForeignKeys(alterTableConstraints))
                {
                    connection.Seed(new SqlFourPartName("Message") { SchemaName = "smq" }, messages);
                    connection.Seed(new SqlFourPartName("TimeRange") { SchemaName = "smq" }, timeRanges);
                }
            });
        }

        // disable because it executs for a long time
        //[TestMethod]
        public void Enqueue_TestMethod1()
        {
            //var rowCount = 1_000_000;
            var rowCount = 100_000;

            for (var i = 0; i < rowCount; i += 500)
            {
                var pendingMessages = _repository.PeekAsync("test-queue-2", 500, CancellationToken.None).GetAwaiter().GetResult();
                _repository.DequeueAsync("test-queue-2", pendingMessages.Select(m => m.Id), CancellationToken.None).GetAwaiter().GetResult();
            }

            Assert.AreEqual(0, _repository.GetMessageCountAsync("test-queue-2", CancellationToken.None).GetAwaiter().GetResult());
        }
    }
}
