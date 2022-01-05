using JetBrains.Annotations;

namespace Reusable.ReMember
{
    [PublicAPI]
    public abstract class SelectorToken //: IEquatable<SelectorToken>
    {
        protected SelectorToken(string name) => Name = name;

        public string Name { get; }

        public override string ToString() => Name;

        //public override int GetHashCode() => AutoEquality<SelectorToken>.Comparer.GetHashCode(this);

        //public override bool Equals(object obj) => obj is SelectorToken st && Equals(st);

        //public bool Equals(SelectorToken obj) => AutoEquality<SelectorToken>.Comparer.Equals(this, obj);

        public static implicit operator string(SelectorToken token) => token.ToString();
    }

    namespace Tokens
    {
        public class SchemeToken : SelectorToken
        {
            public SchemeToken(string name) : base(name) { }
        }

        public class NamespaceToken : SelectorToken
        {
            public NamespaceToken(string name) : base(name) { }
        }

        public class TypeToken : SelectorToken
        {
            public TypeToken(string name) : base(name) { }
        }

        public class MemberToken : SelectorToken
        {
            public MemberToken(string name) : base(name) { }
        }

        public class IndexToken : SelectorToken
        {
            public IndexToken(string name) : base(name) { }
        }
    }
}