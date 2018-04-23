using System;
using System.Reactive;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    [UsedImplicitly]
    public class ColoredConsoleRx : LogRx
    {
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

        protected override IObserver<Log> Initialize()
        {
            return Observer.Create<Log>(log =>
            {
                var template = log.Property<string>(null, nameof(LoggerExtensions.Write));
                if (template.IsNotNullOrEmpty())
                {
                    _renderer.Render(template);
                }
            });
        }
    }
}