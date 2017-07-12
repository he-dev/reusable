using System.Configuration;

namespace Reusable.Logging.Loggex.ComputedProperties
{
    public class AppSetting : ComputedProperty
    {
        private readonly string _key;
        public AppSetting(string name, string key) : base(name) => _key = key;
        public override object Compute(LogEntry logEntry) => ConfigurationManager.AppSettings[_key];
    }
}