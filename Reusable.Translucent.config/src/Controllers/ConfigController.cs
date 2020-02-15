using Reusable.OneTo1;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    /// <summary>
    /// This is the base class for controllers handling the 'config' scheme.
    /// </summary>
    public abstract class ConfigController : ResourceController<ConfigRequest>
    {
        protected ConfigController(ControllerName name) : base(name, default) { }

        public ITypeConverter Converter { get; set; } = TypeConverter.PassThru;
    }
}