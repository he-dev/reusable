using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Extensions
{
    public static class FunctionalExtensions
    {
        public static T Tee<T>(this T @this, Action<T> tee)
        {
            tee(@this);
            return @this;
        }    
    }
}
