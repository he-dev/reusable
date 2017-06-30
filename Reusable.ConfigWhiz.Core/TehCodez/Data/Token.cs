using System;

namespace Reusable.SmartConfig.Data
{
    public interface IToken : IEquatable<IToken>
    {
        string Value { get; }
        TokenType Type { get; }
    }

    public class Token : IToken
    {
        public Token(string value, TokenType type)
        {
            Value = value;
            Type = type;
        }

        public string Value { get; }

        public TokenType Type { get; }

        public static IToken Literal(string value) => new Token(value, TokenType.Literal);

        public static IToken Element(string value) => new Token(value, TokenType.Element);

        public bool Equals(IToken other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return StringComparer.OrdinalIgnoreCase.Equals(Value, other.Value) && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is IToken token && Equals(token);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ (int)Type;
            }
        }
    }
}