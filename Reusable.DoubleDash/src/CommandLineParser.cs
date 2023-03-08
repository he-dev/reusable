﻿using System.Collections.Generic;
using System.Linq;

namespace Reusable.DoubleDash;

using static CommandLineToken;

public interface ICommandLineParser
{
    IEnumerable<List<CommandLineArgument>> Parse(string commandLine);
}

public class CommandLineParser : ICommandLineParser
{
    private readonly ICommandLineTokenizer _tokenizer;

    public CommandLineParser(ICommandLineTokenizer tokenizer)
    {
        _tokenizer = tokenizer;
    }

    public IEnumerable<List<CommandLineArgument>> Parse(string commandLine)
    {
        // The first parameter is always a command.
        var arguments = new Stack<CommandLineArgument>();

        foreach (var token in _tokenizer.Tokenize(commandLine))
        {
            switch (token.Type)
            {
                case Value:
                    arguments.Peek().Values.Add(token.Value);
                    break;

                case Argument:
                    arguments.Add(NameCollection.Create(token.Value));
                    break;

                case Flag:
                    foreach (var flag in token.Value) arguments.Add(NameCollection.Create(flag.ToString()));
                    break;

                case Params:
                    arguments.Add(NameCollection.Params);
                    break;

                case Pipe when arguments.Any():
                    yield return arguments;
                    arguments = new List<CommandLineArgument> { NameCollection.Command };
                    break;
            }
        }

        // The second part handles an empty command-line where there is not even a command name.
        if (arguments.Any() && arguments.First().Any())
        {
            yield return arguments;
        }
    }
}