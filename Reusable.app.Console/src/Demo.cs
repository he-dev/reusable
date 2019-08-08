using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
using Reusable.OmniLog.Computables;
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

            //            var loggerFactory = new LoggerFactory
            //            {
            //                Observers =
            //                {
            //                    new ConsoleRx(),
            //                },
            //                Attachments =
            //                {
            //                    new Timestamp<DateTimeUtc>(),
            //                }
            //            };

            //var consoleLogger = loggerFactory.CreateLogger("ConsoleTemplateTest");

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
                builder.DisplayScalar(x => x.FirstName, "{0,8}");
                builder.DisplayScalar(x => x.LastName);
                builder.DisplayScalar(x => x.DBNullTest);
                builder.DisplayScalar(x => x.GraduationYears.Sum());
                builder.DisplayScalar(x => x.Age, "{0:F2}");
                builder.DisplayScalar(x => x.GraduationYears.Count);
                builder.DisplayEnumerable(x => x.GraduationYears, x => x);
                builder.DisplayEnumerable(x => x.GraduationYears, x => x, "{0:X2}");
                builder.DisplayEnumerable(x => x.Nicknames, x => x);
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
            builder.DisplayScalar(x => x.FirstName);
            builder.DisplayScalar(x => x.LastName);
            builder.DisplayScalar(x => x._testField);
            builder.DisplayScalar(x => x.DBNullTest);
            builder.DisplayScalar(x => x.Age.ToString("F2"));
            builder.DisplayScalar(x => x.Age, "{0:F2}");
            builder.DisplayScalar(x => x.GraduationYears.Count);
            builder.DisplayEnumerable(x => x.GraduationYears, x => x);
            builder.DisplayEnumerable(x => x.GraduationYears, x => x, "{0:X2}");
            builder.DisplayEnumerable(x => x.Nicknames, x => x);
        });
    }
}