using System;

namespace Reusable.Markup
{
    public interface ISanitizer
    {
        string Sanitize(object value, IFormatProvider formatProvider);
    }
}