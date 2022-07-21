using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Reusable.Essentials.Data;
using Reusable.Essentials.Extensions;

namespace Reusable.Experiments.Chain2;

public static class Chain
{
    public static async Task Test()
    {
        var flow = new Flow();

        await flow.From("foo --bar").Parse().Validate().CreateCommands().Execute();
    }

    public static INode<TParameterY, TResultY> Then<TParameterX, TResultX, TParameterY, TResultY>(this INode<TParameterX, TResultX> from, INode<TParameterY, TResultY> to)
    {
        return to;
    }
}

public class Step<T>
{
    public T Value { get; set; }

    public ILifetimeScope Scope { get; set; }
}

public class Flow
{
    public Step<T> From<T>(T value) => new() { Value = value };
}

public class Parameter { }

public class Command { }

public class StartService : Node<string, string> { }

public class TelemetryService : Node<string, string> { }

public static class ParsingService
{
    public static Step<IEnumerable<Parameter>> Parse(this Step<string> commandline) => default;
}

public static class ValidationService
{
    public static Step<IEnumerable<Parameter>> Validate(this Step<IEnumerable<Parameter>> parameters) => default;
}

public static class FactoryService
{
    public static Step<IEnumerable<Command>> CreateCommands(this Step<IEnumerable<Parameter>> parameters) => default;
}

public static class CommandService
{
    public static Task Execute(this Step<IEnumerable<Command>> parameters) => default;
}