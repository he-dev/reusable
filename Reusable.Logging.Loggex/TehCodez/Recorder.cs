using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Logging.Loggex
{
    public interface IRecorder
    {
        string Name { get; set; }

        void Log(LogEntry logEntry);
    }

    public class Recorder : IRecorder
    {
        public string Name { get; set; }

        public void Log(LogEntry logEntry)
        {
            throw new NotImplementedException();
        }
    }
}
