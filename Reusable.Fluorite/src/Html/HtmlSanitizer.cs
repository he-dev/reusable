﻿using System;

namespace Reusable.Fluorite.Html;

public class HtmlSanitizer : ISanitizer
{
    public string Sanitize(object value, IFormatProvider formatProvider)
    {
        return System.Web.HttpUtility.HtmlEncode(string.Format(formatProvider, "{0}", value));
    }
}