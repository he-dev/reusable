using System;
using System.Collections.Generic;
using System.Security.Authentication;
using Reusable.Apps;
using Reusable.Exceptionize;
using Reusable.Utilities.NLog.LayoutRenderers;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Connectors;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap.Services.Properties;

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
                    .Configure<InvokePropertyService>(node =>
                    {
                        node.Services.Add(new Constant("Environment", "Demo"));
                        node.Services.Add(new Constant("Product", "Reusable.app.Console"));
                    })
                    .Configure<MapSnapshot>(node =>
                    {
                        node.Mappings.Add<Person>(x => new
                        {
                            FullName = $"{x.LastName}, {x.FirstName}".ToUpper()
                        });
                    })
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

            var logger = loggerFactory.CreateLogger("Demo");

            logger.Trace("Hallo Trace!");
            logger.Debug("Hallo Debug!");
            logger.Warning("Hallo Warning!");
            logger.Error("Hallo Error!");
            logger.Fatal("Hallo Fatal!");


            // Opening outer-scope.
            using (logger.BeginScope("outer").WithCorrelationHandle("outer"))
            {
                logger.Log(Telemetry.Collect.Application().Argument("arg", "a"));
                logger.Log(Telemetry.Collect.Application().Variable("var", "v").Warning());
                logger.Log(Telemetry.Collect.Application().Property("prop", "p"));
                logger.Log(Telemetry.Collect.Application().Metadata("meta", new { m = "m" }));
                logger.Log(Telemetry.Collect.Application().Metadata("Nothing", new { Null = (string)default }));
                logger.Log(Telemetry.Collect.Application().Metric("LayerCount", 4));
                logger.Log(Telemetry.Collect.Application().Metric("Performance", "excellent"));
                logger.Log(Telemetry.Collect.Business().Metadata("meta", new { m = "m" }));
                logger.Log(Telemetry.Collect.Presentation().Metadata("meta", new { m = "m" }));
                logger.Log(Telemetry.Collect.Dependency().File().Metadata("meta", new { m = "m" }));
                logger.Log(Telemetry.Collect.Dependency().Database().Metadata("storage", new { meta = "data" }));
                logger.Log(Telemetry.Collect.Dependency().Http().Metadata("www", new { meta = "data" }));
                logger.Log(Telemetry.Collect.Dependency().Network().Metadata("ip", new { meta = "data" }));
                logger.Log(Telemetry.Collect.Dependency().Resource().Metadata("image", new { meta = "data" }));
                logger.Log(Telemetry.Collect.Dependency().File().Metadata("note", new { meta = "data" }));

                logger.Scope().Exceptions.Push(new Exception());

                // Opening inner-scope.
                using (logger.BeginScope("inner", new { fileName = "note.txt" }).WithCorrelationHandle("inner"))
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

                    logger.Log(Telemetry.Collect.Application().Logic().Decision("Don't do this.").Because("Disabled."));
                    logger.Log(Layer.Service, Decision.Make("Don't do this either.", because: "It's disabled as well."));

                    logger.Scope().Exceptions.Push(new InvalidCredentialException());
                }

                logger.Scope().Exceptions.Push(new DivideByZeroException());
                logger.Log(Telemetry.Collect.Application().Metadata("Goodbye", "Bye bye scopes!"));
            }

            using (logger.BeginScope("Transaction"))
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
                    logger.Information("This message is logged first.", l => l.OverrideBuffer());
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