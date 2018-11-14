using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Flexo;
using Reusable.Flexo.Expressions;
using Reusable.Flexo.Extensions;
using Reusable.IO;
using Reusable.OmniLog.Expressions;

namespace Reusable.OmniLog
{
    public class LoggerFactoryConfiguration
    {
        private static readonly IFileProvider CurrentDirectoryProvider = new RelativeFileProvider(new PhysicalFileProvider(), Path.GetDirectoryName(typeof(LoggerFactoryConfiguration).Assembly.Location));

        private static readonly IFileProvider AbsoluteProvider = new PhysicalFileProvider();

        [JsonIgnore]
        [NotNull]
        public HashSet<ILogAttachement> Attachements { get; set; } = new HashSet<ILogAttachement>();

        [JsonProperty("LogFilter")]
        public IExpression LogPredicateExpression { get; set; }

        [JsonIgnore]
        public LogPredicate LogPredicate => log => LogPredicateExpression?.Invoke(new ExpressionContext().Log(log)).Value<bool>() ?? true;

        public static LoggerFactoryConfiguration Load(string fileName)
        {
            var provider = Path.IsPathRooted(fileName) ? AbsoluteProvider : CurrentDirectoryProvider;
            using (var jsonStream = provider.GetFileInfoAsync(fileName).Result.CreateReadStream())
            {
                return Load(jsonStream);
            }
        }

        public static LoggerFactoryConfiguration Load(Stream jsonStream, IExpressionSerializer serializer = null)
        {
            return (serializer ?? ExpressionSerializerFactory.CreateSerializer()).Deserialize<LoggerFactoryConfiguration>(jsonStream);
        }
    }
}