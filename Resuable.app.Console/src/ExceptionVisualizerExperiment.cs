using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Autofac;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace Reusable.Apps
{
    internal class ExceptionVisualizerExperiment
    {
        public static void Run()
        {
            try
            {
                try
                {
                    var builder = new ContainerBuilder();
                    builder.RegisterType<User>();
                    var container = builder.Build();
                    container.Resolve<User>();
                    throw new DivideByZeroException("Blub");
                }
                catch (Exception ex)
                {
                    throw new Exception("Blub", ex);

                }
            }
            catch (Exception ex)
            {
                var exceptionString = ex.ToString();
                exceptionString = RemoveStackStrace(exceptionString);
                var exceptions = ParseExceptions(exceptionString);
                TestShowVisualizer(exceptions);
            }
        }

        public static string RemoveStackStrace(string exceptionString)
        {
            return Regex.Split(exceptionString, @"^\s+at", RegexOptions.Multiline).First();
        }

        public static IEnumerable<ExceptionInfo> ParseExceptions(string exceptionString)
        {
            var exceptions =
                Regex
                    .Matches(exceptionString, @"(?<exception>(^|\w+)?Exception):\s(?<message>(.|\n)+?)(?=( --->|$))", RegexOptions.ExplicitCapture)
                    .Cast<Match>()
                    .Select(m => new ExceptionInfo { Name = m.Groups["exception"].Value, Message = m.Groups["message"].Value });

            return new Stack<ExceptionInfo>(exceptions);
        }

        public static void TestShowVisualizer(object objectToVisualize)
        {
            VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(ExceptionVisualizer));
            visualizerHost.ShowVisualizer();
        }

        public class ExceptionVisualizer : DialogDebuggerVisualizer
        {
            protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
            {
                var data = objectProvider.GetObject();

                //MessageBox.Show("Hi!");
                //Application.EnableVisualStyles();
                //var form = new Form();
                //form.Controls.Add(new RichTextBox { Text = "Blub!",  });

                //windowService.ShowDialog(form);

                var win = new Window();
                win.Title = "My Visualizer";
                win.Width = 1024;
                win.Height = 768;
                win.Background = Brushes.Blue;
                win.Content = new ExceptionControl
                {
                    DataContext = new ExceptionVisualizerData { Exceptions = (IEnumerable<ExceptionInfo>)data },
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                win.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                win.ShowDialog();
            }
        }

        class User
        {
            public User(string name)
            {

            }
        }

    }
    public class ExceptionVisualizerData
    {
        public IEnumerable<ExceptionInfo> Exceptions { get; set; } = new ExceptionInfo[]
        {
                new ExceptionInfo {Name = "DependencyResolutionException", Message = "An error occurred during the activation of a particular registration. See the inner exception for details. Registration: Activator = User (ReflectionActivator), Services = [UserQuery+User], Lifetime = Autofac.Core.Lifetime.CurrentScopeLifetime, Sharing = None, Ownership = OwnedByLifetimeScope"},
                new ExceptionInfo {Name = "DependencyResolutionException", Message = "None of the constructors found with 'Autofac.Core.Activators.Reflection.DefaultConstructorFinder' on type 'UserQuery+User' can be invoked with the available services and parameters: Cannot resolve parameter 'System.String name' of constructor 'Void .ctor(System.String)'."},
        };
    }

    [Serializable]
    public class ExceptionInfo
    {
        public string Name { get; set; }

        public string Message { get; set; }
    }
}
