using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Reusable.MQLite.Entities;
using Reusable.MQLite.Models;
using TimeRange = Reusable.MQLite.Entities.TimeRange;

[assembly: InternalsVisibleTo("Reusable.Tests")]

namespace Reusable.MQLite
{
    public interface IMessageRepository
    {
        Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        Task<int> EnqueueAsync(string name, Range<DateTime> timeRange, IEnumerable<NewMessage> data, TimeSpan expires, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<Models.PendingMessage>> PeekAsync(string name, int count, CancellationToken cancellationToken = default(CancellationToken));

        Task<int> DequeueAsync(string name, IEnumerable<long> messageIds, CancellationToken cancellationToken = default(CancellationToken));

        Task<int> GetPendingMessageCountAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        Task<int> GetTimeRangeCountAsync(string name, CancellationToken cancellationToken = default(CancellationToken));

        Task<Models.TimeRange> GetLastTimeRangeAsync(string name, bool ignoreEmpty, CancellationToken cancellationToken = default(CancellationToken));

        Task<int> RemoveTimeRangesAsync(string name, DateTime? createdOnMin, DateTime? createdOnMax, CancellationToken cancellationToken = default(CancellationToken));

        Task<int> RemoveMessagesAsync(string name, DateTime? createdOnMin, DateTime? createdOnMax, CancellationToken cancellationToken = default(CancellationToken));
    }

    public class MessageRepository : IMessageRepository
    {
        public const string DefaultSchema = "smq";

        public const string DefaultConnectionStringName = nameof(MessageRepository);

        private readonly string _connectionString;

        private readonly string _schema;

        public MessageRepository(string connectionString, string schema = DefaultSchema)
        {
            _connectionString = connectionString;
            _schema = schema;
        }

        public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken)
        {
            using (var context = new MessageContext(_connectionString, _schema))
            {
                return await context.Queues.AnyAsync(q => q.Name == name, cancellationToken);
            }
        }

        public async Task<int> EnqueueAsync(string name, Range<DateTime> timeRange, IEnumerable<NewMessage> data, TimeSpan expires, CancellationToken cancellationToken)
        {
            using (var context = CreateContext())
            {
                var now = await GetUtcDateAsync(context.Database.GetDbConnection(), cancellationToken);

                var messages =
                    (from item in data
                     let isNew = !context.Messages.Any(Exists(item.Fingerprint))
                     let isExpired = isNew && !context.Messages.Any(IsValid(item.Fingerprint, now))
                     where isNew && isExpired
                     select new Message
                     {
                         Body = item.Body,
                         Fingerprint = item.Fingerprint,
                         Priority = 0
                     })
                    .ToList();

                var newTimeRange = new Entities.TimeRange
                {
                    Queue = context.Queues.Single(q => q.Name == name),
                    StartsOn = timeRange.Min,
                    EndsOn = timeRange.Max,
                    Messages = messages
                };

                await context.TimeRanges.AddAsync(newTimeRange, cancellationToken);
                return await context.SaveChangesAsync(cancellationToken);
            }

            Expression<Func<Message, bool>> Exists(byte[] fingerprint)
            {
                return m =>
                    m.TimeRange.Queue.Name == name &&
                    m.Fingerprint == fingerprint &&
                    m.DeletedOn == null;
            }

            Expression<Func<Message, bool>> IsValid(byte[] fingerprint, DateTime now)
            {
                return m =>
                    m.TimeRange.Queue.Name == name &&
                    m.Fingerprint == fingerprint &&
                    m.DeletedOn != null &&
                    (
                        expires == TimeSpan.Zero || 
                        expires >= now - m.TimeRange.CreatedOn
                    );
            }
        }

        public async Task<List<Models.PendingMessage>> PeekAsync(string name, int count, CancellationToken cancellationToken)
        {
            using (var context = CreateContext())
            {
                return
                    await context
                        .Messages.AsNoTracking()
                        .Where(m => m.TimeRange.Queue.Name == name && m.DeletedOn == null)
                        .Take(count)
                        .Select(m => new Models.PendingMessage
                        {
                            Id = m.Id,
                            Body = m.Body,
                            Fingerprint = m.Fingerprint,
                            CreatedOn = m.TimeRange.CreatedOn
                        })
                        .ToListAsync(cancellationToken);
            }
        }

