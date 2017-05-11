using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Reusable.Extensions;

namespace Reusable.Management
{
    public static class Wmi
    {
        public static IEnumerable<string> GetCommandLines(string processName)
        {
            if (processName.IsNullOrEmpty()) throw new ArgumentNullException(nameof(processName));

            var query = $"SELECT CommandLine FROM Win32_Process WHERE Name = '{processName}'";
            using (var searcher = new ManagementObjectSearcher(query))
            {
                foreach (var instance in searcher.Get())
                {
                    yield return instance["CommandLine"].ToString();
                }
            }
        }
    }
}
