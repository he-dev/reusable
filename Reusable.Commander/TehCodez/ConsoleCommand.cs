using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.CommandLine;
using Reusable.OmniLog;

namespace Reusable.Commander
{
    public interface IConsoleCommand
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }

    //    public interface IConditionalCommand : IConsoleCommand
    //    {
    //        Task<bool> CanExecuteAsync(object parameter);
    //    }

    public abstract class ConsoleCommand : IConsoleCommand
    {
        protected ConsoleCommand(ILoggerFactory loggerFactory)
        {
            var longestName = NameFactory.CreateCommandName(GetType()).OrderByDescending(name => name.Length).First();
            Logger = loggerFactory.CreateLogger(longestName);
        }        

        protected ILogger Logger { get; }

        public abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }

    //    internal class LinkedConsoleCommand : IConsoleCommand
    //    {
    //        private readonly IConsoleCommand _pre;
    //        private readonly IConsoleCommand _post;
    //
    //        public LinkedConsoleCommand(IConsoleCommand pre, IConsoleCommand post)
    //        {
    //            _pre = pre;
    //            _post = post;
    //        }
    //
    //        public SoftKeySet Name => SoftKeySet.Create(_pre.Name.Concat(_post.Name)); // $"{nameof(LinkedConsoleCommand)}: {_pre.Name.ToString()} + {_post.Name.ToString()}";
    //
    //        public async Task ExecuteAsync(ICommandContext context)
    //        {
    //            await _pre.ExecuteAsync(context);
    //            await _post.ExecuteAsync(context);
    //        }
    //    }
    //
    //    public static class ConsoleCommandComposer
    //    {
    //        public static IConsoleCommand Pre(this IConsoleCommand current, IConsoleCommand pre)
    //        {
    //            return new LinkedConsoleCommand(pre, current);
    //        }
    //
    //        public static IConsoleCommand Post(this IConsoleCommand current, IConsoleCommand post)
    //        {
    //            return new LinkedConsoleCommand(current, post);
    //        }
    //    }
}