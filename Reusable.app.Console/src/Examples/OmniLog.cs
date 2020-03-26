using System;
using System.Collections.Generic;
using System.Security.Authentication;
using Reusable.Apps;
using Reusable.Exceptionize;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Connectors;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Properties;
using Reusable.OmniLog.Utilities;
using Reusable.OmniLog.Utilities.Logging;
using Reusable.Utilities.NLog.LayoutRenderers;

namespace Reusable
{
    public static partial class Examples
    {
        public static void Log()
        {
            SmartPropertiesLayoutRenderer.Register();

            using var loggerFactory =
                LoggerPipelines
                    .Complete
                    .Configure<AttachProperty>(node =>
                    {
                        node.Properties.Add(new Constant("Environment", "Demo"));
                        node.Properties.Add(new Constant("Product", "Reusable.app.Console"));
                    })
                    .Configure<MapObject>(node => { node.Mappings.Add(MapObject.Mapping.For<Person>(x => new { FullName = $"{x.LastName}, {x.FirstName}".ToUpper() })); })
                    .Configure<RenameProperty>(node =>
                    {
                        node.Mappings.Add(Names.Properties.Correlation, "Scope");
                        node.Mappings.Add(Names.Properties.Unit, "Identifier");
                    })
                    .Configure<Echo>(node =>
                    {
                        node.Connectors.Add(new NLogConnector());
                        node.Connectors.Add(new SimpleConsoleRx
                        {
                            // Render output with this template. This is the default.
                            Template = @"[{Timestamp:HH:mm:ss:fff}] [{Level}] {Layer} | {Category} | {Identifier}: {Snapshot} {Elapsed}ms | {Message} {Exception}"
                        });
                    })
                    .ToLoggerFactory();
            ;
            var logger = loggerFactory.CreateLogger("Demo");

            logger.Information("Hallo omni-log!");

            //logger.Log(Abstraction.Layer.Service().Routine(nameof(Log)).Running());
            logger.Log(Telemetry.Collect.Application().Metadata("Greeting", "Hallo omni-log!"));
            logger.Log(Telemetry.Collect.Application().Metadata("Nothing", new { Null = (string)default }));

            // Opening outer-scope.
            using (logger.BeginScope("outer").WithCorrelationHandle("outer").UseStopwatch())
            {
                var variable = new { John = "Doe" };
                // Logging some single business variable and a message.
                logger.Log(Telemetry.Collect.Business().Variable("person", variable).Message("I'm a variable!"));
                logger.Log(Telemetry.Collect.Dependency().Database().Unit("localdb"));
                logger.Log(Telemetry.Collect.Application().Metric("prime", 7));
                logger.Log(Telemetry.Collect.Business().Logic().Decision("Log something.", "Logger works!"));
                //logger.Log(Layer.Service, Category.WorkItem, new { test = new { fileName = "test" } });
                //logger.Log(Layer.Service, Category.WorkItem, Snapshot.Take("testFile", new { fileName = "test" }), CallerInfo.Create());
                //logger.Log(Layer.Service, Category.WorkItem, new { testFile = new { fileName = "test" } }, CallSite.Create());

                logger.Log(Telemetry.Collect.Business().WorkItem("testFile", new { fileName = "test" }).Message("Blub!"));

                //logger.Scope().Flow().Push(new Exception());
                logger.Scope().Exceptions.Push(new Exception());

                // Opening inner-scope.
                using (logger.BeginScope("inner", new { fileName = "note.txt" }).WithCorrelationHandle("inner").UseStopwatch())
                {
                    // Logging an entire object in a single line.
                    var customer = new Person
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Age = 123.456,
                        DBNullTest = DBNull.Value,
                        GraduationYears = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                        Nicknames = { "Johny", "Doe" }
                    };
                    logger.Log(Telemetry.Collect.Business().Metadata("customer", customer));

                    // Logging action results.
                    //logger.Log(Telemetry.Data.Service().Routine(nameof(Log)).Running());
                    //logger.Log(Telemetry.Data.Service().Routine(nameof(Log)).Canceled().Because("No connection."));
                    //logger.Log(Telemetry.Data.Service().Routine(nameof(Log)).Faulted(DynamicException.Create("Test", "This is only a test.")));
                    logger.Log(Telemetry.Collect.Application().Logic().Decision("Don't do this.", "Disabled."));
                    logger.Log(Layer.Service, Decision.Make("Don't do this either.", because: "It's disabled as well."));

                    logger.Scope().Exceptions.Push(new InvalidCredentialException());
                }

                logger.Scope().Exceptions.Push(new DivideByZeroException());
                logger.Log(Telemetry.Collect.Application().Metadata("Goodbye", "Bye bye scopes!"));
            }

            using (logger.BeginScope("Transaction").UseStopwatch())
            {
                using (logger.BeginScope("no-logs-1").UseBuffer())
                {
                    logger.Information("This message is not logged.");
                }

                using (logger.BeginScope("no-logs-2").UseBuffer())
                {
                    logger.Information("This message is not logged.");
                    //logger.Information("This message overrides the transaction.", LoggerTransaction.Override);
                }

                using (logger.BeginScope("delayed").UseBuffer())
                {
                    logger.Information("This message is delayed.");
                    logger.Information("This message is delayed too.");
                    //logger.Information("This message overrides the transaction as first.", LoggerTransaction.Override);
                    logger.Scope().Buffer().Flush();
                }
            }
        }
    }

    public enum Layer
    {
        Service
    }
}