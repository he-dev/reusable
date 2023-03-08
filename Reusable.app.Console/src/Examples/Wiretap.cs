using System;
using Reusable.Apps;
using Reusable.Wiretap;
using Reusable.Wiretap.Channels;
using Reusable.Wiretap.Services;

namespace Reusable;

public static partial class Examples
{
    public static void Log()
    {
        var logger = 
            LoggerBuilder
                .CreateDefault()
                .Use<LogToNLog>()
                .Use<LogToConsole>()
                .Build();
        
        // Opening outer-scope.
        using (var outer = logger.Start("outer"))
        {
            outer.AttachCorrelationId("123");
            outer.Running();
            outer.Running(new { m = "m" });

            // Opening inner-scope.
            using (var inner = logger.Start("inner", new { fileName = "note.txt" }))
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
                inner.Running(new { customer });

                try
                {
                    throw new DivideByZeroException();
                }
                catch (Exception e)
                {
                    //unitOfWork2.Exception(e);
                    inner.Faulted(new { foo = "bar" }, e);
                }
            }

            //logger.Scope().Exceptions.Push(new DivideByZeroException());
            outer.Running(new { greeting = "Bye bye scopes!" });
            
        }
    }
}