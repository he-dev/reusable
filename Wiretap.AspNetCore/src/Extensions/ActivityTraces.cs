namespace Reusable.Wiretap.AspNetCore.Extensions;

public static class ActivityTraces
{
    public static IProcedure LogRequest(this IProcedure procedure, string? message = default, object? details = default, object? attachment = default)
    {
        return procedure.LogTraceByCaller(message, details, attachment, false);
    }

    public static IProcedure LogResponse(this IProcedure procedure, string? message = default, object? details = default, object? attachment = default)
    {
        return procedure.LogTraceByCaller(message, details, attachment, false);
    }
}