using System;
using System.Collections.Generic;

namespace Reusable.Wiretap;

public static class ProcedureTraces
{
    public static Procedure LogInfo
    (
        this Procedure procedure,
        string? message = default,
        object? data = default,
        IEnumerable<string>? tags = default
    )
    {
        return Trace.GetNameByCaller(name => procedure.Log(new Trace(name, message, data, tags)));
    }

    public static Procedure LogMetric
    (
        this Procedure procedure,
        object data,
        string? message = default,
        IEnumerable<string>? tags = default
    )
    {
        return Trace.GetNameByCaller(name => procedure.Log(new Trace(name, message, data, tags), inProgress: false));
    }

    public static Procedure LogSnapshot
    (
        this Procedure procedure,
        object data,
        string? message = default,
        IEnumerable<string>? tags = default
    )
    {
        return Trace.GetNameByCaller(name => procedure.Log(new Trace(name, message, data, tags, Severity.Low)));
    }

    /// <summary>
    /// Everything went fine.
    /// </summary>
    public static Procedure LogEnd
    (
        this Procedure procedure,
        string? message = default,
        object? data = default,
        IEnumerable<string>? tags = default
    )
    {
        return Trace.GetNameByCaller(name => procedure.Log(new Trace(name, message, data, tags), inProgress: false));
    }

    /// <summary>
    /// Something went wrong.
    /// </summary>
    public static Procedure LogError
    (
        this Procedure procedure,
        string? message = default,
        object? data = default,
        IEnumerable<string>? tags = default,
        Exception? exception = default
    )
    {
        return Trace.GetNameByCaller(name => procedure.Log(new Trace(name, message, data, tags, Severity.High), exception, inProgress: false));
    }
}