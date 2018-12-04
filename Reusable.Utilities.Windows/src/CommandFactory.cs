using System;
using System.Diagnostics;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Utilities.Windows
{
    public static class CommandFactory<T>
    {
        public static ICommand Create([NotNull] Action<T> execute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            return new Command
            (
                obj => execute(SafeCast(obj))
            );
        }

        public static ICommand Create([NotNull] Action<T> execute, [NotNull] Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            if (canExecute == null) throw new ArgumentNullException(nameof(canExecute));

            return new Command
            (
                obj => execute(SafeCast(obj)),
                obj => canExecute(SafeCast(obj))
            );
        }

        private static T SafeCast(object obj)
        {
            return
                obj is null
                    ? default
                    : obj is T parameter
                        ? parameter
                        : throw DynamicException.Create("CommandParameterType", $"Could not cast '{obj.GetType().ToPrettyString()}' to '{typeof(T).ToPrettyString()}'.");
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