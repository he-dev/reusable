using System;
using System.Reactive;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Collections;

namespace Reusable.ConsoleColorizer
{
    [UsedImplicitly]
    public class ConsoleTemplateRx
    {
        public static IObserver<Log> Create(IConsoleTemplateRenderer templateRenderer)
        {
            return Observer.Create<Log>(log =>
            {
                var template = log.Property<string>(null, nameof(LoggerExtensions.ConsoleMessage));
                if (template.IsNotNullOrEmpty())
                {
                    templateRenderer.Render(template);
                }
            });
        }
    }
}