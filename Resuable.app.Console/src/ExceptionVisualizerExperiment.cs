using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Autofac;
using JetBrains.Annotations;
using Reusable.Apps;
using Reusable.DebuggerVisualizers;

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
                exceptionString = ExceptionParser.RemoveStackStrace(exceptionString);
                var exceptions = ExceptionParser.ParseExceptions(exceptionString);
                ExceptionVisualizer.TestShowVisualizer(exceptions.Reverse().ToArray());
            }
        }


        
    }

    internal class User
    {
        public User(string name) { }
    }
    
}
