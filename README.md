# reusable

The `reusable` repository is a collection of utilities that simplify many tedious tasks. Here are the highlights.

## resuable.core

- `SoftString` - allows you to completely avoid mistakes concerning string comparison. It trims whitespace charactes from the input value and uses case-insensitive comparer internally. Useful for all kinds of names and keys where leading or trailing whitespace as well as case are irrelevant.

- `DynamicException` - allows you to generate exceptions at runtime instead of creating countless classes for exceptions that are never cought.

- 
