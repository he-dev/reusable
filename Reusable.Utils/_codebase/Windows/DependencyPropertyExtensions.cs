using System.Windows;

namespace Reusable.Windows
{
    public static class DependencyPropertyExtensions
    {
        public static T GetValue<T>(
            this DependencyProperty dependencyProperty,
            DependencyObject dependencyObject
        ) => (T)dependencyObject.GetValue(dependencyProperty);

        public static void SetValue(
            this DependencyProperty dependencyProperty,
            DependencyObject dependencyObject, object value
        ) => dependencyObject.SetValue(dependencyProperty, value);
    }
}