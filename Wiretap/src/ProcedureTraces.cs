using System;
using System.Collections.Generic;

namespace Reusable.Wiretap;

public static class ProcedureTraces
{
    public static IProcedure LogInfo
    (
        this IProcedure procedure,
        string? message = default,
        object? data = default,
        IEnumerable<object>? tags = default
    )
    {
        return Trace.GetNameByCaller(name => procedure.Log(new Trace(name, message, data, tags)));
    }

    public static IProcedure LogMetric
    (
        this IProcedure procedure,
        object data,
        string? message = default,
        IEnumerable<object>? tags = default
    )
    {
        return Trace.GetNameByCaller(name => procedure.Log(new Trace(name, message, data, tags), inProgress: false));
    }

    public static IProcedure LogSnapshot
    (
        this IProcedure procedure,
        object data,
        string? message = default,
        IEnumerable<object>? tags = default
    )
    {
        return Trace.GetNameByCaller(name => procedure.Log(new Trace(name, message, data, tags, Severity.Low)));
    }

    /// <summary>
    /// Everything went fine.
    /// </summary>
    public static IProcedure LogEnd
    (
        this IProcedure procedure,
        string? message = default,
        object? data = default,
        IEnumerable<object>? tags = default
    )
    {
        return Trace.GetNameByCaller(name => procedure.Log(new Trace(name, message, data, tags), inProgress: false));
    }

    /// <summary>
    /// Something went wrong.
    /// </summary>
    public static IProcedure LogError
    (
        this IProcedure procedure,
        string? message = default,
        object? data = default,
        IEnumerable<object>? tags = default,
        Exception? exception = default
    )
    {
        return Trace.GetNameByCaller(name => procedure.Log(new Trace(name, message, data, tags, Severity.High), exception, inProgress: false));
    }
}