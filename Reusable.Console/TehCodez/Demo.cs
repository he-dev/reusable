using System;
using System.Collections.Generic;
using Reusable.ConsoleColorizer;
using Reusable.Converters;
using Reusable.DateTimes;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Extensions;

namespace Reusable.Console
{
    public static class Demo
    {
        public static void ConsoleColorizer()
        {
            Reusable.ThirdParty.NLogUtilities.LayoutRenderers.SmartPropertiesLayoutRenderer.Register();

            var loggerFactory = new LoggerFactory
            {
                Observers =
                {
                    ConsoleTemplateRx.Create(new ConsoleTemplateRenderer()),
                },
                Configuration = new LoggerConfiguration
                {
                    Attachements = new HashSet<ILogAttachement>
                    {
                        new OmniLog.Attachements.Timestamp<UtcDateTime>(),
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
            Reusable.ThirdParty.NLogUtilities.LayoutRenderers.SmartPropertiesLayoutRenderer.Register();

            var loggerFactory = new LoggerFactory
            {
                Observers =
                {
                    ConsoleTemplateRx.Create(new ConsoleTemplateRenderer()),
                    NLogRx.Create(new [] { new LogTransactionMerge() })
                },
                Configuration = new LoggerConfiguration
                {
                    Attachements =
                    {
                        OmniLog.Attachements.AppSetting.CreateMany("omnilog:", "Environment", "Product"),
                        new OmniLog.Attachements.Timestamp<UtcDateTime>(),
                        new OmniLog.SemanticExtensions.Attachements.Snapshot()
                    }
                }
            };

            var logger = loggerFactory.CreateLogger("Demo");

            using (logger.BeginTransaction(new { OuterTransaction = 123 }, log => log.Elapsed()))
            {
                logger.Log(Abstraction.Layer.Business().Data().Variable(new { foo = "bar" }), log => log.Message("Hallo variable!"));
                using (logger.BeginTransaction(new { InnerTransaction = 456 }, log => log.Elapsed()))
                {
                    var customer = new { FirstName = "John", LastName = "Doe" };
                    logger.Log(Abstraction.Layer.Business().Data().Object(new { customer }));

                    var baz = 123;
                    var qux = "quux";

                    logger.Log(Abstraction.Layer.Infrastructure().Data().Variable(new { baz, qux }));
                    
                    //logger.Event(Layer.Application, Event.ApplicationStart, Result.Success, "Hallo event!");
                    logger.Log(Abstraction.Layer.Infrastructure().Action().Started("TestLogger"));
                    logger.Log(Abstraction.Layer.Infrastructure().Action().Cancelled("TestLogger"), log => log.Message("No connection."));
                    logger.Log(Abstraction.Layer.Infrastructure().Action().Failed("TestLogger"), log => log.Exception(new DivideByZeroException("Cannot divide.")));
                    //logger.Trace("Just a trace");
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
}