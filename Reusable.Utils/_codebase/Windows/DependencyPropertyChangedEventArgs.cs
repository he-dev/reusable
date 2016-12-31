using System.Windows;

namespace Reusable.Windows
{
    public struct DependencyPropertyChangedEventArgs<T>
    {
        private readonly DependencyPropertyChangedEventArgs _e;

        public DependencyPropertyChangedEventArgs(DependencyPropertyChangedEventArgs e)
        {
            _e = e;
        }

        public T OldValue => (T)_e.OldValue;

        public T NewValue => (T)_e.NewValue;

        public DependencyProperty Property => _e.Property;
    }
}