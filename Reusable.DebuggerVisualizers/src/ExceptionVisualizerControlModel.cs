using System;
using System.Collections.Generic;
using System.Linq.Custom;
using System.Windows;
using System.Windows.Input;
using Reusable.Utilities.Windows;

namespace Reusable.DebuggerVisualizers
{
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
}