using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.DoubleDash.Annotations;

namespace Reusable.DoubleDash;


public class CommandLine
{
    public IEnumerable<CommandLineSwitch> Switch(IEnumerable<string> args, Action<CommandLineSwitch> configureSwitch)
    {
        var tokenizer = new CommandLineTokenizer();
        var parser = new CommandLineParser(tokenizer);
        var x = parser.Parse(string.Join(" ", args));
        
        yield return new CommandLineSwitch();
    }
}

public class CommandLineSwitch
{
    public string Command { get; set; }
    
    public IEnumerable<CommandLineArgument> Arguments { get; set; }

    public Stack<Type> Cases { get; } = new();
}

public static class CommandLineCase
{
    public static IEnumerable<CommandLineSwitch> Case<T>(this IEnumerable<CommandLineSwitch> commandLineSwitches, Action<T> execute) where T : new()
    {
        foreach (var commandLineSwitch in commandLineSwitches)
        {
            commandLineSwitch.Cases.Push(typeof(T));
            
            if (typeof(T).Name == commandLineSwitch.Command)
            {
                execute(new T());
                yield break;
            }

            yield return commandLineSwitch;
        }
    }
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
            from n in Enumerable.Empty<NameCollection>() // p.GetMultiName()
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
        var positions = parameterType.GetParameterProperties().Select(p => p.GetCustomAttribute<PositionAttribute>()).Where(p => p is { }).Select(p => p.Value);
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