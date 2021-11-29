using System.Runtime.CompilerServices;


namespace Reusable.OmniLog.Data
{
    public class CallSite
    {
        public CallSite
        (
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            CallerMemberName = callerMemberName!;
            CallerLineNumber = callerLineNumber;
            CallerFilePath = callerFilePath!;
        }

        public string CallerMemberName { get; }

        public int CallerLineNumber { get; }

        public string CallerFilePath { get; }


        // public IEnumerator<LogProperty> GetEnumerator()
        // {
        //     yield return new LogProperty(Names.Properties.CallerMemberName, CallerMemberName!, LogPropertyMeta.Builder.ProcessWith<Echo>());
        //     yield return new LogProperty(Names.Properties.CallerLineNumber, CallerLineNumber, LogPropertyMeta.Builder.ProcessWith<Echo>());
        //     yield return new LogProperty(Names.Properties.CallerFilePath, Path.GetFileName(CallerFilePath!), LogPropertyMeta.Builder.ProcessWith<Echo>());
        // }
    }
}