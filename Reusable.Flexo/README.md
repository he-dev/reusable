# Flexo

`Flexo` is a framework for evaluating configurable expressions. You specify them via a special `*.json` file. See [this](https://github.com/he-dev/reusable/blob/dev/Reusable.Tests.XUnit/res/Flexo/Array-of-expressions.json) for examples.

Expressions are constructed as trees. In addition to this they support _extensions_ via `This` property (which is an array) where you can specify any number of extension expressions in a functional way to process the result of the previous expression:

```json
  {
    "$t": "Flexo.Collection",
    "Values": [
      1.0,
      2.0
    ],
    "This": [
      {
        "$t": "Flexo.Sum"
      }
    ]
  }
```

All `Flexo` expressions must be prefixed with the `Flexo` namespace. Use the `[NamespaceAttribute]` to introduce your own prefix.

In order to read a `Flexo` `*.json` use the provided `ExpressionSerializer`. See [here](https://github.com/he-dev/reusable/blob/dev/Reusable.Tests.XUnit/src/Flexo/ExpressionSerializerTest.cs) for examples.

## Expressions

- `Switch` - evaluates multiple cases and invokes the `Body` of the first match.
  - `Value` - an expression returning a `Constant<object>`
  - `Cases` - a collection of `SwitchCase`s where
    - `When` - if `Constant` then `ObjectEqual` is used otherwise it must be a `Predicate`.
    - `Body` - any expression
  - `Default` - any expression that should be invoked when there was no match.
  - Context - `Switch.Value`

- `Contains`
  - `Value` - an `object` or an `Expression`
  - `Comparer` - if `null` then `ObjectEqual` is used otherwise it must be a `Predicate`.
  - Context - `Contains.Value`, `Contains.Item`
  - Extension: `Yes`

- `Matches`
  - `IgnoreCase` - indicates whether `Regex` should ignore case. Default is `true`.
  - `Value` - an `string` or a `Constant<string>`
  - `Pattern` - `string` or an expression returning a `Constant<string>`
  - Extension: `Yes`

- `SwitchToDouble` - maps `1.0` to `true` and `0.0` to `false`.
  - Extension: `Yes`

- `GetContextItem` - gets an item from the expression-context.
  - `Key` - the key of the item to get. _Use `ExpressionContext.CreateKey` to create a key when testing._

- `Not` - negates a predicate
  - `Value` - an `Expression`
  - Extension: `Yes`

- `Sum` - calculates the sum of its values
  - `Values` - a list of `Expression`s
  - Extension: `Yes`