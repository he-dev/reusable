using System;
using System.Windows.Input;

namespace Reusable.Commands
{
    public static class CommandComposition
    {
        public static ICommand Pre(this ICommand current, ICommand pre) => new LinkedCommand(
            pre ?? throw new ArgumentNullException(nameof(pre)),
            current ?? throw new ArgumentNullException(nameof(current))
        );

        public static ICommand Post(this ICommand current, ICommand post) => new LinkedCommand(
            current ?? throw new ArgumentNullException(nameof(current)),
            post ?? throw new ArgumentNullException(nameof(post))
        );
    }
}