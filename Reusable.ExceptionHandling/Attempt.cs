using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.ExceptionHandling
{
    public class Attempt
    {
        internal Attempt(Exception exception, int count)
        {
            Exception = exception;
            Count = count;
        }

        public Exception Exception { get; }

        public int Count { get; }

        public bool Handled { get; set; }
    }
}
