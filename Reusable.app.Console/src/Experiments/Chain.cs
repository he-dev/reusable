using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reusable.Experiments;

public static class Chain
{
    public static void Test()
    {
        var flow =
            Flow.From(new StartService(), x => x
                .Then(new TelemetryService())
                .Then(new ParsingService())
                .Then(new ValidationService())
                .Then(new FactoryService())
                .Then(new CommandService())).Build();
    }

    public static INode<TParameterY, TResultY> Then<TParameterX, TResultX, TParameterY, TResultY>(this INode<TParameterX, TResultX> from, INode<TParameterY, TResultY> to)
    {
        return to;
    }
}

public interface IFrom<T> { }

public interface INext<T> { }

public interface INode<TParameter, TResult> : IFrom<TParameter>, INext<TResult>
{
    public INext<TResult> Next { get; set; }

    public Task<TResult> InvokeAsync(TParameter parameter);
}

public abstract class Node<TParameter, TResult> : INode<TParameter, TResult>
{
    public INext<TResult> Next { get; set; }

    public virtual Task<TResult> InvokeAsync(TParameter parameter)
    {
        throw new System.NotImplementedException();
    }
}

public static class Flow
{
    public static FlowBuilder<TParameterX> From<TParameterX, TResultX>(INode<TParameterX, TResultX> first, Action<INext<TResultX>> configure)
    {
        return new FlowBuilder<TParameterX>(first);
    }

    public static INext<TResultY> Then<TResultX, TResultY>(this INext<TResultX> from, INext<TResultY> to)
    {
        return to;
    }
}

public class FlowBuilder<TParameter>
{
    private IFrom<TParameter> From { get; }

    public FlowBuilder(IFrom<TParameter> from)
    {
        From = from;
    }

    public IFrom<TParameter> Build() => From;
}

public class Parameter { }

public class Command { }

public class StartService : Node<string, string> { }

public class TelemetryService : Node<string, string> { }

public class ParsingService : Node<string, Parameter> { }

public class ValidationService : Node<Parameter, Parameter> { }

public class FactoryService : Node<Parameter, Command> { }

public class CommandService : Node<Command, object> { }