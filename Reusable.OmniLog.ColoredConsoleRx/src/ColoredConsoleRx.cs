using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    [UsedImplicitly]
    public class ColoredConsoleRx : LogRx
    {
        public static readonly string TemplatePropertyName = $"{nameof(ColoredConsoleRx)}.Template";

        private readonly IConsoleRenderer _renderer;

        public ColoredConsoleRx(IConsoleRenderer renderer)
        {
            _renderer = renderer;
        }

        public ColoredConsoleRx()
            : this(new ConsoleRenderer())
        { }

        public static ColoredConsoleRx Create(IConsoleRenderer renderer)
        {
            return new ColoredConsoleRx(renderer);
        }

        protected override void Log(ILog log)
        {
            var template = log.GetItemOrDefault<string>(TemplatePropertyName);
            if (template.IsNotNullOrEmpty())
            {
                _renderer.Render(template);
            }
        }
    }
}