

`Quickey` creates string keys from property hierarchy.


Default tokens:

- `[Prefix:][Name.space+][Type.]Member[[Index]]`

Examples:

```cs
[UsePrefix("global"), UseNamespace, UseType, UseMember, UseIndex?]
public class Car
{
    public string Make { get; set; }
}
```

This will create the following key: `global:Namespace+Car.Make`