using System.Runtime.CompilerServices;

namespace Reusable.Marbles.Reflection;

public class CallerInfo
{        
    public static (string CallerMemberName, int CallerLineNumber, string CallerFilePath) Create
    (
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        return (callerMemberName, callerLineNumber, callerFilePath);
    }
}