using System;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Policy;
using System.Text;
using Reusable.Experiments;
using Reusable.Fluorite;
using Reusable.Fluorite.Html;

namespace Reusable
{
    using static CommandLineToken;
    
    public static partial class Examples
    {
        private const bool DoNotDisplayKey = true;
        
        public static void Tokenize()
        {
            var text = new StringBuilder();
            var tokenizer = new CommandLineTokenizer();
            
            Console.WriteLine("Go!");
                
            while (true)
            {
                var input = Console.ReadKey(DoNotDisplayKey);

                switch (input.Key)
                {
                    case ConsoleKey.Backspace:
                        text.Length--;
                        //Console.Clear();
                        //Console.Write(text.ToString());
                        
                        break;
                    
                    default:
                        text.Append(input.KeyChar);
                        Console.Write(input.KeyChar);
                        break;
                }

                var tokens = tokenizer.Tokenize(text.ToString()).ToList();
                var html = HtmlElement.Create("p");
//                foreach (var token in tokens)
//                {
//                    switch (token.Type)
//                    {
//                        case Command:
//                            html.Append(HtmlElement.Builder.span(s => s.text(token.Text)))
//                    }
//                }
            }
        }
    }
    
    [EnumTokenInfo]
    public enum CommandLineToken
    {
        Start = 0,

        [Regex(@"\s*(\?|[a-z0-9][a-z0-9\-_]*)")]
        Command,

        //[Context("Argument")]
        [Regex(@"(\s*[\-\.\/])")]
        ArgumentSeparator,

        [Context("Argument")]
        [Regex(@"([a-z0-9][a-z0-9\-_]*)")]
        Argument,

        //[Context("Value")]
        [Regex(@"([\=\:]|\,?\s*)")]
        ValueSeparator,

        [Context("Value")]
        //[QText(@"([a-z0-9][a-z0-9\-_]*)")]
        [Regex(@"([a-z0-9][a-z0-9\-_]*|"".*(?<!\\)"")")]
        Value,
    }

    public class CommandLineTokenizer : Tokenizer<CommandLineToken>
    {
        /*

         command [-argument][=value][,value]
         
         command --------------------------- CommandLine
                \                           /
                 -argument ------   ------ /    
                          \      / \      /
                           =value   ,value
                    
        */
        private static readonly State<CommandLineToken>[] States =
        {
            new State<CommandLineToken>(default, Command),
            new State<CommandLineToken>(Command, ArgumentSeparator),
            new State<CommandLineToken>(ArgumentSeparator, Argument),
            new State<CommandLineToken>(Argument, ArgumentSeparator, ValueSeparator),
            new State<CommandLineToken>(ValueSeparator, Value),
            new State<CommandLineToken>(Value, ArgumentSeparator, ValueSeparator),
        };

        public CommandLineTokenizer() : base(States.ToImmutableList()) { }
    }
}