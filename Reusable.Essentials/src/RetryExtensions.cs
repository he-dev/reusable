using System;
using System.Threading;
using System.Threading.Tasks;

namespace Reusable.Essentials;

public static class RetryExtensions
{
    public static async Task ExecuteAsync(this IRetry retry, Func<CancellationToken, Task> body)
    {
        await retry.ExecuteAsync<object>(cancellationToken => { body(cancellationToken); return null; }, CancellationToken.None);
    }

    public static T Execute<T>(this IRetry retry, Func<T> body)
    {
        return retry.ExecuteAsync(_ => Task.FromResult(body()), CancellationToken.None).GetAwaiter().GetResult();
    }

    public static void Execute(this IRetry retry, Action body)
    {
        retry.ExecuteAsync(_ => { body(); return Task.FromResult<object>(null); }, CancellationToken.None).GetAwaiter().GetResult();
    }
}