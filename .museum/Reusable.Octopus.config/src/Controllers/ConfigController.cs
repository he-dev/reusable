using Reusable.Octopus.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers;

/// <summary>
/// This is the base class for controllers handling the 'config' scheme.
/// </summary>
public abstract class ConfigController : ResourceController<ConfigRequest>
{
    protected ConfigController() : base(new[] { "config" }) { }
    
    //public ITypeConverter Converter { get; set; } = new Never();
}