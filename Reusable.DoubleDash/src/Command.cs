using System;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.DoubleDash.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.DoubleDash;

public interface ICommand
{
    ArgumentName Name { get; }

    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    /// <remarks>This property is required for when a command is decorated and there is no access to the parameter type via generic arguments.</remarks>
    Type ParameterType { get; }

    Task ExecuteAsync(object? parameter, CancellationToken cancellationToken = default);
}

[PublicAPI]
public abstract class Command<TParameter> : ICommand where TParameter : class, new()
{
    protected Command(ArgumentName? name = default)
    {
        Name = name ?? GetType().GetArgumentName();
    }

    public virtual ArgumentName Name { get; }

    public Type ParameterType => typeof(TParameter);

    public virtual async Task ExecuteAsync(object? parameter, CancellationToken cancellationToken)
    {
        await ExecuteAsync
        (
            parameter as TParameter ?? throw new ArgumentOutOfRangeException(paramName: nameof(parameter), message: $"{Name} command parameter must be of type {typeof(TParameter).ToPrettyString()}."),
            cancellationToken
        );
    }

    protected abstract Task ExecuteAsync(TParameter parameter, CancellationToken cancellationToken);
}

public static class Command
{
    private static void ValidateParameter(Type parameterType)
    {
        try
        {
            ValidateParameterPropertyNames(parameterType);
            ValidateParameterPropertyPositions(parameterType);
        }
        catch (Exception inner)
        {
            throw DynamicException.Create("ParameterValidation", $"Parameter {parameterType.GetArgumentName().Join(", ").EndsWith("[]")} is invalid. See the inner exception for details.", inner);
        }
    }

    private static void ValidateParameterPropertyNames(Type parameterType)
    {
        var query =
            from p in parameterType.GetParameterProperties()
            from n in Enumerable.Empty<ArgumentName>() // p.GetMultiName()
            group n by n into g
            where g.Count() > 1
            select g.Key;

        if (query.ToList() is var duplicates && duplicates.Any())
        {
            throw DynamicException.Create($"AmbiguousArgumentName", $"Some argument names are used multiple times: {duplicates.Join(", ").EncloseWith("[]")}");
        }
    }

    private static void ValidateParameterPropertyPositions(Type parameterType)
    {
        var positions = parameterType.GetParameterProperties().Select(p => p.GetCustomAttribute<PositionAttribute>()).Where(p => p is {}).Select(p => p.Value);
        var positionCount = parameterType.GetParameterProperties().Count(p => p.IsDefined(typeof(PositionAttribute)));
        positionCount = positionCount.IsEven() ? positionCount : positionCount + 1;
        var pairCount = positionCount / 2;
        var expectedSum = (1 + positionCount) * pairCount;
        var actualSum = positions.Sum();
        if (actualSum != expectedSum)
        {
            throw DynamicException.Create($"NonContinuousArgumentPosition", "Argument positions must start with 1 and be continuous.");
        }
    }
}