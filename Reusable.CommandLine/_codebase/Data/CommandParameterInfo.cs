using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Shelly.Data
{
    public class CommandParameterInfo
    {
        private CommandParameterInfo(StringSetCI names, bool required, int position, char listSeparator) { }

        public CommandParameterInfo(PropertyInfo property)
        {
            throw new NotImplementedException();
        }

        public StringSetCI Names { get; }

        public bool Required { get; }

        public char ListSeparator { get; }

        public int Position { get; }
    }
}
