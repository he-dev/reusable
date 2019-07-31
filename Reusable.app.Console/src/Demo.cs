using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.IOnymous;
using Reusable.IOnymous.Http;
using Reusable.IOnymous.Http.Mailr;
using Reusable.IOnymous.Http.Mailr.Models;
using Reusable.IOnymous.Mail;
using Reusable.IOnymous.Mail.Smtp;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Attachments;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Quickey;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.NLog.LayoutRenderers;

//[assembly: DebuggerDisplay("{DebuggerDisplay(),nq}", Target = typeof(Person))]

namespace Reusable.Apps
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
                    new ColoredConsoleRx(),
                },
                Attachments =
                {
                    new Timestamp<DateTimeUtc>(),
                }
            };

            var consoleLogger = loggerFactory.CreateLogger("ConsoleTemplateTest");

//            consoleLogger.WriteLine(m => m
//                .text(">")
//                .span(s => s.text("foo").color(ConsoleColor.Red))
//                .text(" bar ")
//                .span(s => s
//                    .text("foo ")
//                    .span(ss => ss.text("bar").backgroundColor(ConsoleColor.Gray))
//                    .text(" baz")
//                    .backgroundColor(ConsoleColor.DarkYellow)
//                )
//            );
        }

        public static void SemanticExtensions()
        {
            SmartPropertiesLayoutRenderer.Register();

            //var fileProvider = new RelativeResourceProvider(new PhysicalFileProvider(), typeof(Demo).Assembly.Location);

            var loggerFactory =
                new LoggerFactory()
                    .AttachObject("Environment", "Demo")
                    .AttachObject("Product", "Reusable.Apps.Console")
                    .AttachScope()
                    .AttachSnapshot(SimpleJsonConverter<Person>.Create(x => new { x.FirstName, x.LastName }))
                    .Attach<Timestamp<DateTimeUtc>>()
                    .AttachElapsedMilliseconds()
                    //.AddObserver<NLogRx>()
                    .AddObserver<ColoredConsoleRx>();
            // UseConverter<Something>(x => x.ToString());
            //.UseConfiguration(LoggerFactoryConfiguration.Load(fileProvider.GetFileInfo(@"cfg\omnilog.json").CreateReadStream()));

            var logger = loggerFactory.CreateLogger("Demo");
            var logger2 = loggerFactory.CreateLogger("Demo");

            if (logger != logger2) throw new Exception();

            logger.Log(Abstraction.Layer.Service().Routine("SemLogTest").Running());
            logger.Log(Abstraction.Layer.Service().Meta(new { Null = (string)null }));


            // Opening outer-transaction.
            using (logger.BeginScope().CorrelationHandle("Blub").Routine("MyRoutine").CorrelationContext(new { Name = "OuterScope", CustomerId = 123 }).AttachElapsed())
            {
                // Logging some single business variable and a message.
                logger.Log(Abstraction.Layer.Business().Variable(new { foo = "bar" }), log => log.Message("Hallo variable!"));
                logger.Log(Abstraction.Layer.Database().Counter(new { RowCount = 7 }));
                logger.Log(Abstraction.Layer.Database().Decision("blub").Because("bluby"));

                // Opening inner-transaction.
                using (logger.BeginScope().CorrelationContext(new { Name = "InnerScope", ItemId = 456 }).AttachElapsed())
                {
                    logger.Log(Abstraction.Layer.Service().RoutineFromScope().Running());

                    var correlationIds = logger.Scopes().CorrelationIds<string>().ToList();

                    // Logging an entire object in a single line.
                    //var customer = new { FirstName = "John", LastName = "Doe" };
                    var customer = new Person
                    {
                        FirstName = "John",
                        LastName = null,
                        Age = 123.456,
                        DBNullTest = DBNull.Value,
                        GraduationYears = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                        Nicknames = { "Johny", "Doe" }
                    };
                    logger.Log(Abstraction.Layer.Business().Variable(new { customer }));

                    // Logging multiple variables in a single line.
                    var baz = 123;
                    var qux = "quux";

                    logger.Log(Abstraction.Layer.Service().Composite(new { multiple = new { baz, qux } }));

                    // Logging action results.
                    logger.Log(Abstraction.Layer.Service().Routine("DoSomething").Running());
                    logger.Log(Abstraction.Layer.Service().Routine("DoSomething").Canceled().Because("No connection."));
                    logger.Log(Abstraction.Layer.Service().Routine("DoSomething").Faulted(), new DivideByZeroException("Cannot divide."));
                    logger.Log(Abstraction.Layer.Service().Decision("Don't do this.").Because("Disabled."));
                }
            }

            using (logger.BeginScope().CorrelationHandle("Transaction").AttachElapsed())
            {
                using (var tran = logger.BeginTransaction())
                {
                    tran.Information("This message is not logged.");
                }

                using (var tran = logger.BeginTransaction())
                {
                    tran.Information("This message is not logged.");
                    tran.Information("This message overrides the transaction.", LoggerTransaction.Override);
                }

                using (var tran = logger.BeginTransaction())
                {
                    tran.Information("This message is delayed.");
                    tran.Information("This message is delayed too.");
                    tran.Information("This message overrides the transaction as first.", LoggerTransaction.Override);
                    tran.Commit();
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
                builder.DisplayValue(x => x.LastName);
                builder.DisplayValue(x => x.DBNullTest);
                builder.DisplayValue(x => x.GraduationYears.Sum());
                builder.Property(x => x.Age, "{0:F2}");
                builder.DisplayValue(x => x.GraduationYears.Count);
                builder.DisplayValues(x => x.GraduationYears);
                builder.DisplayValues(x => x.GraduationYears, x => x, "{0:X2}");
                builder.DisplayValues(x => x.Nicknames);
            });
        }

        public static async Task SendEmailAsync_Smtp()
        {
            var context =
                ImmutableContainer
                    .Empty
                    .SetItem(SmtpRequestContext.Host, "localhost")
                    .SetItem(SmtpRequestContext.Port, 25);

            var smtp = new SmtpProvider();
            await smtp.SendEmailAsync(new Email<EmailSubject, EmailBody>
            {
                From = "console@test.com",
                To = { "me@test.com" },
                CC = { "you@test.com" },
                Subject = new EmailSubject { Value = "How are you?" },
                Body = new EmailBody { Value = "<p>I'm fine!</p>" },
                IsHtml = true,
                Attachments = new Dictionary<string, byte[]>()
            }, context);
        }

        public static async Task SendEmailAsync_Mailr()
        {
            var http = HttpProvider.FromBaseUri("http://localhost:7000/api");
            await http.SendEmailAsync
            (
                "v1.0/mailr/messages/plaintext",
                new UserAgent("Console", "v12"),
                new Email.Html
                {
                    From = "console@test.com",
                    To = { "me@test.com" },
                    CC = { "you@test.com" },
                    Subject = "How are you Mailr?",
                    Body = "<p>I'm great!</p>",
                    IsHtml = true,
                    Attachments = new Dictionary<string, byte[]>()
                }
            );
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
            builder.DisplayValue(x => x.FirstName);
            builder.DisplayValue(x => x.LastName);
            builder.DisplayValue(x => x._testField);
            builder.DisplayValue(x => x.DBNullTest);
            builder.DisplayValue(x => x.Age.ToString("F2"));
            builder.Property(x => x.Age, "{0:F2}");
            builder.DisplayValue(x => x.GraduationYears.Count);
            builder.DisplayValues(x => x.GraduationYears);
            builder.DisplayValues(x => x.GraduationYears, x => x, "{0:X2}");
            builder.DisplayValues(x => x.Nicknames);
        });
    }
}