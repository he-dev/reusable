using System.Diagnostics;
using System.Globalization;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public static class ExpressionInvokeLog
    {
        public static class Templates
        {
            public static RenderTreeNodeValueDelegate<IExpression, NodePlainView> Compact
            {
                get
                {
                    return (expression, depth) => new NodePlainView
                    {
                        Text = expression switch
                        {
                            // Id: 'Value' (valueType) [tag1 tag2]
                            IConstant c => $"{c.Id}: '{FormatValue(c.Value)}' ({c.Value?.GetType().ToPrettyString()}){FormatTags(c)}",
                            {} e => $"{e.Id} ({e.GetType().ToPrettyString()}){FormatTags(e)}",
                            _ => "null"
                        },
                        Depth = depth
                    };
                }
            }
        }

        private static string FormatValue(object? result)
        {
            return result switch
            {
                double d => d.ToString("F2", CultureInfo.InvariantCulture),
                bool b => b.ToString(),
                string s => s,
                {} x => x.GetType().ToPrettyString(),
                _ => "null",
            };
        }

        private static string FormatTags(IExpression expression)
        {
            return expression.Tags switch
            {
                {} tags => $" {tags.Join(" ")}",
                _ => string.Empty
            };
        }
    }
}