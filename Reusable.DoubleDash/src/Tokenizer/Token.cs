using System;
using System.Diagnostics;

namespace Reusable.Marbles.Lexing;

public class Token<TToken> where TToken : Enum
{
    public Token(string value, int position, int length, TToken type)
    {
        (Value, Position, Length, Type) = (value, position, length, type);
    }
    public string Value { get; }

    public int Position { get; }

    public int Length { get; }

    public TToken Type { get; }
}