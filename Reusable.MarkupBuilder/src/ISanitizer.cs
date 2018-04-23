using System;

namespace Reusable.MarkupBuilder
{
    public interface ISanitizer
    {
        string Sanitize(object value, IFormatProvider formatProvider);
    }
}