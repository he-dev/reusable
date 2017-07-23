using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable
{
    public static class VersionValidator
    {
        public static void ValidateMinVersion(params int[] versions)
        {
            if (versions.Any(x => x < 0)) throw new VersionOutOfRangeException();
        }
    }
}
