# Reusable.ConfigWhiz v1.0.0-alpha

The new `ConfigWhiz` is similar to `SmartConfig` in that it does not use any magic strings. All configurations and datatypes are strongly typed. It is however more powerfull. Here are the highlights:

- It does not use static classes any more. The `Configuration` object can be used with DI.
- It can use multiple datastores that are not bound to any container. Datastores are tested one after another and the first one that contains a setting is used and associated with the setting. The setting will be saved in the same datastore. 
- Each setting (a property of a container) can use a different datastore.
- It can associate settings with instances `Some.Namespace.Consumer["InstanceName"].Container.Setting["ElementName"]`

---

Example as used in tests:

```cs

var ns = typeof(Foo).Namespace;

var datastores = new IDatastore[]
{
    new Memory("Memory1")
    {
        { $@"{ns}.Foo.Bar.Qux", "quux" }
    },
    new Memory("Memory2")
    {
        { $@"{ns}.Foo.Bar.Baz", "bar" },
        { $@"{ns}.Foo[""qux""].Bar.Baz", "bar" }
    },
    new Memory("Memory3").AddRange(SettingFactory.ReadSettings<Foo>()), 
};

var configuration = new Configuration(Datastores);

var load = configuration.Load<Foo, Bar>(); // returns Result<Bar>

load.Succees.Verify().IsTrue(); // Reusable.Fuse
load.Value.Verify().IsNotNull();
load.Value.Baz.Verify().IsEqual("bar");

var bar = load.Value;

```

where

```cs

public class Foo
{
    public string Name { get; set; }
}

public class Bar
{
    public string Baz { get; set; }
}
```
