using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Reusable.Apps;
using Reusable.Utilities.NLog.LayoutRenderers;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Channels;
using Reusable.Wiretap.Conventions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap.Pipelines;

namespace Reusable;

public static partial class Examples
{
    public static void Log()
    {
        SmartPropertiesLayoutRenderer.Register();

        var pipeline =
            Logger
                .Pipeline<Default>()
                .Use(new AttachProperty(LogProperty.Names.Environment(), "Demo"))
                //.Use(new Constant<LoggableProperty.Product>("Reusable.app.Console"))
                .Use(SnapshotMapping.For<Person>(x => new { FullName = $"{x.LastName}, {x.FirstName}".ToUpper() }))
                //.Use<FormatPropertyName>(new Replace{MatchPattern = }) Correlation -> Scope
                //.Use<FormatPropertyName>(new Replace{MatchPattern = }) trim Layer$
                .Use<Channel>(new NLogChannel())
                .Use<Channel>(new ConsoleChannelDynamic
                {
                    // Render output with this template. This is the default.
                    Template = new TextMessageBuilder
                    {
                        Template = @"[{Timestamp:HH:mm:ss:fff}] [{Level}] {Layer} | {Category} | {Identifier}: {Snapshot} {Elapsed}ms | {Message} {Exception}"
                    }
                });

        using var loggerFactory = new LoggerFactory(pipeline);


        var logger = loggerFactory.CreateLogger("Demo");

        logger.Trace("Hallo Trace!");
        logger.Debug("Hallo Debug!");
        logger.Warning("Hallo Warning!");
        logger.Error("Hallo Error!");
        logger.Fatal("Hallo Fatal!");


        // Opening outer-scope.
        using (logger.BeginScope().WithName("outer"))
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
            logger.Log(Telemetry.Collect.Application().Metadata("meta", new { m = "m" }));
            logger.Log(Telemetry.Collect.Persistence().Database().Snapshot("storage", new { meta = "data" }));
            logger.Log(Telemetry.Collect.Persistence().Cloud().Snapshot("www", new { meta = "data" }));

            //logger.Scope().Exceptions.Push(new Exception());

            // Opening inner-scope.
            using (logger.BeginScope("inner").WorkItem(new { fileName = "note.txt" }))
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

                //logger.Log(Telemetry.Collect.Application().Decision().Decision("Don't do this.").Because("Disabled."));
                logger.Log(Telemetry.Collect.Application().Decision("Don't do this.", "Disabled."));

                logger.Log(Telemetry.Collect.Application().Decision("Don't do this either.", because: "It's disabled as well."));

                //logger.Scope().Exceptions.Push(new InvalidCredentialException());
            }

            //logger.Scope().Exceptions.Push(new DivideByZeroException());
            logger.Log(Telemetry.Collect.Application().Metadata("Goodbye", "Bye bye scopes!"));
        }

        using (logger.BeginScope().WithName("Transaction"))
        {
            using (logger.BeginScope().WithName("no-logs-1").UseBuffer())
            {
                logger.Information("This message is not logged.");
            }

            using (logger.BeginScope().WithName("no-logs-2").UseBuffer())
            {
                logger.Information("This message is not logged.");
                //logger.Information("This message overrides the transaction.", LoggerTransaction.Override);
            }

            using (logger.BeginScope().WithName("delayed").UseBuffer())
            {
                logger.Information("This message is delayed.");
                logger.Information("This message is delayed too.");
                logger.Information("This message is logged first.", l => l.ScopeBufferOff());
                //logger.Information("This message overrides the transaction as first.", LoggerTransaction.Override);
                logger.Scope().Buffer().Flush();
            }
        }
    }
}