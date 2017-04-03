using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable
{
    public class ExceptionStringBuilder
    {
        private readonly StringBuilder _exceptionString = new StringBuilder(1024);

        private readonly int _indentWidth;

        public const int DefaultIndentWidth = 4;

        public ExceptionStringBuilder() : this(DefaultIndentWidth) { }

        public ExceptionStringBuilder(int indentWidth)
        {
            _indentWidth = indentWidth;
        }

        public ExceptionStringBuilder AppendExceptionMessage(Exception ex, int depth)
        {
            _exceptionString.Append(Indent(0, depth)).AppendLine($"{ex.GetType().Name}: \"{ex.Message}\"");
            return this;
        }

        public ExceptionStringBuilder AppendInnerExceptionCount(Exception ex, int depth)
        {
            if (ex is AggregateException aex)
            {
                _exceptionString.Append(Indent(1, depth)).AppendLine($"InnerExceptions: \"{aex.InnerExceptions.Count}\"");
            }
            return this;
        }

        public ExceptionStringBuilder AppendExceptionProperties(Exception ex, int depth)
        {
            foreach (var property in ex.GetPropertiesExcept<Exception>())
            {
                _exceptionString.Append(Indent(1, depth)).AppendLine($"{property.Name}: \"{property.Value}\"");
            }
            return this;
        }

        public ExceptionStringBuilder AppendExceptionData(Exception ex, int depth)
        {
            foreach (var property in ex.GetData())
            {
                _exceptionString.Append(Indent(1, depth)).AppendLine($"Data[{property.Key}]: \"{property.Value}\"");
            }
            return this;
        }

        public ExceptionStringBuilder AppendExceptionStackTrace(Exception ex, int depth)
        {
            _exceptionString.Append(Indent(1, depth)).AppendLine($"StackTrace:");

            foreach (var stackTrace in ex.GetStackTrace())
            {
                _exceptionString.Append(Indent(2, depth)).AppendLine($"{stackTrace.Caller} in \"{stackTrace.FileName}\" Ln {stackTrace.LineNumber}");
            }
            return this;
        }

        private string Indent(int baseDepth, int currentDepth) => new string(' ', _indentWidth * (baseDepth + currentDepth));

        public static implicit operator string(ExceptionStringBuilder builder) => builder._exceptionString.ToString();

        public static implicit operator StringBuilder(ExceptionStringBuilder builder) => builder._exceptionString;
    }
}
