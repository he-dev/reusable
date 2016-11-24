using System.Windows.Input;
using Reusable.Fuse;

namespace Reusable.Commands
{
    public static class CommandComposition
    {
        public static ICommand Pre(this ICommand current, ICommand pre)
        {
            current.Validate(nameof(current)).IsNotNull();
            pre.Validate(nameof(pre)).IsNotNull();

            return new LinkedCommand(pre, current);
        }

        public static ICommand Post(this ICommand current, ICommand post)
        {
            current.Validate(nameof(current)).IsNotNull();
            post.Validate(nameof(post)).IsNotNull();

            return new LinkedCommand(current, post);
        }
    }
}