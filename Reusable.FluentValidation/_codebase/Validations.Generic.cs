using System;

namespace Reusable.Fuse
{
    public static class GenericValidation
    {
        //private static readonly IFormatProvider FormatProvider = new QuoteFormatter();

        public static ICurrent<T> IsNull<T>(this ICurrent<T> current, string message = null)
        {
            return current.Check(
                value => ReferenceEquals(value, null),
                message ?? $"\"{current.MemberName}\" must be null.");
        }

        public static ICurrent<T> IsNotNull<T>(this ICurrent<T> current, string message = null)
        {
            return current.Check(
                value => !ReferenceEquals(value, null),
                message ?? $"\"{current.MemberName}\" must not be null.");
        }
        
        public static ICurrent<T> IsLessThen<T>(this ICurrent<T> current, T max, string message = null)
            where T : IComparable
        {
            return current.Check(
                value => value.CompareTo(max) < 0,
                message ?? $"\"{current.MemberName}\" must be less then \"{max}\".");
        }

        public static ICurrent<T> IsLessThenOrEqual<T>(this ICurrent<T> current, T max, string message = null)
            where T : IComparable
        {
            return current.Check(
                value => value.CompareTo(max) <= 0,
                message ?? $"\"{current.MemberName}\" must be less then or equal \"{max}\".");
        }

        public static ICurrent<T> IsGreaterThen<T>(this ICurrent<T> current, T min, string message = null)
            where T : IComparable
        {
            return current.Check(
                value => value.CompareTo(min) > 0,
                message ?? $"\"{current.MemberName}\" must be greater then \"{min}\".");
        }

        public static ICurrent<T> IsGreaterThenOrEqual<T>(this ICurrent<T> current, T min, string message = null)
            where T : IComparable
        {
            return current.Check(
                value => value.CompareTo(min) >= 0,
                message ?? $"\"{current.MemberName}\" must be greater or equal \"{min}\".");
        }

        public static ICurrent<T> IsBetween<T>(this ICurrent<T> current, T min, T max, string message = null)
            where T : IComparable
        {
            return current.Check(
                value => value.CompareTo(min) > 0 && value.CompareTo(max) < 0,
                message ?? $"\"{current.MemberName}\" must be between \"{min}\" and \"{max}\".");
        }

        public static ICurrent<T> IsBetweenOrEqual<T>(this ICurrent<T> current, T min, T max, string message = null)
            where T : IComparable
        {
            return current.Check(
                value => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0,
                message ?? $"\"{current.MemberName}\" must be between or equal \"{min}\" and \"{max}\".");
        }

        public static ICurrent<T> IsEqual<T>(this ICurrent<T> current, T equalValue, string message = null)
            where T : IComparable
        {
            return current.Check(
                value => value.CompareTo(equalValue) == 0,
                message ?? $"\"{current.MemberName}\" must be equal \"{equalValue}\" but is \"{current.Value}\".");
        }

        public static ICurrent<T> IsNotEqual<T>(this ICurrent<T> current, T notEqualValue, string message = null)
            where T : IComparable
        {
            return current.Check(
                value => value.CompareTo(notEqualValue) != 0,
                message ?? $"\"{current.MemberName}\" must not be equal \"{notEqualValue}\".");
        }
        
        public static ICurrent<T> IsTrue<T>(this ICurrent<T> current, Func<T, bool> predicate, string message = null)
        {
            return current.Check(
                value => predicate(value),
                message ?? $"\"{current.MemberName}\" must be \"{bool.TrueString}\".");
        }

        public static ICurrent<T> IsFalse<T>(this ICurrent<T> current, Func<T, bool> predicate, string message = null)
        {
            return current.Check(
                value => !predicate(value),
                message ?? $"\"{current.MemberName}\" must be \"{bool.FalseString}\".");
        }

        public static ICurrent<T> IsInstanceOfType<T>(this ICurrent<T> current, Type type, string message = null)
        {
            return current.Check(
                value => type.IsInstanceOfType(value),
                message ?? $"\"{current.MemberName}\" must be instance of \"{type.FullName}\".");
        }
    }    
}
