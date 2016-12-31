# PropertyMiddleware

The `PropertyMiddleware` is a utility for getting or setting properties by name. 

Sometimes it's necessary to get or set a value with reflection. This project uses compiled and cached expressions so there is only a minimal overhead.

It can work with up to two dimensional indexers.

## Examples

### Getting values

You can get property values by creating an instance of the `PropertyReader<T>` and calling the `GetValue` method.

```cs
var reader = new PropertyReader<Foo>();
var bar = reader.GetValue<string>(foo, nameof(Foo.Bar);
```

Using indexers is equaly simple. After the property name (which is ignored, thus `null`) specify the index:

```cs
var reader = new PropertyReader<Foo>();
var value = reader.GetValue<int, string>(foo, null, 1);
```

### Setting values

You can get property values by creating an instance of the `PropertyWriter<T>` and calling the `SetValue` method.

```cs
var writer = new PropertyWriter<Foo>();
writer.SetValue<string>(foo, nameof(Foo.Bar), "new-value");
```

Using indexers is equaly simple. After the property name (which is ignored, thus `null`) specify the index:

```cs
var writer = new PropertyWriter<Foo>();
writer.GetValue<int, string>(foo, null, "new-value", 1);
```