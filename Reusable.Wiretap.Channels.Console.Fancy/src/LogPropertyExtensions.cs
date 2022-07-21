using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Channels;

public static class LogPropertyExtensions
{
    public static string Template(this ILogPropertyName? _) => nameof(Template);
}