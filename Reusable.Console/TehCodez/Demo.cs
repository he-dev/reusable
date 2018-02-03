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

            var abstractionContext = Abstraction.Layer.Business().Action().Failed("Main");

            //for (int i = 0; i < 10000; i++)
            {
                //using (logger.BeginTransaction(new { OuterTransaction = 123 }, log => log.Elapsed()))
                //{
                //    logger.Log(Category.Data.Variable("foo", "bar"), Layer.Business, log => log.Message("Hallo variable!"));
                //    using (logger.BeginTransaction(new { InnerTransaction = 456 }, log => log.Elapsed()))
                //    {
                //        var customer = new { FirstName = "John", LastName = "Doe" };
                //        logger.Log(Category.Data.Object(customer, nameof(customer)), Layer.Business);
                //        //logger.Event(Layer.Application, Event.ApplicationStart, Result.Success, "Hallo event!");
                //        logger.Log(Category.Action.Started("TestLogger"), Layer.Application);
                //        logger.Log(Category.Action.Cancelled("TestLogger", "No connection."), Layer.Application);
                //        logger.Log(Category.Action.Failed("TestLogger", new DivideByZeroException("Cannot divide.")), Layer.Application);
                //        //logger.Trace("Just a trace");
                //    }
                //}
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