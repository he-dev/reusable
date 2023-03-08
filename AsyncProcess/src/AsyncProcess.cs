using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Reusable;

public static class AsyncProcess
{
    public static async Task<Result> StartAsync(string fileName, IEnumerable<string> arguments, string? workingDirectory, int timeoutMilliseconds)
    {
        // If you run bash-script on Linux it is possible that ExitCode can be 255.
        // To fix it you can try to add '#!/bin/bash' header to the script.
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = string.Join(' ', arguments),
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? string.Empty
            }
        };

        var outputBuilder = new StringBuilder();
        var outputCloseEvent = new TaskCompletionSource<bool>();

        process.OutputDataReceived += (_, e) =>
        {
            // The output stream has been closed i.e. the process has terminated.
            if (e.Data is null)
            {
                outputCloseEvent.SetResult(true);
            }
            else
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        var errorBuilder = new StringBuilder();
        var errorCloseEvent = new TaskCompletionSource<bool>();

        process.ErrorDataReceived += (s, e) =>
        {
            // The error stream has been closed i.e. the process has terminated.
            if (e.Data is null)
            {
                errorCloseEvent.SetResult(true);
            }
            else
            {
                errorBuilder.AppendLine(e.Data);
            }
        };

        try
        {
            if (process.Start())
            {
                // Reads the output stream first and then waits because deadlocks are possible.
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Creates task to wait for process exit using timeout.
                var waitForExit = WaitForExitAsync(process, timeoutMilliseconds);

                // Create task to wait for process exit and closing all output streams.
                var processTask = Task.WhenAll(waitForExit, outputCloseEvent.Task, errorCloseEvent.Task);

                // Waits process completion and then checks it was not completed by timeout.
                if (await Task.WhenAny(Task.Delay(timeoutMilliseconds), processTask) == processTask && waitForExit.Result)
                {
                    return new Result(process.ExitCode)
                    {
                        Completed = true,
                        Output = outputBuilder.ToString(),
                        Error = errorBuilder.ToString()
                    };
                }

                // Kill it if it takes too long to complete or hangs.
                try
                {
                    process.Kill();
                    return new Result(-1)
                    {
                        TimedOut = true,
                        Killed = true,
                        Output = outputBuilder.ToString(),
                        Error = errorBuilder.ToString()
                    };
                }
                catch (Exception ex)
                {
                    return new Result(-1)
                    {
                        TimedOut = true,
                        Output = outputBuilder.ToString(),
                        Error = errorBuilder.ToString(),
                        Exception = ex
                    };
                }
            }
        }
        catch (Exception ex)
        {
            // Usually it occurs when an executable file is not found or is not executable.
            return new Result(-1)
            {
                Output = outputBuilder.ToString(),
                Error = errorBuilder.ToString(),
                Exception = ex,
            };
        }

        return new Result(0);
    }


    private static Task<bool> WaitForExitAsync(Process process, int timeout)
    {
        return Task.Run(() => process.WaitForExit(timeout));
    }
    
    public record Result(int ExitCode)
    {
        public bool Success => ExitCode == 0;
        public bool Completed { get; init; }
        public bool TimedOut { get; init; }
        public bool Killed { get; init; }
        public string? Output { get; init; }
        public string? Error { get; init; }
        public Exception? Exception { get; init; }

        public override string ToString()
        {
            return
                new StringBuilder()
                    .AppendLine($"ExitCode: {ExitCode}")
                    .AppendLine($"Completed: {Completed}")
                    .AppendLine($"TimedOut: {TimedOut}")
                    .AppendLine($"Killed: {Killed}")
                    .AppendLine("Output:")
                    .AppendLine(Output).AppendLine()
                    .AppendLine("Error:")
                    .AppendLine(Error).AppendLine()
                    .AppendLine("Exception:")
                    .AppendLine(Exception?.ToString())
                    .ToString();
        }

        public static implicit operator bool(Result result) => result.Success;
    }
}