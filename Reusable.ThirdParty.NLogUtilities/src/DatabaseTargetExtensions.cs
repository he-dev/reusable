using NLog.Layouts;
using NLog.Targets;

namespace Reusable.Utilities.ThirdParty.NLog
{
    public static class DatabaseTargetExtensions
    {
        public static string CommandText(this DatabaseTarget databaseTarget)
        {
            return ((SimpleLayout)databaseTarget.CommandText).OriginalText;
        }
    }
}
