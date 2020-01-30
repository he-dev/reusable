using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    public class ConfigRequest : Request
    {
        public Type SettingType { get; set; } = default!;

        public IEnumerable<ValidationAttribute> ValidationAttributes { get; set; } = Enumerable.Empty<ValidationAttribute>();
    }

    public class ConfigResponse : Response { }
}