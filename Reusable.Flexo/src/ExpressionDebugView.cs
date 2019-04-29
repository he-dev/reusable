using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;

namespace Reusable.Flexo
{
    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class ExpressionDebugView
    {
        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.DisplayValue(x => x.Name);
            b.DisplayValue(x => x.Result);
            b.DisplayValue(x => x.Description);
        });

        public static ExpressionDebugView Root => new ExpressionDebugView
        {
            Name = "Root",
            Description = "This is the root node of the expression debug-view."
        };

        public static RenderValueCallback<ExpressionDebugView> DefaultRender
        {
            get { return (dv, d) => $"[{dv.Type}] as [{dv.Name}]: '{dv.Result}' ({dv.Description})"; }
        }

        public string Type { get; set; } = $"<{nameof(Type)}>";

        public string Name { get; set; } = $"<{nameof(Name)}>";

        public string Description { get; set; } = $"<{nameof(Description)}>";

        public object Result { get; set; } = $"<{nameof(Result)}>";
    }
}