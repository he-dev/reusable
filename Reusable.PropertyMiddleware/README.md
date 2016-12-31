# PropertyMiddleware

The `PropertyMiddleware` is a utility for getting or setting properties by name. 

Sometimes it's necessary to get or set a value with reflection. This project uses compiled and cached expressions so there is only a minimal overhead.

It can work with up to two dimensional indexers.

## Examples

### Getting values

You can get property values by first creating an instance of the `PropertyReader<T>` 

```cs
var reader = new PropertyReader<Foo>();
```

and then calling the appropriate `GetValue` method.

```cs
// Normal property
var result1 = reader.GetValue<string>(foo, nameof(Foo.Bar);

// Indexer
var result2 = reader.GetValue<int, string>(foo, null, 1);
```

When using an indexers you can ommit the property name, here `null`, it will be ignored anyway.

```cs
var reader = new PropertyReader<Foo>();

```

### Setting values

You can set property values by first creating an instance of the `PropertyWriter<T>` 

```cs
var writer = new PropertyWriter<Foo>();
```
and calling the appropriate `SetValue` method

```cs
// Normal property
writer.SetValue<string>(foo, nameof(Foo.Bar), "new-value");

// Indexer
writer.SetValue<int, string>(foo, null, "new-value", 1);
```
