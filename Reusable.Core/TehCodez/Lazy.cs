using System;

namespace Reusable
{
    public static class Lazy
    {
        public static Lazy<T> Create<T>(Func<T> valueFactory) => new Lazy<T>(valueFactory);
    }
}
