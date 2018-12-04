using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Reusable.DebuggerVisualizers;

[assembly: DebuggerVisualizer(
    visualizer: typeof(ExceptionVisualizer),
    visualizerObjectSource: typeof(ExceptionVisualizerObjectSource),
    Target = typeof(Exception),
    Description = "Exception Visualizer")]

namespace Reusable.DebuggerVisualizers
{
    public class ExceptionVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var exceptions = (IEnumerable<ExceptionInfo>)objectProvider.GetObject();

            var window = new Window
            {
                Title = "Exception Visualizer",
                Width = SystemParameters.WorkArea.Width * 0.4,
                Height = SystemParameters.WorkArea.Height * 0.6,
                Content = new ExceptionVisualizerControl
                {
                    DataContext = new ExceptionVisualizerControlModel
                    {
                        Exceptions = exceptions
                    },
                    HorizontalAlignment = HorizontalAlignment.Stretch
                },
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.ShowDialog();
        }

        public static void TestShowVisualizer(object objectToVisualize)
        {
            var visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(ExceptionVisualizer));
            visualizerHost.ShowVisualizer();
        }
    }

    public class ExceptionVisualizerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            var exceptionString = target.ToString();
            exceptionString = ExceptionParser.RemoveStackStrace(exceptionString);
            var exceptions = ExceptionParser.ParseExceptions(exceptionString).Reverse();
            Serialize(outgoingData, exceptions);
        }
    }

    [Serializable]
    public class ExceptionInfo
    {
        public string Name { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return Name + Environment.NewLine + Message;
        }
    }

    public class ExceptionParser
    {
        public static string RemoveStackStrace(string exceptionString)
        {
            // Stack-trace begins at the first 'at'
            return Regex.Split(exceptionString, @"^\s{3}at", RegexOptions.Multiline).First();
        }

        public static IEnumerable<ExceptionInfo> ParseExceptions(string exceptionString)
        {
            // Exceptions start with 'xException:' string and end either with '$' or '--->' if an inner exception follows.
            return
                Regex
                    .Matches(exceptionString, @"(?<exception>(^|\w+)?Exception):\s(?<message>(.|\n)+?)(?=( --->|$))", RegexOptions.ExplicitCapture)
                    .Cast<Match>()
                    .Select(m => new ExceptionInfo { Name = m.Groups["exception"].Value, Message = m.Groups["message"].Value.Trim() });
        }
    }
}
