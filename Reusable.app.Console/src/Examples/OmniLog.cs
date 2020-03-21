using System;
using Reusable.Apps;
using Reusable.Exceptionize;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Properties;
using Reusable.OmniLog.Utilities;
using Reusable.Utilities.NLog.LayoutRenderers;

namespace Reusable
{
    using Reusable.OmniLog.SemanticExtensions;

    public static partial class Examples
    {
        public static void Log()
        {
            SmartPropertiesLayoutRenderer.Register();

            var loggerFactoryBuilder = new LoggerFactoryBuilder
            {
                // Adds computed properties to each log-entry.
                new PropertyNode
                {
                    Properties =
                    {
                        new Constant("Environment", "Demo"),
                        new Constant("Product", "Reusable.app.Console"),
                        // Adds utc timestamp to each log-entry.
                        new Timestamp<DateTimeUtc>()
                    }
                },
                // Adds support for logger.Log(log => ..) overload.
                new DelegateNode(),
                new PropertyFactoryNode(),
                // Copies everything from AbstractionBuilder to each log-entry.
                // Contains properties Layer and Category and Meta#Dump.
                new BuilderNode(),
                // Explodes objects and dictionaries into multiple log-entries. One per each property/item.
                new DestructureNode(),
                // Converts #Serializable items. Objects and dictionaries are treated as collections of KeyValuePairs.
                // They are added as Variable & #Serializable to each log-entry.
                new ObjectMapperNode
                {
                    // Maps Person to a different type.
                    Mappings =
                    {
                        ObjectMapperNode.Mapping.For<Person>(x => new { FullName = $"{x.LastName}, {x.FirstName}".ToUpper() })
                    }
                },
                // Filters log-entries and short-circuits the pipeline when False.
                new FilterNode(logEntry => true),
                new LogLevelFromPropertyNode
                {
                    Property = Names.Custom.Layer,
                    Mapper = new StringLogLevelMapper
                    {
                        //[Layer. nameof(Business)] = LogLevel.Information,
                        [nameof(ApplicationLayers.Service)] = LogLevel.Debug,
                        [nameof(ApplicationLayers.Telemetry)] = LogLevel.Information,
                        //[nameof(Presentation)] = LogLevel.Trace,
                        //[nameof(IO)] = LogLevel.Trace,
                        //[nameof(Database)] = LogLevel.Trace,
                        //[nameof(Network)] = LogLevel.Trace,
                    }
                },
                // Renames properties.
                // Sets default values for the specified keys when they are not set already. 
                new FallbackNode
                {
                    Defaults =
                    {
                        [Names.Default.Level] = LogLevel.Information,
                        [Names.Custom.Layer] = "Undefined",
                        [Names.Custom.Category] = "Undefined"
                    }
                },
                new BranchNode
                {
                    CreateNodes = () => new ILoggerNode[]
                    {
                        new CorrelationNode(),
                        new StopwatchNode(),
                        new BufferNode(),
                        new MemoryNode(),
                        new WorkItemNode(),
                    }
                },
                // Serializes every #Serializable item in the log-entry and adds it as #Property.
                new SerializerNode(),
                new PropertyMapperNode
                {
                    Mappings =
                    {
                        //{ LogEntry.Names.Scope, "Scope" },
                        { Names.Default.SnapshotName, "Identifier" },
                        { Names.Default.Correlation, "Scope" },
                        //{ LogEntry.Names.Snapshot, "Snapshot" },
                    }
                },
                new CamelCaseNode
                {
                    PropertyNames =
                    {
                        "Identifier",
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
                        new SimpleConsoleRx // Use console.
                        {
                            // Render output with this template. This is the default.
                            Template = @"[{Timestamp:HH:mm:ss:fff}] [{Level}] {Layer} | {Category} | {Identifier}: {Snapshot} {Elapsed}ms | {Message} {Exception}"
                        }
                    },
                }
            };

            var logger = loggerFactoryBuilder.Build().CreateLogger("Demo");

            logger.Information("Hallo omni-log!");

            //logger.Log(Abstraction.Layer.Service().Routine(nameof(Log)).Running());
            logger.Log(Abstraction.Layer.Service().Meta(new { Greeting = "Hallo omni-log!" }));
            logger.Log(Abstraction.Layer.Service().Meta(new { Null = (string)default }));

            // Opening outer-scope.
            using (logger.BeginScope().WithCorrelationHandle("outer").UseStopwatch())
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

                logger.Log(Application.Context.WorkItem("testFile", new { fileName = "test" }).Message("Blub!"));
                logger.Scope().WorkItem().Push(new Exception());


                // Opening inner-scope.
                using (logger.BeginScope().WithCorrelationHandle("inner").UseStopwatch())
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

    public enum Layer
    {
        Service
    }
}