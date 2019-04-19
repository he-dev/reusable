using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Flexo;
using Reusable.IOnymous;
using Reusable.OmniLog.Abstractions;


namespace Reusable.OmniLog
{
    public class LoggerFactoryConfiguration
    {
        private static readonly IResourceProvider CurrentDirectoryProvider = new RelativeProvider(new PhysicalFileProvider(), Path.GetDirectoryName(typeof(LoggerFactoryConfiguration).Assembly.Location));

        private static readonly IResourceProvider AbsoluteProvider = new PhysicalFileProvider();

        [JsonIgnore]
        [NotNull]
        public HashSet<ILogAttachment> Attachments { get; set; } = new HashSet<ILogAttachment>();

        [JsonProperty("LogFilter")]
        public IExpression LogPredicateExpression { get; set; }

        //[JsonIgnore]
        //public LogPredicate LogPredicate => log => LogPredicateExpression?.Invoke(new ExpressionContext().Log(log)).Value<bool>() ?? true;

        public static LoggerFactoryConfiguration Load(string fileName)
        {
            var provider = Path.IsPathRooted(fileName) ? AbsoluteProvider : CurrentDirectoryProvider;
//            using (var jsonStream = provider.GetFileInfoAsync(fileName).Result.CreateReadStream())
//            {
//                return Load(jsonStream);
//            }
            return default;
        }

//        public static LoggerFactoryConfiguration Load(Stream jsonStream, IExpressionSerializer serializer = null)
//        {
//            return (serializer ?? ExpressionSerializerFactory.CreateSerializer()).Deserialize<LoggerFactoryConfiguration>(jsonStream);
//        }
    }
}