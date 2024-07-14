using System;
using Reusable.Wiretap;
using Reusable.Wiretap.Loggers;

namespace Reusable;

public class Examples
{
    public static void LogExample()
    {
        NLogJsonAdapter.Register();
        var telemetry = new Telemetry<Examples>(new Wiretap.Loggers.NLog())
        {
            Properties =
            {
                new EnvironmentProperty.Provider { "OneDrive" }
            }
        };

        LogNestedProcedures(telemetry);

        return;
        // Opening outer-scope.
        using (var outer = telemetry.LogBegin(tags: new[] { "foo-tag" }))
        {
            outer.LogInfo("this_is_an_info");
            outer.LogSnapshot(new { m = "m" });

            // Opening inner-scope.
            using (var inner = telemetry.LogBegin("inner", data: new { fileName = "note.txt" }))
            {
                // Logging an entire object in a single line.
                // var customer = new Person
                // {
                //     FirstName = "John",
                //     LastName = "Doe",
                //     Age = 123.456,
                //     DBNullTest = DBNull.Value,
                //     GraduationYears = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                //     Nicknames = { "Johny", "Doe" }
                // };
                //inner.Running(new { customer });

                try
                {
                    throw new DivideByZeroException();
                }
                catch (Exception e)
                {
                    //unitOfWork2.Exception(e);
                    inner.LogError(data: new { foo = "bar" }, exception: e);
                }
            }

            //logger.Scope().Exceptions.Push(new DivideByZeroException());
            outer.LogInfo(tags: new[] { "Bye bye scopes!" });
        }
    }

    public static void LogNestedProcedures(Telemetry telemetry)
    {
        using var p1 = telemetry.LogBegin(data: new { foo = "foo", bar = "bar" }, tags: new[] { "foo" });
        using var p2 = telemetry.LogBegin(data: new { bar = "baz", baz = "baz" }, tags: new[] { "bar" });
    }
}