using System;
using System.Diagnostics;
using Reusable.Essentials.Diagnostics;

namespace Reusable.Essentials.Lexing;

[DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
public class Token<TToken> where TToken : Enum
{
    public Token(string value, int position, int length, TToken type)
    {
        (Value, Position, Length, Type) = (value, position, length, type);
    }

    private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
    {
        builder.DisplaySingle(x => x.Value);
        builder.DisplaySingle(x => x.Position);
        builder.DisplaySingle(x => x.Length);
        builder.DisplaySingle(x => x.Type);
    });

    public string Value { get; }

    public int Position { get; }

    public int Length { get; }

    public TToken Type { get; }

    //[DebuggerStepThrough]
    //public override string ToString() => $"{base.ToString()} {Type.ToString()}";
}