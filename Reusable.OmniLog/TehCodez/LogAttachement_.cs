using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    public interface ILogAttachement //: IEquatable<ILogAttachement>
    {
        void Attach(Log log);
    }


    public class OmniProperty : ILogAttachement
    {
        public void Attach(Log log)
        {
            var settings =
                from key in ConfigurationManager.AppSettings.AllKeys.Where(key => key.StartsWith("omnilog:", StringComparison.OrdinalIgnoreCase))
                let value = ConfigurationManager.AppSettings[key]
                where value.IsNotNullOrEmpty()
                select (key: Regex.Replace(key, "^omnilog:", string.Empty, RegexOptions.IgnoreCase), value);

            foreach (var setting in settings)
            {
                log.With(setting.key, setting.value);
            }
        }
    }

    public class Elapsed2 : ILogAttachement
    {
        public void Attach(Log log)
        {

        }
    }
}
