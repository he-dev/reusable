using System;
using System.Collections.Generic;
using Reusable.Apps;
using Reusable.Exceptionize;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Connectors;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;
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
                    .Default
                    .Configure<AttachProperty>(node =>
                    {
                        node.Properties.Add(new Constant("Environment", "Demo"));
                        node.Properties.Add(new Constant("Product", "Reusable.app.Console"));
                    })
                    // .Configure<PropertyNode>().Add
                    // (
                    //     node => node.Properties,
                    //     new Constant("Environment", "Demo"),
                    //     new Constant("Product", "Reusable.app.Console")
                    // )
                    .Configure<MapObject>(node => { node.Mappings.Add(MapObject.Mapping.For<Person>(x => new { FullName = $"{x.LastName}, {x.FirstName}".ToUpper() })); })
                    .Configure<RenameProperty>(node =>
                    {
                        node.Mappings.Add(Names.Properties.Correlation, "Scope");
                        node.Mappings.Add(Names.Properties.Unit, "Identifier");
                    })
                    // .Configure<PropertyMapperNode>().Add
                    // (
                    //     node => node.Mappings,
                    //     (Names.Properties.Correlation, "Scope"),
                    //     (Names.Properties.SnapshotName, "Identifier")
                    // )
                    .Configure<Echo>(node =>
                    {
                        node.Connectors.Add(new NLogConnector());
                        node.Connectors.Add(new SimpleConsoleRx
                        {
                            // Render output with this template. This is the default.
                            Template = @"[{Timestamp:HH:mm:ss:fff}] [{Level}] {Layer} | {Category} | {Identifier}: {Snapshot} {Elapsed}ms | {Message} {Exception}"
                        });
                    })
                    // .Configure<EchoNode>().Add
                    // (
                    //     node => node.Connectors,
                    //     new NLogConnector(),
                    //     new SimpleConsoleRx
                    //     {
                    //         // Render output with this template. This is the default.
                    //         Template = @"[{Timestamp:HH:mm:ss:fff}] [{Level}] {Layer} | {Category} | {Identifier}: {Snapshot} {Elapsed}ms | {Message} {Exception}"
                    //     }
                    // )
                    .ToLoggerFactory();
            ;
            var logger = loggerFactory.CreateLogger("Demo");

            logger.Information("Hallo omni-log!");

            //logger.Log(Abstraction.Layer.Service().Routine(nameof(Log)).Running());
            logger.Log(Abstraction.Layer.Service().Meta(new { Greeting = "Hallo omni-log!" }));
            logger.Log(Abstraction.Layer.Service().Meta(new { Null = (string)default }));

            // Opening outer-scope.
            using (logger.BeginScope("outer").WithCorrelationHandle("outer").UseStopwatch())
            {
                var variable = new { John = "Doe" };
                // Logging some single business variable and a message.
                logger.Log(Abstraction.Layer.Business().Variable(variable).Message("I'm a variable!"));
                logger.Log(Abstraction.Layer.Database().Counter(new { Prime = 7 }));
                logger.Log(Abstraction.Layer.Database().Flow().Decision("Log something.").Because("Logger works!"));
                logger.Log(Abstraction.Layer.Database().Flow("Log something.", because: "Logger works!"));
                //logger.Log(Layer.Service, Category.WorkItem, new { test = new { fileName = "test" } });
                //logger.Log(Layer.Service, Category.WorkItem, Snapshot.Take("testFile", new { fileName = "test" }), CallerInfo.Create());
                //logger.Log(Layer.Service, Category.WorkItem, new { testFile = new { fileName = "test" } }, CallSite.Create());

                logger.Log(Execution.Context.WorkItem("testFile", new { fileName = "test" }).Message("Blub!"));
                logger.Log(Execution.Context.Routine(nameof(Log)).Message("Blub!"));
                //logger.Scope().Flow().Push(new Exception());
                logger.Scope().Push(new Exception());


                // Opening inner-scope.
                using (logger.BeginScope("inner").WithCorrelationHandle("inner").UseStopwatch())
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
                    logger.Log(Abstraction.Layer.Business().Meta(new { customer }));

                    // Logging action results.
                    //logger.Log(Abstraction.Layer.Service().Routine(nameof(Log)).Running());
                    //logger.Log(Abstraction.Layer.Service().Routine(nameof(Log)).Canceled().Because("No connection."));
                    //logger.Log(Abstraction.Layer.Service().Routine(nameof(Log)).Faulted(DynamicException.Create("Test", "This is only a test.")));
                    logger.Log(Abstraction.Layer.Service().Flow().Decision("Don't do this.").Because("Disabled."));
                    logger.Log(Layer.Service, Decision.Make("Don't do this either.", because: "It's disabled as well."));
                }

                logger.Log(Abstraction.Layer.Service().Meta(new { GoodBye = "Bye bye scopes!" }));
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