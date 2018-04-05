using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reusable.Console;
using Reusable.ConsoleColorizer;
using Reusable.Converters;
using Reusable.DateTimes;
using Reusable.Diagnostics;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Extensions;
using Reusable.Utilities.ThirdParty.NLog.LayoutRenderers;

//[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(Person))]

namespace Reusable.Console
{
    public static class Demo
    {
        public static void ConsoleColorizer()
        {
            SmartPropertiesLayoutRenderer.Register();

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
            SmartPropertiesLayoutRenderer.Register();

            var loggerFactory = LoggerFactorySetup.SetupLoggerFactory("development", "Reusable.Console", new[] { NLogRx.Create() });

            var logger = loggerFactory.CreateLogger("Demo");

            logger.Log(Abstraction.Layer.Infrastructure().Action().Running("SemLogTest"));

            // Opening outer-transaction.
            using (logger.BeginScope("OuterScope", new { CustomerId = 123 }).AttachElapsed())
            {
                // Logging some single business variable and a message.
                logger.Log(Abstraction.Layer.Business().Data().Variable(new { foo = "bar" }), log => log.Message("Hallo variable!"));

                // Opening innter-transaction.
                using (logger.BeginScope("InnerScope", new { ItemId = 456 }).AttachElapsed())
                {
                    // Logging an entire object in a single line.
                    var customer = new { FirstName = "John", LastName = "Doe" };
                    logger.Log(Abstraction.Layer.Business().Data().Object(new { customer }));

                    // Logging multiple variables in a single line.
                    var baz = 123;
                    var qux = "quux";

                    logger.Log(Abstraction.Layer.Infrastructure().Data().Variable(new { baz, qux }));

                    // Logging action results.
                    logger.Log(Abstraction.Layer.Infrastructure().Action().Running("DoSomething"));
                    logger.Log(Abstraction.Layer.Infrastructure().Action().Canceled("DoSomething"), log => log.Message("No connection."));
                    logger.Log(Abstraction.Layer.Infrastructure().Action().Faulted("DoSomething"), log => log.Exception(new DivideByZeroException("Cannot divide.")));
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

        public static void DebuggerDisplay()
        {
            var person = new Person
            {
                FirstName = "John",
                LastName = null,
                Age = 123.456,
                DBNullTest = DBNull.Value,
                GraduationYears = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                Nicknames = { "Johny", "Doe" }
            };

            //var toStringNull = default(Person).ToDebuggerDisplayString(builder => { builder.Property(x => x.FirstName); });

            var toString = person.ToDebuggerDisplayString(builder =>
            {
                builder.Property(x => x.FirstName, "{0,8}");
                builder.Property(x => x.LastName);
                builder.Property(x => x.DBNullTest);
                builder.Property(x => x.GraduationYears.Sum());
                builder.Property(x => x.Age, "{0:F2}");
                builder.Property(x => x.GraduationYears.Count);
                builder.Collection(x => x.GraduationYears);
                builder.Collection(x => x.GraduationYears, x => x, "{0:X2}");
                builder.Collection(x => x.Nicknames);
            });
        }
    }

    public class Person
    {
        private string _testField = "TestValue";

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public double Age { get; set; }

        public object DBNullTest { get; set; } = DBNull.Value;

        public IList<int> GraduationYears { get; set; } = new List<int>();

        public IList<string> Nicknames { get; set; } = new List<string>();

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder =>
        {
            builder.Property(x => x.FirstName);
            builder.Property(x => x.LastName);
            builder.Property(x => x._testField);
            builder.Property(x => x.DBNullTest);
            builder.Property(x => x.Age.ToString("F2"));
            builder.Property(x => x.Age, "{0:F2}");
            builder.Property(x => x.GraduationYears.Count);
            builder.Collection(x => x.GraduationYears);
            builder.Collection(x => x.GraduationYears, x => x, "{0:X2}");
            builder.Collection(x => x.Nicknames);
        });
    }
}