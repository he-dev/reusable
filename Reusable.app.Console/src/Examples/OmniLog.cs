using System;
using System.Collections.Generic;
using Reusable.Apps;
using Reusable.Exceptionize;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Rx;
using Reusable.OmniLog.Rx.ConsoleRenderers;
using Reusable.OmniLog.Scalars;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Utilities.NLog.LayoutRenderers;

namespace Reusable
{
    public static partial class Examples
    {
        public static void Log()
        {
            SmartPropertiesLayoutRenderer.Register();

            var loggerFactory = new LoggerFactory
            {
                Nodes =
                {
                    // Adds constant values to each log-entry.
                    new ConstantNode
                    {
                        Values =
                        {
                            { "Environment", "Demo" },
                            { "Product", "Reusable.app.Console" }
                        }
                    },
                    // Adds elapsed time to each log-entry. Can be enabled with logger.UseStopwatch(). Dispose to disable.
//                    new StopwatchNode
//                    {
//                        // Selects milliseconds to be logged. This is the default.
//                        GetValue = elapsed => elapsed.TotalMilliseconds
//                    },
                    // Adds computed properties to each log-entry.
                    new ScalarNode
                    {
                        Functions =
                        {
                            // Adds utc timestamp to each log-entry.
                            new Timestamp<DateTimeUtc>()
                        }
                    },
                    // Adds support for logger.Log(log => ..) overload.
                    new LambdaNode(),
                    // Adds Correlation object to each log-entry.
                    //new CorrelationNode(),
                    new ScopeNode(),
                    // Copies everything from AbstractionBuilder to each log-entry.
                    // Contains properties Layer and Category and Meta#Dump.
                    new BuilderNode
                    {
                        Names =
                        {
                            nameof(Abstraction)
                        }
                    },
                    // Explodes objects and dictionaries into multiple log-entries. One per each property/item.
                    new OneToManyNode(),
                    // Converts #Serializable items. Objects and dictionaries are treated as collections of KeyValuePairs.
                    // They are added as Variable & #Serializable to each log-entry.
                    new MapperNode
                    {
                        // Maps Person to a different type.
                        Mappings =
                        {
                            MapperNode.Mapping.For<Person>(x => new { FullName = $"{x.LastName}, {x.FirstName}".ToUpper() })
                        }
                    },
                    // Serializes every #Serializable item in the log-entry and adds it as #Property.
                    new SerializerNode(),
                    // Filters log-entries and short-circuits the pipeline when False.
                    new FilterNode(logEntry => true),
                    // Renames properties.
                    new RenameNode
                    {
                        Mappings =
                        {
                            { LogEntry.Names.Scope, "Scope" },
                            { LogEntry.Names.Object, "Identifier" },
                            { LogEntry.Names.Snapshot, "Snapshot" },
                        }
                    },
                    // Sets default values for the specified keys when they are not set already. 
                    new FallbackNode
                    {
                        Defaults =
                        {
                            [LogEntry.Names.Level] = LogLevel.Information
                        }
                    },
                    // When activated, buffers log-entries until committed. Can be enabled with logger.UseTransaction(). Dispose to disable.
                    //new BufferNode(),
                    // The final node that sends log-entries to the receivers.
                    new EchoNode
                    {
                        Rx =
                        {
                            new NLogRx(), // Use NLog.
                            new ConsoleRx // Use console.
                            {
                                // Use simple console renderer with color per line. This is the default.
                                Renderer = new SimpleConsoleRenderer
                                {
                                    // Render output with this template. This is the default.
                                    Template = @"[{Timestamp:HH:mm:ss:fff}] [{Level:u}] {Layer} | {Category} | {Identifier}: {Snapshot} {Elapsed}ms | {Message} {Exception}"
                                }
                            }
                        },
                    }
                }
            };

            var logger = loggerFactory.CreateLogger("Demo");

            logger.Information("Hallo omni-log!");

            logger.Log(Abstraction.Layer.Service().Routine(nameof(Log)).Running());
            logger.Log(Abstraction.Layer.Service().Meta(new { Greeting = "Hallo omni-log!" }));
            logger.Log(Abstraction.Layer.Service().Meta(new { Null = (string)default }));

            // Opening outer-scope.
            using (logger.UseScope(correlationHandle: "my-handle").UseStopwatch())
            {
                var variable = new { John = "Doe" };
                // Logging some single business variable and a message.
                logger.Log(Abstraction.Layer.Business().Variable(variable), log => log.Message("I'm a variable!"));
                logger.Log(Abstraction.Layer.Database().Counter(new { Prime = 7 }));
                logger.Log(Abstraction.Layer.Database().Flow().Decision("Log something.").Because("Logger works!"));

                // Opening inner-scope.
                using (logger.UseScope().UseStopwatch())
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
                    logger.Log(Abstraction.Layer.Service().Routine(nameof(Log)).Running());
                    logger.Log(Abstraction.Layer.Service().Routine(nameof(Log)).Canceled().Because("No connection."));
                    logger.Log(Abstraction.Layer.Service().Routine(nameof(Log)).Faulted(DynamicException.Create("Test", "This is only a test.")));
                    logger.Log(Abstraction.Layer.Service().Flow().Decision("Don't do this.").Because("Disabled."));
                }
            }

            using (logger.UseScope(correlationHandle: "Transaction").UseStopwatch())
            {
                using (logger.UseScope().UseBuffer())
                {
                    logger.Information("This message is not logged.");
                }

                using (logger.UseScope().UseBuffer())
                {
                    logger.Information("This message is not logged.");
                    //logger.Information("This message overrides the transaction.", LoggerTransaction.Override);
                }

                using (logger.UseScope().UseBuffer())
                {
                    logger.Information("This message is delayed.");
                    logger.Information("This message is delayed too.");
                    //logger.Information("This message overrides the transaction as first.", LoggerTransaction.Override);
                    logger.Scope().Buffer().Flush();
                }
            }
        }
    }
}