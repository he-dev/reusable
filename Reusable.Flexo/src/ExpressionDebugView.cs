using System.CodeDom;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public class ExpressionDebugView
    {
        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.DisplayScalar(x => x.Name);
            b.DisplayScalar(x => x.Result);
            b.DisplayScalar(x => x.Description);
        });

        public static ExpressionDebugView Root => new ExpressionDebugView
        {
            Name = "Root",
            Description = $"This is the root node of the {nameof(ExpressionDebugView)}."
        };

        public static RenderTreeNodeValueCallback<ExpressionDebugView> DefaultRenderTreeNode
        {
            get { return (dv, d) => $"[{dv.Type}] as [{dv.Name}]: '{FormatResult(dv.Result)}' ({dv.Description})"; }
        }

        public string Type { get; set; } = $"<{nameof(Type)}>";

        public string Name { get; set; } = $"<{nameof(Name)}>";

        public string Description { get; set; } = $"<{nameof(Description)}>";

        public object Result { get; set; } = $"<{nameof(Result)}>";

        private static string FormatResult(object result)
        {
            switch (result)
            {
                case double d: return d.ToString("F2", CultureInfo.InvariantCulture);
                case bool b: return b.ToString();
                case string s: return s;
                case null: return "null";
                default: return result.GetType().ToPrettyString();
            }
        }
    }
}