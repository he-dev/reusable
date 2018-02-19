using System;
using System.Reactive;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Collections;

namespace Reusable.ConsoleColorizer
{
    [UsedImplicitly]
    public class ConsoleTemplateRx : LogRx
    {
        private readonly IConsoleTemplateRenderer _templateRenderer;

        public ConsoleTemplateRx(IConsoleTemplateRenderer templateRenderer)
        {
            _templateRenderer = templateRenderer;
        }

        public static ConsoleTemplateRx Create(IConsoleTemplateRenderer templateRenderer)
        {
            return new ConsoleTemplateRx(templateRenderer);
        }

        protected override IObserver<Log> Initialize()
        {
            return Observer.Create<Log>(log =>
            {
                var template = log.Property<string>(null, nameof(LoggerExtensions.ConsoleMessage));
                if (template.IsNotNullOrEmpty())
                {
                    _templateRenderer.Render(template);
                }
            });
        }
    }
}