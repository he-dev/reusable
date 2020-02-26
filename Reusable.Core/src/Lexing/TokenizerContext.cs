namespace Reusable.Lexing
{
    public class TokenizerContext<TToken>
    {
        public string Value { get; set; }

        public int Position { get; private set; }

        public string Processed => Value.Substring(0, Position);

        public string Left => Value.Substring(Position, Value.Length - Position);

        public TToken TokenType { get; set; }

        public bool Eof => Position >= Value.Length - 1;

        public char Current => Value[Position];

        public bool MoveNext() => !Eof && Position++ < Value.Length;
        
        public void MoveBy(int length) => Position += length;

        public void Backtrack(int length) => Position -= length;

        //public override string ToString() => $"{nameof(Position)}={Position}, {nameof(TokenType)}={TokenType}";
    }
}