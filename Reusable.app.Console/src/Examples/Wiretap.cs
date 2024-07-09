using System;
using System.Linq;
using Reusable.Wiretap;
using Reusable.Wiretap.Modules.Loggers;
using NLog;

namespace Reusable;

public static partial class Examples
{
    public static void LogExample()
    {
        NativeJsonSerializer.Register();
        var logger = new Wiretap.Logger(new[] { new NLogAdapter() });

        // Opening outer-scope.
        using (var outer = logger.LogBegin(tags: new[] { "foo-tag" }))
        {
            outer.LogInfo("this_is_an_info");
            outer.LogSnapshot(new { m = "m" });

            // Opening inner-scope.
            using (var inner = logger.LogBegin("inner", data: new { fileName = "note.txt" }))
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
}