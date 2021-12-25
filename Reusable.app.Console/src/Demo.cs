using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials.Diagnostics;
using Reusable.Snowball;
using Reusable.Snowball.Converters;
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
                TypeConverterStack
                    .Empty
                    .Push<StringToDateTime>()
                    .Push<StringToInt32>();

            var result = converter.ConvertOrDefault("123", typeof(int));
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
                builder.DisplaySingle(x => x.FirstName, "{0,8}");
                builder.DisplaySingle(x => x.LastName);
                builder.DisplaySingle(x => x.DBNullTest);
                builder.DisplaySingle(x => x.GraduationYears.Sum());
                builder.DisplaySingle(x => x.Age, "{0:F2}");
                builder.DisplaySingle(x => x.GraduationYears.Count);
                builder.DisplayEnumerable(x => x.GraduationYears, x => x);
                builder.DisplayEnumerable(x => x.GraduationYears, x => x, "{0:X2}");
                builder.DisplayEnumerable(x => x.Nicknames, x => x);
            });
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
            builder.DisplaySingle(x => x.FirstName);
            builder.DisplaySingle(x => x.LastName);
            builder.DisplaySingle(x => x._testField);
            builder.DisplaySingle(x => x.DBNullTest);
            builder.DisplaySingle(x => x.Age.ToString("F2"));
            builder.DisplaySingle(x => x.Age, "{0:F2}");
            builder.DisplaySingle(x => x.GraduationYears.Count);
            builder.DisplayEnumerable(x => x.GraduationYears, x => x);
            builder.DisplayEnumerable(x => x.GraduationYears, x => x, "{0:X2}");
            builder.DisplayEnumerable(x => x.Nicknames, x => x);
        });
    }
}