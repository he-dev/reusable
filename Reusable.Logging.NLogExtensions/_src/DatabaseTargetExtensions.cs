using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Layouts;
using NLog.Targets;

namespace Reusable.Logging.NLogExtensions
{
    public static class DatabaseTargetExtensions
    {
        public static string CommandText(this DatabaseTarget databaseTarget)
        {
            return ((SimpleLayout)databaseTarget.CommandText).OriginalText;
        }
    }
}
