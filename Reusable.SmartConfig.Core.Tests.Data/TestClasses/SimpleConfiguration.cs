using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.SmartConfig.Annotations;

namespace Reusable.SmartConfig.Tests.Common.Configurations
{
    public class SimpleConfiguration
    {
        [SimpleSetting]
        public string SimpleSetting { get; set; }
    }
}
