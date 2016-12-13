using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SmartCommandLine
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Argument : List<string>, IGrouping<string, string>
    {
        internal Argument(string key, params string[] values)
        {
            Key = key;
            if (values != null)
            {
                AddRange(values);
            }
        }

        internal static  Argument Anonymous => new Argument(string.Empty);

        public string Key { get; }

        private string DebuggerDisplay => $"Key = '{Key}' Values = [{string.Join(", ", this)}]";

        //public IEnumerator<string> GetEnumerator() => _values.GetEnumerator();

        //IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}