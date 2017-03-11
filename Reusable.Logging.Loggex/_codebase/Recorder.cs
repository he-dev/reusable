using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Logging.Loggex
{
    public interface IRecorder
    {
        void Write(LogEntry logEntry);
    }

    public class Recorder : IRecorder
    {
        public void Write(LogEntry logEntry)
        {
            throw new NotImplementedException();
        }
    }
}
