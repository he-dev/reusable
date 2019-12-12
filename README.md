# Reusable

The `reusable` repository is a collection of utilities that simplify many tedious tasks. Here are the highlights.

## Resuable.Core

- `SoftString` - allows you to completely avoid mistakes concerning string comparison. It trims whitespace charactes from the input value and uses case-insensitive comparer internally. Useful for all kinds of names and keys where leading or trailing whitespace as well as case are irrelevant.

- `DynamicException` - allows you to generate exceptions at runtime instead of creating countless classes for exceptions that are never cought.

- `Maybe<T>` - a popular monad that can be used instead of the `TryDoSomething` pattern.

- `Range<T>` - a simple range class.

## Reusable.Flexo

This project allows you to create easily configurable decision trees with `json`. It works similar to `linq` and provides many basic expressions.

In this example you see a collection with three elements that is being iterated by `Contains`. It uses a custom `Prodicate` that checks whether an item `IsEqual` the specified `Value` and `SoftString` comparer that does the actual comparison. 

You've probably noticed the _unusual_ syntax for specifying types like `"$f": "Contains"` instead of `"$type": "Assemply, Type"` etc. See `Reusable.Utilities.JsonNet` for more details.

```json
{
    "$f": "Collection",
    "Values": [
        "foo",
        "bar",
        "baz"
    ],
    "This": {
        "$f": "Contains",
        "Predicate": {
            "$f": "IsEqual",
            "Value": "BAR",
            "Comparer": "SoftString"
        }
    }
}
```

### Reusable.Utilities.JsonNet

- `PrettyJsonSerializer` - allows you to use convenient and short type names like `"$f": "Contains"` instead of full names like `"$type": "Assemply, Type"`.

### extensions

- `Type.ToPrettyString()` - generates a nice looking type strings for generic types, e.g. `List<int>` instead of `List´1`.

