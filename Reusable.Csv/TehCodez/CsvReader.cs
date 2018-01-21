using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.FileFormats.Csv
{
    [PublicAPI]
    public interface ICsvReader : IDisposable
    {
        Task<IList<string>> ReadLineAsync();
    }

    public class CsvReader : ICsvReader
    {
        public const char DefaultSeparator = ';';

        private const char DoubleQuote = '"';
        private const char CarriageReturn = '\r';
        private const char LineFeed = '\n';
        private const int CharCount = 1;
        private const int NoCharRead = 0;

        private readonly TextReader _csv;
        private readonly char _separator;
        private readonly char[] _current = new char[1];
        private int _position;
        private bool _isEndOfLine;
        private bool _isEndOfStream;

        public CsvReader(TextReader csv, char separator = DefaultSeparator)
        {
            _csv = csv;
            _separator = separator;
        }

        private char Current => _current[0];

        public Task<IList<string>> ReadLineAsync()
        {
            if (_isEndOfStream)
            {
                return Task.FromResult((IList<string>)null); ;
            }

            _isEndOfLine = false;

            var line =
                enumerable
                    .Always(async () => await ReadFieldAsync())
                    .TakeWhile(field => field.Result != null)
                    .Select(t => t.Result)
                    .ToList();

            return Task.FromResult((IList<string>)line);
        }

        private async Task<string> ReadFieldAsync()
        {
            if (_isEndOfStream || _isEndOfLine)
            {
                return null;
            }

            var isQuoted = false;
            var field = new StringBuilder();

            while (await MoveNextAsync())
            {
                if (Current == DoubleQuote)
                {
                    // Ignore the first double-quote.
                    if (!await MoveNextAsync())
                    {
                        return field.ToString();
                    }

                    // Double-quote not followed by another double-quote means the filed is quoted.
                    if (Current != DoubleQuote)
                    {
                        isQuoted = !isQuoted;
                    }
                }

                // Use only not-quoted separators for splitting.
                if (Current == _separator && !isQuoted)
                {
                    return field.ToString();
                }

                if (Current == CarriageReturn && !isQuoted)
                {
                    // Ignore carragie-return.
                    if (!await MoveNextAsync())
                    {
                        throw new ArgumentException($"Missing line-feed at {_position}.");
                    }

                    if (Current == LineFeed)
                    {
                        _isEndOfLine = true;
                        return field.ToString();
                    }

                    throw new ArgumentException($"Invalid character at {_position}. Expected '\\n' (line-feed) but found '{Current}'.");
                }

                field.Append(Current);
            }

            return field.ToString();
        }

        private async Task<bool> MoveNextAsync()
        {
            if (!_isEndOfStream)
            {
                _position++;
            }

            // Read one character at a time.
            return !(_isEndOfStream = await _csv.ReadAsync(_current, 0, CharCount) == NoCharRead);
        }

        [NotNull]
        public static CsvReader FromFile([NotNull] string path, char separator = DefaultSeparator)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return new CsvReader(File.OpenText(path), separator);
        }

        [NotNull]
        public static CsvReader FromString([NotNull] string csv, char separator = DefaultSeparator, Encoding encoding = null)
        {
            if (csv == null) throw new ArgumentNullException(nameof(csv));
            return new CsvReader(csv.ToStreamReader(encoding ?? Encoding.UTF8), separator);
        }

        public void Dispose()
        {
            _csv.Dispose();
        }
    }

    // This interface supports dependency-injection.
    public interface ICsvReaderFactory
    {
        [NotNull]
        ICsvReader CreateFromStream([NotNull] TextReader csv, char separator = CsvReader.DefaultSeparator);

        [NotNull]
        ICsvReader CreateFromFile([NotNull] string path, char separator = CsvReader.DefaultSeparator);

        [NotNull]
        ICsvReader CreateFromString([NotNull] string csv, char separator = CsvReader.DefaultSeparator, Encoding encoding = null);
    }

    public class CsvReaderFactory : ICsvReaderFactory
    {
        public ICsvReader CreateFromStream([NotNull] TextReader csv, char separator = CsvReader.DefaultSeparator)
        {
            return new CsvReader(csv, separator);
        }

        public ICsvReader CreateFromFile(string path, char separator = CsvReader.DefaultSeparator)
        {
            return CsvReader.FromFile(path, separator);
        }

        public ICsvReader CreateFromString(string csv, char separator = CsvReader.DefaultSeparator, Encoding encoding = null)
        {
            return CsvReader.FromString(csv, separator, encoding);
        }
    }
}
