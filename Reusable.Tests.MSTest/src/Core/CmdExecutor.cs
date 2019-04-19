using System;
using System.Diagnostics;
using System.Linq.Custom;
using System.Text;
using JetBrains.Annotations;

namespace Reusable.Tests
{
    /// <inheritdoc />
    /// <summary>
    /// Executes commands in a new instance of the Windows command interpreter.
    /// </summary>
    [UsedImplicitly]
    public class CmdExecutor : MarshalByRefObject
    {
        public CmdResult Execute([NotNull] string fileName, [NotNull] string arguments, params string[] cmdSwitches)
        {
            if (cmdSwitches == null) throw new ArgumentNullException(nameof(cmdSwitches));
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            arguments = $"{cmdSwitches.Join(" ")} {fileName} {arguments}";
            var startInfo = new ProcessStartInfo("cmd", arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using (var process = new Process { StartInfo = startInfo })
            {
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

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                return new CmdResult
                {
                    Arguments = arguments,
                    Output = output.ToString(),
                    Error = error.ToString(),
                    ExitCode = process.ExitCode
                };
            }
        }
    }

    [Serializable]
    public class CmdResult
    {
        public string Arguments { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public int ExitCode { get; set; }
    }

    public static class CmdResultExtensions
    {
        public static bool Success(this CmdResult result)
        {
            return result.ExitCode == 0;
        }
    }
}
