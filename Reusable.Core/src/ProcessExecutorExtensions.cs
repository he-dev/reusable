using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable
{
    public static class ProcessExecutorExtensions
    {
        public static Task<ProcessResult> NoWindowCmdExecuteAsync(this ProcessExecutor executor, [NotNull] string cmdSwitches, [NotNull] string fileName, [NotNull] string arguments)
        {
            return executor.NoWindowExecuteAsync("cmd", $"{cmdSwitches} {fileName} {arguments}");
        }

        public static int ShellCmdExecute(this ProcessExecutor executor, [NotNull] string cmdSwitches, [NotNull] string fileName, [NotNull] string arguments)
        {
            return executor.ShellExecute("cmd", $"{cmdSwitches} {fileName} {arguments}");
        }
    }
}