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
        private CommandParameterInfo(StringSet names, bool required, int? position, char? listSeparator)
        {
            Names = names;
            Required = required;
            Position = position;
            ListSeparator = listSeparator;
        }

        public CommandParameterInfo(PropertyInfo property) 
            : this(
                  CommandParameter.GetNames(property),
                  false,
                  default(int?),
                  default(char?)
            )
        {
        }

        public StringSet  Names { get; }

        public bool Required { get; }

        public char? ListSeparator { get; }

        public int? Position { get; }
    }
}
