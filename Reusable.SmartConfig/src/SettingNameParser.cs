using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Reflection;

namespace Reusable.SmartConfig
{
    using Token = SettingNameToken;
        
    public enum SettingNameToken
    {
        Prefix,
        Namespace,
        Type,
        Member,
        Instance
    }
    
    internal static class SettingNameParser
    {
        public static class Separator
        {
            public const char Prefix = ':';
            public const char Namespace = '+';
            public const char Type = '.';
            public const char Member = ',';
        }

        public static IDictionary<Token, ReadOnlyMemory<char>> Tokenize([NotNull] string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var name = new ReadOnlyMemory<char>(text.ToArray());
            var tokens = new Dictionary<Token, ReadOnlyMemory<char>>();
            var (min, max) = (0, 0);

            foreach (var c in name.Span)
            {
                switch (c)
                {
                    case Separator.Prefix:
                        tokens.Clear();
                        min = 0;
                        Add(Token.Prefix, name.Slice(min, max - min));
                        min = max;
                        break;

                    case Separator.Namespace:
                        min = tokens.TryGetValue(Token.Prefix, out var assembly) ? assembly.Length + 1 : 0;
                        tokens.Remove(Token.Type);
                        Add(Token.Namespace, name.Slice(min, max - min));
                        min = max;
                        break;

                    case Separator.Type:
                        if (tokens.Any())
                        {
                            Add(Token.Type, name.Slice(min + 1, max - min - 1));
                        }
                        else
                        {
                            Add(Token.Type, name.Slice(min, max - min));
                        }

                        //Add(Token.Type, name.Slice(min + 1, max - min - 1));
                        min = max;
                        break;

                    case Separator.Member:
                        Add(Token.Member, name.Slice(min + 1, max - min - 1));
                        min = max;
                        break;
                }

                max++;
            }

            if (min < max)
            {
                var token = tokens.ContainsKey(Token.Member) ? Token.Instance : Token.Member;
                Add(
                    token,
                    tokens.Any()
                        ? name.Slice(min + 1, max - min - 1)
                        : name.Slice(min, max - min)
                );
            }

            if (!tokens.ContainsKey(Token.Member))
            {
                throw ("MemberMissing", $"Setting name must at least contains member.").ToDynamicException();
            }

            if (tokens.ContainsKey(Token.Namespace) && !tokens.ContainsKey(Token.Type))
            {
                throw ("TypeMissing", $"Namespace must not exist without type.").ToDynamicException();
            }

            return tokens;

            void Add(Token token, ReadOnlyMemory<char> value)
            {
                if (tokens.ContainsKey(token)) throw ("SettingNameFormat", $"Unexpected token '{token}' at {max}.").ToDynamicException();
                tokens.Add(token, value);
            }
        }
    }
}