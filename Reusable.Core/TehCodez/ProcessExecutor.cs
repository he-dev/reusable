using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable
{
    public interface IProcessExecutor
    {
        Task<ProcessResult> NoWindowExecuteAsync([NotNull] string fileName, [NotNull] string arguments);
        int ShellExecute([NotNull] string fileName, [NotNull] string arguments);
    }

    /// <summary>
    /// Executes commands in a new instance of the Windows command interpreter.
    /// </summary>
    [UsedImplicitly, PublicAPI]
    public class ProcessExecutor : MarshalByRefObject, IProcessExecutor
    {
        public Task<ProcessResult> NoWindowExecuteAsync([NotNull] string fileName, [NotNull] string arguments)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            var tcs = new TaskCompletionSource<ProcessResult>();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                output.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                error.AppendLine(e.Data);
            };

            process.Exited += (sender, e) =>
            {
                var processLocal = (Process)sender;
                processLocal.WaitForExit();
                tcs.SetResult(new ProcessResult
                {
                    Arguments = arguments,
                    Output = output.ToString(),
                    Error = error.ToString(),
                    ExitCode = processLocal.ExitCode
                });
                processLocal.Dispose();
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            //process.WaitForExit();

            //return new ProcessResult
            //{
            //    Arguments = arguments,
            //    Output = output.ToString(),
            //    Error = error.ToString(),
            //    ExitCode = process.ExitCode
            //};

            return tcs.Task;
        }

        public int ShellExecute([NotNull] string fileName, [NotNull] string arguments)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo(fileName, arguments)
                {
                    UseShellExecute = true,
                }
            })
            {
                process.Start();
                process.WaitForExit();
                return process.ExitCode;
            }
        }
    }

    [Serializable]
    public class ProcessResult
    {
        public string Arguments { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public int ExitCode { get; set; }
    }
}
