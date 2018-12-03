using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using JetBrains.Annotations;
using Microsoft.VisualStudio.DebuggerVisualizers;
using Reusable.DebuggerVisualizers;


[assembly: DebuggerVisualizer(
    visualizer: typeof(ExceptionVisualizer),
    visualizerObjectSource: typeof(ExceptionVisualizerObjectSource),
    //visualizerObjectSource: typeof(VisualizerObjectSource),
    Target = typeof(Exception),
    Description = "Exception Visualizer")]

namespace Reusable.DebuggerVisualizers
{

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

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Reverse<T>(this IEnumerable<T> source) => new Stack<T>(source);
    }

    public class ExceptionVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var data = (IEnumerable<ExceptionInfo>)objectProvider.GetObject();

            var window = new Window
            {
                Title = "Exception Visualizer",
                Width = SystemParameters.WorkArea.Width * 0.4,
                Height = SystemParameters.WorkArea.Height * 0.6,
                Content = new ExceptionVisualizerControl
                {
                    DataContext = new ExceptionVisualizerControlModel
                    {
                        Exceptions = data
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

    public class ExceptionVisualizerControlModel
    {
        public static readonly ICommand CloseCommand = CommandFactory<Window>.Create(p => p.Close());

        public static readonly ICommand CopyCommand = CommandFactory<ExceptionVisualizerControlModel>.Create(p => p.CopyToClipboard());

        public IEnumerable<ExceptionInfo> Exceptions { get; set; } = new[]
        {
            // This is design-time data.
            new ExceptionInfo {Name = "DependencyResolutionException", Message = "An error occurred during the activation of a particular registration. See the inner exception for details. Registration: Activator = User (ReflectionActivator), Services = [UserQuery+User], Lifetime = Autofac.Core.Lifetime.CurrentScopeLifetime, Sharing = None, Ownership = OwnedByLifetimeScope"},
            new ExceptionInfo {Name = "DependencyResolutionException", Message = "None of the constructors found with 'Autofac.Core.Activators.Reflection.DefaultConstructorFinder' on type 'UserQuery+User' can be invoked with the available services and parameters: Cannot resolve parameter 'System.String name' of constructor 'Void .ctor(System.String)'."},
        };

        private void CopyToClipboard()
        {
            Clipboard.SetText(Exceptions.Join(Environment.NewLine + Environment.NewLine));
        }
    }



    public static class CommandFactory<T>
    {
        public static ICommand Create([NotNull] Action<T> execute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            return new Command(parameter => execute((T)parameter));
        }

        public static ICommand Create([NotNull] Action<T> execute, [NotNull] Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            if (canExecute == null) throw new ArgumentNullException(nameof(canExecute));

            return new Command(parameter => execute((T)parameter), parameter => canExecute((T)parameter));
        }

        private class Command : ICommand
        {
            private readonly Action<object> _execute;

            private readonly Predicate<object> _canExecute;

            public Command(Action<object> execute) : this(execute, _ => true) { }

            public Command(Action<object> execute, Predicate<object> canExecute)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            #region ICommand

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            [DebuggerStepThrough]
            public bool CanExecute(object parameter) => _canExecute(parameter);

            [DebuggerStepThrough]
            public void Execute(object parameter) => _execute(parameter);

            #endregion
        }
    }
}
