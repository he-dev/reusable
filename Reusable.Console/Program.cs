using Reusable.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLibs.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = LoggerFactory.CreateLogger("test");
            //logger.Trace(x => x.Message("blah"));
            LogEntry.New().Debug().Message("blah").Log(logger);
        }
    }
}
