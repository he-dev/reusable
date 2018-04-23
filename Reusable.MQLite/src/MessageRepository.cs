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

[assembly: InternalsVisibleTo("Reusable.Tests")]

namespace Reusable.MQLite
{
    public interface IMessageRepository
    {
        Task<bool> ExistsAsync(string name, CancellationToken cancellationToken);

        Task<int> EnqueueAsync(string name, Range<DateTime> timeRange, IEnumerable<NewMessage> data, EnqueueOptions options, CancellationToken cancellationToken);

        Task<List<Models.PendingMessage>> PeekAsync(string name, int count, CancellationToken cancellationToken);

        Task<int> DequeueAsync(string name, IEnumerable<long> messageIds, CancellationToken cancellationToken);

        Task<int> GetMessageCountAsync(string name, CancellationToken cancellationToken);

        Task<int> GetTimeRangeCountAsync(string name, CancellationToken cancellationToken);

        Task<List<Models.TimeRange>> GetLastTimeRangeAsync(string name, int count, CancellationToken cancellationToken);

        Task<int> RemoveTimeRangesAsync(string name, int count, CancellationToken cancellationToken);

        Task<int> RemoveMessagesAsync(string name, int count, RemoveMessageOptions options, CancellationToken cancellationToken);
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

        public async Task<int> EnqueueAsync(string name, Range<DateTime> timeRange, IEnumerable<NewMessage> data, EnqueueOptions options, CancellationToken cancellationToken)
        {
            const int nothingEnqueued = 0;

            using (var context = CreateContext())
            {
                var uniqueInQueue = options.HasFlag(EnqueueOptions.UniqueInQueue);
                var uniqueInPending = options.HasFlag(EnqueueOptions.UniqueInPending);

                var checkUniqueness =
                    uniqueInQueue ||
                    uniqueInPending;

                var messages =
                    (from item in data
                     let isUniqueInQueue = uniqueInQueue && !context.Messages.Any(m => m.TimeRange.Queue.Name == name && m.Fingerprint == item.Fingerprint)
                     let isUniqueInPending = uniqueInPending && !context.Messages.Any(m => m.TimeRange.Queue.Name == name && m.Fingerprint == item.Fingerprint && m.DeletedOn == null)
                     where
                        !checkUniqueness ||
                        isUniqueInQueue ||
                        isUniqueInPending
                     select new Message
                     {
                         Body = item.Body,
                         Fingerprint = item.Fingerprint
                     })
                    .ToList();

                var allowEmptyTimeRange = options.HasFlag(EnqueueOptions.AllowEmptyTimeRange);
                if (!messages.Any() && !allowEmptyTimeRange)
                {
                    return nothingEnqueued;
                }

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
        }

        public async Task<List<Models.PendingMessage>> PeekAsync(string name, int count, CancellationToken cancellationToken)
        {
            using (var context = CreateContext())
            {
                return
                    await context
                        .Messages.AsNoTracking()
                        .Where(m => m.TimeRange.Queue.Name == name && m.DeletedOn == null)
                        .OrderBy(m => m.Id)
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

        public async Task<int> GetMessageCountAsync(string name, CancellationToken cancellationToken)
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

        public async Task<List<Models.TimeRange>> GetLastTimeRangeAsync(string name, int count, CancellationToken cancellationToken)
        {
            using (var context = CreateContext())
            {
                return
                    await context
                        .TimeRanges.AsNoTracking()
                        .Where(tr => tr.Queue.Name == name)
                        .OrderByDescending(tr => tr.Id)
                        .Take(count)
                        .Select(p => new Models.TimeRange
                        {
                            Id = p.Id,
                            StartsOn = p.StartsOn,
                            EndsOn = p.EndsOn,
                            CreatedOn = p.CreatedOn
                        })
                        .ToListAsync(cancellationToken);
            }
        }

        public async Task<int> RemoveTimeRangesAsync(string name, int count, CancellationToken cancellationToken)
        {
            using (var context = CreateContext())
            {
                var timeRanges =
                    context
                        .TimeRanges
                        .Where(tr => tr.Queue.Name == name)
                        .Take(count)
                        .Select(tr => tr);
                        //.ToArrayAsync(cancellationToken);

                //var optionalTake = new OptionalTake();
                //var newExpression = optionalTake.Visit(timeRanges.Expression);

                context.TimeRanges.RemoveRange(timeRanges);
                return await context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> RemoveMessagesAsync(string name, int count, RemoveMessageOptions options, CancellationToken cancellationToken)
        {
            var pendingOnly = options.HasFlag(RemoveMessageOptions.PendingOnly);
            var deletedOnly = options.HasFlag(RemoveMessageOptions.DeletedOnly);
            var any = options.HasFlag(RemoveMessageOptions.Any);

            using (var context = CreateContext())
            {
                var messages =
                    await context
                        .Messages
                        .Where(m => 
                            m.TimeRange.Queue.Name == name && 
                            (
                                any || 
                                (pendingOnly && m.DeletedOn == null) || 
                                (deletedOnly && m.DeletedOn != null)
                            )
                        )
                        .Take(count)
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
    }

    [Flags]
    public enum EnqueueOptions
    {
        None = 1 << 0,
        AllowEmptyTimeRange = 1 << 1,
        UniqueInPending = 1 << 2,
        UniqueInQueue = 1 << 3
    }

    [Flags]
    public enum RemoveMessageOptions
    {
        None = 1 << 0,
        PendingOnly = 1 << 1,
        DeletedOnly = 1 << 2,
        Any =
            PendingOnly |
            DeletedOnly
    }

    internal class OptionalTake : ExpressionVisitor
    {
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            return base.VisitInvocation(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return base.VisitMethodCall(node);
        }
    }
}
