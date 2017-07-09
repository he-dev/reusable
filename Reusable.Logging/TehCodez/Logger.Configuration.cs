using System.Collections.Generic;
using Reusable.Loggex.Collections;

namespace Reusable.Loggex
{
    public class LoggerConfiguration
    {
        public List<IRecorder> Recorders { get; set; } = new List<IRecorder>();

        public List<ILogFilter> Filters { get; set; } = new List<ILogFilter>();

        public ComputedPropertyCollection ComputedProperties { get; } = new ComputedPropertyCollection();
    }
}