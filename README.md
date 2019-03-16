# Flexo

## Expressions

- `Switch` - evaluates multiple cases and invokes the `Body` of the first match.
  - `Value` - an expression returning a `Constant<object>`
  - `Cases` - a collection of `SwitchCase` where
    - `When` - if `Constant` then `ObjectEqual` is used otherwise it must be a `Predicate`.
    - `Body` - any expression
  - `Default` - any expression that should be invoked when there was no match.
  - Context - `Value`

- `Contains`
  - `Comparer` - if `null` then `ObjectEqual` is used otherwise it must be a `Predicate`.
  - Context - `Value`, `Item`

- `Matches`
  - `Value` - an expression returning a `Constant<string>`
  - `Pattern` - `string` or an expression returning a `Constant<string>`