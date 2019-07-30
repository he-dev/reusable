using System.Reflection;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.OmniLog.Console
{
    public abstract class Model
    {
        public abstract HtmlElement Template { get; }

        public static Model Null { get; } = default;
    }
}

namespace Reusable.OmniLog
{
    [UsedImplicitly]
    public class ColoredConsoleRx : LogRx
    {
        //public static readonly string TemplatePropertyName = $"{nameof(ColoredConsoleRx)}.Template";

        private readonly IConsoleRenderer _renderer;

        public ColoredConsoleRx(IConsoleRenderer renderer)
        {
            _renderer = renderer;
        }

        public ColoredConsoleRx()
            : this(new ConsoleRenderer()) { }

        public static ColoredConsoleRx Create(IConsoleRenderer renderer)
        {
            return new ColoredConsoleRx(renderer);
        }

        protected override void Log(ILog log)
        {
            if (log.ConsoleTemplate() is var template && template.IsNotNullOrEmpty())
            {
                _renderer.Render(template.Format(log.ConsoleModel()));
            }
        }
    }

    public static class LoggerExtensions
    {
        public static ConsoleLogger Console(this ILogger logger)
        {
            return new ConsoleLogger(logger);
        }

        public static void Log<T>(this ConsoleLogger console, T model, string template = default, LogLevel level = default) where T : Reusable.OmniLog.Console.Model
        {
            console.Log(log => log
                .Level(level ?? LogLevel.Information)
                .ConsoleModel(model)
                // Logs empty line if model is null.
                .ConsoleTemplate(model == Reusable.OmniLog.Console.Model.Null ? HtmlElement.Builder.p() : (template ?? model.Template)));
        }
    }

    public class ConsoleLogger : ILogger
    {
        private readonly ILogger _logger;

        public ConsoleLogger(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger Log(TransformCallback request, TransformCallback response = default) => _logger.Log(request, response);

        public ILogger Log(ILog log) => _logger.Log(log);

        public void Dispose() => _logger.Dispose();
    }

    public static class LogExtensions
    {
        private static readonly string FallbackTemplate = HtmlElement.Builder.p(p => p.text("{text}")).ToHtml();

        public static ILog ConsoleTemplate(this ILog log, string template)
        {
            return log.SetItem(nameof(ConsoleTemplate), template);
        }

        public static string ConsoleTemplate(this ILog log)
        {
            return log.GetItemOrDefault<string>(nameof(ConsoleTemplate));
        }

        public static ILog ConsoleModel(this ILog log, object data)
        {
            return log.SetItem(nameof(ConsoleModel), data);
        }

        public static object ConsoleModel(this ILog log)
        {
            return log.GetItemOrDefault<object>(nameof(ConsoleModel), new { });
        }
    }
}