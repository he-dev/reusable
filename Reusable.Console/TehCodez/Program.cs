using System;
using System.Collections.Generic;
using Reusable.ConsoleColorizer;
using Reusable.DateTimes;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Attachements;
using Reusable.OmniLog.Collections;
using Reusable.OmniLog.SemLog;
using Reusable.OmniLog.SemLog.Attachements;

namespace Reusable.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var html = HtmlElement.Builder;

            // todo move to test
            //            var message = html
            //                .Element("p", p => p
            //                    .Append(">")
            //                    .Element("span", span => span.Append("ERROR:").Style("color: red"))
            //                    .Append(" {message} ")
            //                    .Element("span", span => span.Append("(Press Enter to exit)").Style("background-color: darkyellow"))).ToHtml(HtmlFormatting.Empty);

            Reusable.ThirdParty.NLogUtilities.LayoutRenderers.IgnoreCaseEventPropertiesLayoutRenderer.Register();

            var loggerFactory = new LoggerFactory(new[]
            {
                ConsoleTemplateRx.Create(new ConsoleTemplateRenderer()),
                NLogRx.Create()
            })
            {
                Configuration = new LoggerConfiguration
                {
                    Attachements = new HashSet<ILogAttachement>(AppSetting.FromAppConfig("omnilog:", "environment", "product"))
                    {
                        new Timestamp<UtcDateTime>(),
                        new Expected(),
                        new Actual(),
                        new AreEqual()
                    }
                }
            };

            //var consoleLogger = loggerFactory.CreateLogger("ConsoleTemplateTest");

            //consoleLogger.ConsoleMessageLine(m => m
            //    .text(">")
            //    .span(s => s.text("foo").color(ConsoleColor.Red))
            //    .text(" bar ")
            //    .span(s => s
            //        .text("foo ")
            //        .span(ss => ss.text("bar").backgroundColor(ConsoleColor.Gray))
            //        .text(" baz")
            //        .backgroundColor(ConsoleColor.DarkYellow)
            //    )
            //);

            var dbLogger = loggerFactory.CreateLogger("DbLogger");

            //for (int i = 0; i < 10000; i++)
            {
                using (var scope = dbLogger.BeginScope(s => s.TransactionId(123).WithElapsed()))
                {
                    dbLogger.State(Layer.Business, expected: () => "foo", actual: null, message: "Hallo state!");
                    dbLogger.State(Layer.Business, expected: () => "foo", actual: () => "foo", message: "Hallo state!");
                    dbLogger.State(Layer.Business, expected: () => new { FirstName = "John", LastName = "Doe" }, actual: () => "foo", message: "Hallo state!");
                    dbLogger.Event(Layer.Application, Event.ApplicationStart, Result.Success, "Hallo event!");
                }
            }


            //System.Console.ReadKey();
        }
    }

    public static class Event
    {
        public const string ApplicationStart = nameof(ApplicationStart);
    }
}