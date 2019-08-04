using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.IOnymous.Http;
using Reusable.IOnymous.Http.Mailr;
using Reusable.IOnymous.Http.Mailr.Models;
using Reusable.IOnymous.Mail;
using Reusable.IOnymous.Mail.Smtp;
using Reusable.OmniLog;
using Reusable.OmniLog.Attachments;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.OmniLog.SemanticExtensions.v2;
using Reusable.OmniLog.v2.Middleware;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.NLog.LayoutRenderers;
using LoggerTransaction = Reusable.OmniLog.LoggerTransaction;
using v2 = Reusable.OmniLog.v2;

namespace Reusable.Apps.Examples
{
    public static class OmniLogV2
    {
        public static void Run()
        {
            SmartPropertiesLayoutRenderer.Register();

            //var fileProvider = new RelativeResourceProvider(new PhysicalFileProvider(), typeof(Demo).Assembly.Location);

            var loggerFactory = new v2.LoggerFactory
            {
                Receivers =
                {
                    new NullRx()
                },
                Middleware =
                {
                    new LoggerPropertySetter
                    (
                        ("Environment", "Demo"),
                        ("Product", "Reusable.Apps.Console")
                    ),
                    new LoggerAttachment
                    {
                        new Timestamp<DateTimeUtc>()
                    },
                    new LoggerSerializer(new JsonSerializer())
                }
            };


            //var u = loggerFactory.Where(l => true);

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
    }
}