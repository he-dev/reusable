# Reusable.Converters

This project provides type converters for dozen of types.

## Examples

Compose a converter that can convert `string` to `int` or `DateTime`:

```cs
var converter =
    TypeConverter.Empty
        .Add<StringToInt32Converter>()
        .Add<StringToDateTimeConverter>();

var result = converter.Convert("123", typeof(int));
```

By default all converters use the `InvariantCulture`. Additional arguments can be specified if there is a need for custom formats.

## Extending

Converters can perform ony one coversion. This means there is usualy a pair of each converter e.g. `StringToInt32Converter` and `Int32ToStringConverter`.

To create a new converter derive it from `TypeConverter<TValue, TResult>`. See: [StringToBooleanConverter](https://github.com/he-dev/Reusable/blob/master/Reusable.Converters/TehCodez/_Converters.Specific/BooleanConverters.cs)


