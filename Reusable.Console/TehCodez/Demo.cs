using System;
using System.Collections.Generic;
using Reusable.ConsoleColorizer;
using Reusable.Converters;
using Reusable.DateTimes;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Attachements;
using Reusable.OmniLog.SemLog;
using Reusable.OmniLog.SemLog.Attachements;

namespace Reusable.Console
{
    public static class Demo
    {
        public static void ConsoleColorizer()
        {
            Reusable.ThirdParty.NLogUtilities.LayoutRenderers.IgnoreCaseEventPropertiesLayoutRenderer.Register();

            var loggerFactory = new LoggerFactory(new[]
            {
                ConsoleTemplateRx.Create(new ConsoleTemplateRenderer()),
            })
            {
                Configuration = new LoggerConfiguration
                {
                    Attachements = new HashSet<ILogAttachement>
                    {
                        new Timestamp<UtcDateTime>(),
                    }
                }
            };

            var consoleLogger = loggerFactory.CreateLogger("ConsoleTemplateTest");

            consoleLogger.ConsoleMessageLine(m => m
                .text(">")
                .span(s => s.text("foo").color(ConsoleColor.Red))
                .text(" bar ")
                .span(s => s
                    .text("foo ")
                    .span(ss => ss.text("bar").backgroundColor(ConsoleColor.Gray))
                    .text(" baz")
                    .backgroundColor(ConsoleColor.DarkYellow)
                )
            );
        }

        public static void SemLog()
        {
            Reusable.ThirdParty.NLogUtilities.LayoutRenderers.IgnoreCaseEventPropertiesLayoutRenderer.Register();

            var loggerFactory = new LoggerFactory(new[]
            {
                ConsoleTemplateRx.Create(new ConsoleTemplateRenderer()),
                NLogRx.Create(new [] { new TransactionMerge() })
            })
            {
                Configuration = new LoggerConfiguration
                {
                    Attachements = new HashSet<ILogAttachement>(AppSetting.FromAppConfig("omnilog:", "Environment", "Product"))
                    {
                        new Timestamp<UtcDateTime>(),
                        new Snapshot()
                        //new Expected(),
                        //new Actual(),
                        //new AreEqual()
                    }
                }
            };          

            var logger = loggerFactory.CreateLogger("Demo");

            //for (int i = 0; i < 10000; i++)
            {
                using (logger.BeginScope(s => s.Transaction(123).Elapsed()))
                {
                    logger.State(Layer.Business, () => ("foo", "bar", "Hallo state!"), LogLevel.Warning);
                    using (logger.BeginScope(s => s.Transaction(456).Elapsed()))
                    {
                        logger.State(Layer.Business, () => ("Customer", new { FirstName = "John", LastName = "Doe" }, "Hallo state!"));
                        logger.Event(Layer.Application, Event.ApplicationStart, Result.Success, "Hallo event!");
                        logger.Trace("Just a trace");
                    }
                }
            }
        }

        public static void Converters()
        {
            var converter =
                TypeConverter.Empty
                    .Add<StringToInt32Converter>()
                    .Add<StringToDateTimeConverter>();

            var result = converter.Convert("123", typeof(int));
        }
    }

    public static class Event
    {
        public const string ApplicationStart = nameof(ApplicationStart);
        public const string ApplicationExit = nameof(ApplicationExit);
        public const string InitializeConfiguration = nameof(InitializeConfiguration);
        public const string InitializeContainer = nameof(InitializeContainer);
    }
}