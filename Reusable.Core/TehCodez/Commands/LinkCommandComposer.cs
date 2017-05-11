using System;
using System.Windows.Input;

namespace Reusable.Commands
{
    public static class LinkCommandComposer
    {
        public static ICommand Pre(this ICommand current, ICommand pre)
        {
            return new LinkCommand
            (
                pre ?? throw new ArgumentNullException(nameof(pre)),
                current ?? throw new ArgumentNullException(nameof(current))
            );
        }

        public static ICommand Post(this ICommand current, ICommand post)
        {
            return new LinkCommand
            (
                current ?? throw new ArgumentNullException(nameof(current)),
                post ?? throw new ArgumentNullException(nameof(post))
            );
        }
    }
}