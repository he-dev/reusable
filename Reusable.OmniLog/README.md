
# OmniLog

This is my own logging abstraction based on top of the [`ReactiveX`](http://reactivex.io) framework. I use it in all my tools because it gives a lot more options and flexibility then any other available loggers. 

Unlike any of the popular frameworks that allow you to only specify the minimul log level, `OmniLog.LogLevel` are flags that can also be combined to selectively filter whats get logged.

---

The first example is taken from the [`SemLog`](https://github.com/he-dev/Reusable/tree/master/Reusable.OmniLog.SemLog).

`LoggerFactory` is a dependency-injection friendly class whose main pupose it to create new loggers. Each logger supports any number of attachemes that are added automatically to each log. In this example the `Environment` is used that is set via the `app.config`, a runtime static property `Product` and a `Snapshot` renderer.

```cs
var loggerFactory = new LoggerFactory(new[]
{
    NLogRx.Create(Enumerable.Empty<ILogScopeMerge>())
})
{
    Configuration = new LoggerConfiguration
    {
        Attachements = new HashSet<ILogAttachement>
        {
            new AppSetting(nameof(Environment), nameof(Environment)),
            new Lambda(nameof(Product), log => Product),
            new Timestamp<UtcDateTime>(),
            new Reusable.OmniLog.SemLog.Attachements.Snapshot
            {
                Settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Converters =
                    {
                        new SoftStringConverter(),
                        new LogLevelConverter(),
                        new StringEnumConverter()
                    }
                }
            }
        }
    }
};
```


---

Another example is taken from the [`Reusable.ConsoleColorizer`](https://github.com/he-dev/Reusable/tree/master/Reusable.ConsoleColorizer). This project uses a few extensions that make writing colored outuput to the console very easy. 

This one uses only one attachement, which is the timestamp. It then creates a new logger that renders its output via the `ConsoleTemplateRx` _(Rx means Receiver)_.

```cs
Reusable.ThirdParty.NLogUtilities.LayoutRenderers.IgnoreCaseEventPropertiesLayoutRenderer.Register();

var loggerFactory = new LoggerFactory(new[]
{
    ConsoleTemplateRx.Create(new ConsoleTemplateRenderer()),
})
{
    Configuration = new LoggerConfiguration
    {
        Attachements = new HashSet<ILogAttachement>
        {
            new Timestamp<UtcDateTime>(),
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
```