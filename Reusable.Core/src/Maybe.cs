namespace Reusable {
    public readonly struct Maybe<T>
    {
        public Maybe(T value, bool hasValue, object? tag) => (Value, HasValue, Tag) = (value, hasValue, tag);

        public T Value { get; }

        public bool HasValue { get; }

        public object? Tag { get; }
        
        public static implicit operator Maybe<T>((T Value, bool HasValue) maybe) => new Maybe<T>(maybe.Value, maybe.HasValue, default);
        
        public static implicit operator Maybe<T>((T Value, bool HasValue, object? Tag) maybe) => new Maybe<T>(maybe.Value, maybe.HasValue, maybe.Tag);
    }
}