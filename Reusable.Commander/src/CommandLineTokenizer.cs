using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Reusable.Extensions;
using Reusable.Lexing;
using Reusable.Lexing.Matchers;

namespace Reusable.Commander
{
    using static CommandLineToken;

    public interface ICommandLineTokenizer
    {
        IEnumerable<Token<CommandLineToken>> Tokenize(string? text);
    }

    public enum CommandLineToken
    {
        Start = 0,

        [Regex(@"(?:\A|\s+|\\|)(?!\-)([_a-z0-9\+\.\-\:]+)"), Text]
        Value,

        [Regex(@"\s+--([a-z][a-z0-9\+\.\-]+)")]
        Argument,

        [Regex(@"\s+\-([a-z])(?![a-z])")]
        Flag,
        
        [Regex(@"(\|)")]
        Pipe,
    }

    public class CommandLineTokenizer : Tokenizer<CommandLineToken>, ICommandLineTokenizer
    {
        /*
                           
          input ------ x ------------- x ----- x ---> command-line
                \     / \             / \     /
                 value   --arg ----- /   -flag
                              \     /
                               value                                                  
        */

        public CommandLineTokenizer() : base(new StateTransitionBuilder<CommandLineToken>
        {
            { default, Value, Pipe },
            { Value, Value, Argument, Flag, Pipe },
            { Argument, Argument, Value, Flag, Pipe },
            { Flag, Flag, Argument, Pipe },
            { Pipe, Value, Argument, Flag }
        }) { }
    }
}