        public async Task<int> DequeueAsync(string name, IEnumerable<long> messageIds, CancellationToken cancellationToken)
        {
            using (var context = CreateContext())
            {
                var deletedOn = await GetUtcDateAsync(context.Database.GetDbConnection(), cancellationToken);

                var messages = await
                    (from m in context.Messages
                     where m.DeletedOn == null && m.TimeRange.Queue.Name == name && messageIds.Contains(m.Id)
                     select m).ToListAsync(cancellationToken);//.SingleOrDefaultAsync(cancellationToken: cancellationToken);

                foreach (var message in messages)
                {
                    //var message = await context.Messages.FindAsync(messageId);


                    //if (message is null)
                    //{
                    //    throw new ArgumentException($"Message {messageId} not found.");
                    //}

                    //var timeRange = await context.TimeRanges.FindAsync(message.TimeRangeId);

                    //if (timeRange.QueueId != Id)
                    //{
                    //    throw new ArgumentException($"Message {messageId} does not belong to the queue {Id}.");
                    //}

                    //if (message.DeletedOn is null)
                    //{
                    //    message.DeletedOn = deletedOn;
                    //}
                    message.DeletedOn = deletedOn;
                }

                return await context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> GetPendingMessageCountAsync(string name, CancellationToken cancellationToken)
        {
            using (var context = CreateContext())
            {
                return
                    await context
                        .Messages.AsNoTracking()
                        .Where(m => m.TimeRange.Queue.Name == name && m.DeletedOn == null)
                        .CountAsync(cancellationToken);
            }
        }

        public async Task<int> GetTimeRangeCountAsync(string name, CancellationToken cancellationToken)
        {
            using (var context = CreateContext())
            {
                return
                    await context
                        .TimeRanges.AsNoTracking()
                        .Where(tr => tr.Queue.Name == name)
                        .CountAsync(cancellationToken);
            }
        }

        public async Task<Models.TimeRange> GetLastTimeRangeAsync(string name, bool ignoreEmpty, CancellationToken cancellationToken)
        {
            using (var context = CreateContext())
            {
                var lastTimeRange =
                    await context
                        .TimeRanges.AsNoTracking()
                        .OrderByDescending(tr => tr.Id)
                        .FirstOrDefaultAsync(tr => tr.Queue.Name == name && ((ignoreEmpty && tr.Messages.Count > 0) || true), cancellationToken);

                return lastTimeRange == null ? null : new Models.TimeRange
                {
                    Id = lastTimeRange.Id,
                    StartsOn = lastTimeRange.StartsOn,
                    EndsOn = lastTimeRange.EndsOn,
                    CreatedOn = lastTimeRange.CreatedOn
                };
            }
        }

        public async Task<int> RemoveTimeRangesAsync(string name, DateTime? createdOnMin, DateTime? createdOnMax, CancellationToken cancellationToken)
        {
            using (var context = CreateContext())
            {
                var timeRanges =
                    context
                        .TimeRanges
                        .Where(tr => 
                            tr.Queue.Name == name && 
                            (!createdOnMin.HasValue || createdOnMin.Value <= tr.CreatedOn) &&
                            (!createdOnMax.HasValue || tr.CreatedOn <= createdOnMax.Value)
                        )
                        .Select(tr => tr);

                context.TimeRanges.RemoveRange(timeRanges);
                return await context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> RemoveMessagesAsync(string name, DateTime? createdOnMin, DateTime? createdOnMax, CancellationToken cancellationToken)
        {
            using (var context = CreateContext())
            {
                var messages =
                    await context
                        .Messages
                        .Where(m => 
                            m.TimeRange.Queue.Name == name && 
                            (!createdOnMin.HasValue || createdOnMin.Value <= m.TimeRange.CreatedOn) &&
                            (!createdOnMax.HasValue || m.TimeRange.CreatedOn <= createdOnMax.Value)
                        )
                        .Select(tr => tr)
                        .ToArrayAsync(cancellationToken);

                context.Messages.RemoveRange(messages);
                return await context.SaveChangesAsync(cancellationToken);
            }
        }

        private MessageContext CreateContext()
        {
            var context = new MessageContext(_connectionString, _schema);

            //_queue = _queue ?? context.Queues.SingleOrDefault(q => q.Name == Name);

            //if (_queue is null)
            //{
            //    throw new InvalidOperationException($"There is no such queue as {Name}.");
            //}

            // Tell EF that the queue already exists and did not change. 
            // Otherwise it'll try to insert it and crashes. It's important to create a new copy.
            //context.Queues.Attach(new Entities.Queue { Id = Id, Name = Name });

            return context;
        }

        private static async Task<DateTime> GetUtcDateAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            await connection.OpenAsync(cancellationToken);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select getutcdate();";
                return (DateTime)await command.ExecuteScalarAsync(cancellationToken);
            }
        }

        //private static Expression<Func<TimeRange, bool>> IsCreatedBetween(DateTime? createdOnMin, DateTime? createdOnMax)
        //{
        //    return tr =>
        //        (!createdOnMin.HasValue || createdOnMin.Value <= tr.CreatedOn) &&
        //        (!createdOnMax.HasValue || tr.CreatedOn <= createdOnMax.Value);
        //}
    }
}
