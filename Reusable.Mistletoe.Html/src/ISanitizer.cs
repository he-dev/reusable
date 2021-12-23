using System;

namespace Reusable.Htmlize
{
    public interface ISanitizer
    {
        string Sanitize(object value, IFormatProvider formatProvider);
    }
}