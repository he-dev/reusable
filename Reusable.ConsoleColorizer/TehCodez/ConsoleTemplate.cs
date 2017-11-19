using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.MarkupBuilder.Html;

namespace Reusable.ConsoleColorizer
{
    public interface IConsoleTemplate : IHtmlElement
    {
    }

    [UsedImplicitly]
    public class ConsoleTemplate : HtmlElement, IConsoleTemplate
    {
        public ConsoleTemplate([NotNull] string name) : base(name)
        {
        }

        [CanBeNull]
        public new static readonly ConsoleTemplate Builder = default;
    }
}