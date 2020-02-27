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
    
    public static class CommandLineModes
    {
        public const int Params = 1 << 2;
    }

    public enum CommandLineToken
    {
        Start = 0,

        // [Regex(@"(?:\A|\s+|\\|)(?!\-)([_a-z0-9\+\.\-\:\*\/\!]+)"), Text]
        [Text(Mode = TokenizerModes.Default | CommandLineModes.Params)]
        [Regex(@"(?:\A|\s+|\\|)(?!\-)([^\s\|]+)")]
        [Regex(@"(?:\s+)([^\s]+)", Mode = CommandLineModes.Params)]
        Value,

        [Regex(@"\s+--([a-z][a-z0-9\+\.\-]+)")]
        Argument,

        //[Regex(@"\s+\-([a-z])(?![a-z])")]
        [Regex(@"\s+\-([a-z]+)")]
        Flag,

        [Regex(@"\s+(--)(?=\s)", SetMode = CommandLineModes.Params)]
        Params,

        [Regex(@"(\|)")]
        Pipe,
    }

    public class CommandLineTokenizer : Tokenizer<CommandLineToken>, ICommandLineTokenizer
    {
        /*
                           
          input ------ x ------------- x ----- x -- x ----> command-line
                \     / \             / \     / \  /
                 value   --arg ----- /   -flag   --
                              \     /
                               value                                                  
        */

        public CommandLineTokenizer() : base(new StateTransitionBuilder<CommandLineToken>
        {
            { default, Value, Pipe },
            { Value, Value, Argument, Flag, Params, Pipe },
            { Argument, Argument, Value, Flag, Params, Pipe },
            { Flag, Flag, Argument, Params, Pipe },
            { Params, Value },
            { Pipe, Value, Argument, Flag }
        }) { }
    }
}