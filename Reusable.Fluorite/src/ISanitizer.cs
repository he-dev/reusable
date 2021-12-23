using System;

namespace Reusable.Fluorite;

public interface ISanitizer
{
    string Sanitize(object value, IFormatProvider formatProvider);
}