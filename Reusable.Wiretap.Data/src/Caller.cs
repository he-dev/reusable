using System.Runtime.CompilerServices;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data
{
    public class Caller : ICaller
    {
        public Caller
        (
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            MemberName = callerMemberName!;
            LineNumber = callerLineNumber;
            FilePath = callerFilePath!;
        }

        public string MemberName { get; }

        public int LineNumber { get; }

        public string FilePath { get; }


        // public IEnumerator<LogProperty> GetEnumerator()
        // {
        //     yield return new LogProperty(Names.Properties.CallerMemberName, CallerMemberName!, LogPropertyMeta.Builder.ProcessWith<Echo>());
        //     yield return new LogProperty(Names.Properties.CallerLineNumber, CallerLineNumber, LogPropertyMeta.Builder.ProcessWith<Echo>());
        //     yield return new LogProperty(Names.Properties.CallerFilePath, Path.GetFileName(CallerFilePath!), LogPropertyMeta.Builder.ProcessWith<Echo>());
        // }
    }
}