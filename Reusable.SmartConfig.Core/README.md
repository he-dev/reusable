# Reusable.ConfigWhiz v1.0.0-beta

The new `ConfigWhiz` is similar to `SmartConfig` in that it does not use any magic strings. All configurations and data are strongly typed. It is however more powerful. 

#Basics

To be able to successfuly use `ConfigWhiz` you need to know two concepts it works with. The first one is the _consumer_. This is the type that _consumes_ some settings that are contained in a _container_. A _consumer_ is at the same time the provider for the first part of the name for its settings which belong to a _container_.

```cs

namespace Foo.Bar
{
    public class Baz
    {
        public string Qux { get; set; }
    }
}

```cs

```

public class Bar
{
    public string Baz { get; set; }
}
```


The setting name is now more powerful too so the old names will need small adjustments to work with `ConfigWhiz` but the data format does not change. Here are the highlights:

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
