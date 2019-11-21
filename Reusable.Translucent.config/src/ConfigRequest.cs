using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Reusable.Translucent
{
    [Scheme("config")]
    public class ConfigRequest : Request
    {
        public Type SettingType { get; set; } = default!;
        
        public IEnumerable<ValidationAttribute> ValidationAttributes { get; set; } = Enumerable.Empty<ValidationAttribute>();
    }
}