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

        public static void SemanticExtensions()
        {
            Reusable.ThirdParty.NLogUtilities.LayoutRenderers.SmartPropertiesLayoutRenderer.Register();

            var loggerFactory = LoggerFactorySetup.SetupLoggerFactory("development", "Reusable.Console", NLogRx.Create);         

            var logger = loggerFactory.CreateLogger("Demo");

            // Opening outer-transaction.
            using (logger.BeginTransaction(new { OuterTransaction = 123 }))
            {
                // Logging some single business variable and a message.
                logger.Log(Abstraction.Layer.Business().Data().Variable(new { foo = "bar" }), log => log.Message("Hallo variable!"));

                // Opening innter-transaction.
                using (logger.BeginTransaction(new { InnerTransaction = 456 }))
                {
                    // Logging an entire object in a single line.
                    var customer = new { FirstName = "John", LastName = "Doe" };
                    logger.Log(Abstraction.Layer.Business().Data().Object(new { customer }));

                    // Logging multiple variables in a single line.
                    var baz = 123;
                    var qux = "quux";

                    logger.Log(Abstraction.Layer.Infrastructure().Data().Variable(new { baz, qux }));
                    
                    // Logging action results.
                    logger.Log(Abstraction.Layer.Infrastructure().Action().Started("DoSomething"));
                    logger.Log(Abstraction.Layer.Infrastructure().Action().Cancelled("DoSomething"), log => log.Message("No connection."));
                    logger.Log(Abstraction.Layer.Infrastructure().Action().Failed("DoSomething"), log => log.Exception(new DivideByZeroException("Cannot divide.")));

                    logger.Commit();
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