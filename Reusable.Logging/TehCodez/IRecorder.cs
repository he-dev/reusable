using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Reusable.Loggex
{
    public interface IRecorder
    {
        CaseInsensitiveString Name { get; set; }

        void Log(LogEntry logEntry);
    }
}
