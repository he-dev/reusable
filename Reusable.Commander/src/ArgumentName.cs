using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Extensions;


namespace Reusable.Commander
{
    public class ArgumentName : IEnumerable<string>, IEquatable<ArgumentName>
    {
        private readonly ISet<string> _names;

        public ArgumentName(string primary, IEnumerable<string>? secondary = default) => _names = new SortedSet<string>(new[] { primary }.Concat(secondary));

        public string Primary => _names.First();

        public IEnumerable<string> Secondary => _names.Skip(1);
        
        public static ArgumentName Command => new ArgumentName(nameof(Command));
        
        public static ArgumentName Create(string primary, params string[] secondary) => new ArgumentName(primary, secondary);
        
        public override int GetHashCode() => 0;
        
        public override bool Equals(object? obj) => Equals(obj as ArgumentName);
        
        public bool Equals(ArgumentName? other) => _names.Overlaps(other ?? Enumerable.Empty<string>());

        public IEnumerator<string> GetEnumerator() => _names.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_names).GetEnumerator();
    }
}