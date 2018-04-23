using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Reusable.MQLite
{
    public static class MessageRepositoryExtensions
    {
        public static Task EnqueueAsync(
            this IMessageRepository messageRepository,
            DateTime timeRangeStartsOn,
            DateTime timeRangeEndsOn,
            IEnumerable<object> bodies,
            IBodySerializer serializer,
            CancellationToken cancellationToken)
        {
            //return messageQueue.EnqueueAsync(timeRangeStartsOn, timeRangeEndsOn, bodies.Select(serializer.Serialize), cancellationToken);
            return Task.CompletedTask;
        }
    }
}