using Reusable.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Experiments;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Reusable;
using Reusable.Extensions;
using Reusable.Markup;

namespace SmartLibs.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            
            ConsoleColorizer.RenderLine("<p>&gt;<span background-color='red'>Hallo</span> x <span color='darkyellow'>colors!</span></p>");
            //var logger = LoggerFactory.CreateLogger("test");
            //logger.Trace(x => x.Message("blah"));
            //LogEntry.New().Debug().Message("blah").Log(logger);

            //var htmlRenderer = new HtmlRenderer(new HtmlFormatting());
            //var htmlFormatter = new MarkupFormatter(htmlRenderer);

            //var html = MarkupElement.Builder;

            //var table = html.table();
            //table
            //    .tbody(
            //        html.tr(html.td("foo"), html.td("bar")),
            //        html.tr(html.td("foo"), html.td("bar")))
            //    .style("font-size: 2em;");

            //var result = htmlRenderer.Render(table);

            //var table2 = html.table(
            //    table => table.tbody(
            //        tbody => tbody.tr(
            //            tr => tr.td(td => "1"), 
            //            tr => tr.td(td => "2")),
            //        tbody => tbody.tr(
            //            tr => tr.td(td => "3"), 
            //            tr => tr.td(td => "4")))
            //        .style("font-size: 2em;"));

            //var result2 = table2.ToString("html", htmlFormatter); // htmlRenderer.Render(table2);
            System.Console.ReadKey();
        }
    }
}

namespace Experiments2
{
    //class foo
    //{
    //    private void Start(Type interfaceToFind, Predicate<string> propertySelection)
    //    {
    //        //_interfaceToFind = interfaceToFind.Name;
    //        //_propertySelection = propertySelection;

    //        //IServiceProvider hostServiceProvider = (IServiceProvider)Host;
    //        EnvDTE.DTE dte = (EnvDTE.DTE)hostServiceProvider.GetService(typeof(EnvDTE.DTE));

    //        EnvDTE.ProjectItem containingProjectItem = dte.Solution.FindProjectItem("app.config");
    //        Project project = containingProjectItem.ContainingProject;

    //        foreach (ProjectItem pi in project.ProjectItems)
    //        {
    //            ProcessProjectItem(pi);
    //        }
    //    }

    //    private void ProcessProjectItem(ProjectItem pi)
    //    {
    //        FileCodeModel fcm = pi.FileCodeModel;

    //        if (fcm != null)
    //        {
    //            foreach (CodeElement ce in fcm.CodeElements)
    //            {
    //                CrawlElement(ce);
    //            }
    //        }

    //        if (pi.ProjectItems != null)
    //        {
    //            foreach (ProjectItem prji in pi.ProjectItems)
    //            {
    //                ProcessProjectItem(prji);
    //            }
    //        }
    //    }

    //    private void CrawlElement(CodeElement codeElement)
    //    {
    //        switch (codeElement.Kind)
    //        {

    //            case vsCMElement.vsCMElementNamespace:
    //            {
    //                ProcessNamespace(codeElement);
    //                break;
    //            }
    //            case vsCMElement.vsCMElementClass:
    //            {
    //                ProcessClass(codeElement);
    //                break;
    //            }
    //            default:
    //            {
    //                return;
    //            }
    //        }
    //    }

    //    private void ProcessNamespace(CodeElement codeElement)
    //    {
    //        foreach (CodeElement m in ((CodeNamespace)codeElement).Members)
    //        {
    //            CrawlElement(m);
    //        }
    //    }

    //    private void ProcessClass(CodeElement codeElement)
    //    {
    //        CodeClass2 codeClass = (CodeClass2)codeElement;

    //        if (codeClass.ClassKind != vsCMClassKind.vsCMClassKindPartialClass) { return; }

    //        foreach (var i in codeClass.ImplementedInterfaces)
    //        {
    //            if (((CodeElement)i).Name.Equals(_interfaceToFind))
    //            {
    //                GenerateCode(codeClass);
    //            }
    //        }
    //    }

    //    private void GenerateCode(CodeClass classObj)
    //    {
    //        WriteClassStart(classObj.Name, GetAccessString(classObj), classObj.Namespace.Name);

    //        GenerateProperties(classObj);

    //        WriteClassEnd(classObj.Name, GetAccessString(classObj), classObj.Namespace.Name);
    //    }

    //    private void GenerateProperties(CodeClass classObj)
    //    {
    //        foreach (CodeElement childElement in classObj.Children)
    //        {
    //            if (childElement.Kind != vsCMElement.vsCMElementVariable) { continue; }

    //            CodeVariable childVariable = childElement as CodeVariable;

    //            if (!_propertySelection(childVariable.Name)) { continue; }

    //            WriteProperty(childVariable.Name, childVariable.Type.CodeType.FullName);
    //        }
    //    }

    //    private string GetAccessString(CodeClass classObj)
    //    {
    //        string access = string.Empty;
    //        switch (classObj.Access)
    //        {
    //            case vsCMAccess.vsCMAccessPublic: { access = "public"; break; }
    //            case vsCMAccess.vsCMAccessPrivate: { access = "private"; break; }
    //            case vsCMAccess.vsCMAccessDefault: { access = "public"; break; }
    //            case vsCMAccess.vsCMAccessProtected: { access = "protected"; break; }
    //            case vsCMAccess.vsCMAccessAssemblyOrFamily: { access = "internal"; break; }
    //            default: { access = "internal"; break; }
    //        }

    //        return access;
    //    }
    //}

}

namespace Experiments
{
    //public interface IMarkupElement : ICollection<object>
    //{
    //    string Name { get; }
    //    IDictionary<string, string> Attributes { get; }
    //    IMarkupElement Parent { get; set; }
    //    int Depth { get; }
    //    void AddRange(IEnumerable<object> items);
    //}

    //[DebuggerDisplay("{DebuggerDisplay,nq}")]
    //public class MarkupElement : IMarkupElement
    //{
    //    private readonly List<object> _content = new List<object>();

    //    public MarkupElement(string name, IEnumerable<object> content)
    //    {
    //        Name = name.NullIfEmpty() ?? throw new ArgumentNullException(nameof(name));
    //        AddRange(content ?? throw new ArgumentNullException(nameof(content)));
    //        Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    //    }

    //    public MarkupElement(string name, IEnumerable<Func<IMarkupElement, object>> content)
    //    {
    //        Name = name.NullIfEmpty() ?? throw new ArgumentNullException(nameof(name));
    //        foreach (var item in content ?? throw new ArgumentNullException(nameof(content))) item(this);
    //        Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    //    }

    //    private string DebuggerDisplay => $"<{Name} attribute-count=\"{Attributes?.Count ?? 0}\" children-count=\"{Count}\">";

    //    #region IMarkupElement

    //    public static IMarkupElement Builder => default(IMarkupElement);
    //    public string Name { get; }
    //    public IDictionary<string, string> Attributes { get; }
    //    public IMarkupElement Parent { get; set; }

    //    public int Depth => Parent?.Depth + 1 ?? 0;

    //    #endregion

    //    #region ICollection<object>

    //    public int Count => _content.Count;
    //    public bool IsReadOnly => false;
    //    public void Add(object item)
    //    {
    //        switch (item)
    //        {
    //            case IMarkupElement e: e.Parent = this; break;
    //        }
    //        _content.Add(item);
    //    }
    //    public bool Contains(object item) => _content.Contains(item);
    //    public bool Remove(object item) => _content.Remove(item);
    //    public void Clear() => _content.Clear();
    //    void ICollection<object>.CopyTo(object[] array, int arrayIndex) => _content.CopyTo(array, arrayIndex);
    //    public IEnumerator<object> GetEnumerator() => _content.GetEnumerator();
    //    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    //    #endregion       

    //    public void AddRange(IEnumerable<object> items)
    //    {
    //        foreach (var item in items ?? throw new ArgumentNullException(nameof(items))) Add(item);
    //    }
    //}

    //public static class MarkupElementExtensions
    //{
    //    public static IMarkupElement createElement(this IMarkupElement @this, string name, params object[] content)
    //    {
    //        var element = new MarkupElement(name, content);
    //        @this?.Add(element);
    //        return element;
    //    }

    //    public static IMarkupElement createElement(this IMarkupElement @this, string name, params Func<IMarkupElement, object>[] content)
    //    {
    //        var element = new MarkupElement(name, content);
    //        @this?.Add(element);
    //        return element;
    //    }

    //    public static IMarkupElement attr(this IMarkupElement @this, string name, string value) => @this.Tee(e => e.Attributes[name] = value);
    //}

    //public static class Html
    //{
    //    public static IMarkupElement h1(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h1), content);
    //    public static IMarkupElement h2(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h2), content);
    //    public static IMarkupElement h3(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h3), content);
    //    public static IMarkupElement h4(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h4), content);
    //    public static IMarkupElement h5(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h5), content);
    //    public static IMarkupElement h6(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h6), content);

    //    public static IMarkupElement p(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(p), content);
    //    public static IMarkupElement p(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(p), content);

    //    public static IMarkupElement div(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(div), content);
    //    public static IMarkupElement span(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(span), content);

    //    public static IMarkupElement table(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(table), content);
    //    public static IMarkupElement thead(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(thead), content);
    //    public static IMarkupElement tbody(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(tbody), content);
    //    public static IMarkupElement tfoot(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(tfoot), content);
    //    public static IMarkupElement th(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(th), content);
    //    public static IMarkupElement tr(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(tr), content);
    //    public static IMarkupElement td(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(td), content);

    //    public static IMarkupElement table(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(table), content);
    //    public static IMarkupElement tbody(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(tbody), content);
    //    public static IMarkupElement tr(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(tr), content);
    //    public static IMarkupElement td(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(td), content);

    //    public static IMarkupElement id(this IMarkupElement @this, string id) => @this.attr("id", id);
    //    public static IMarkupElement style(this IMarkupElement @this, params string[] css) => @this.attr("style", string.Join("; ", css.Select(c => $"{c.Trim().TrimEnd(';')};")));
    //}

    //[Flags]
    //public enum MarkupFormattingOptions
    //{
    //    None = 0,
    //    PlaceOpeningTagOnNewLine = 1,
    //    PlaceClosingTagOnNewLine = 2,
    //    PlaceBothTagsOnNewLine =
    //        PlaceOpeningTagOnNewLine |
    //        PlaceClosingTagOnNewLine,
    //    IsVoid = 4,
    //    CloseEmptyTag = 8
    //}

    //public interface ISanitizer
    //{
    //    string Sanitize(object value, IFormatProvider formatProvider);
    //}

    //public class HtmlSanitizer : ISanitizer
    //{
    //    public string Sanitize(object value, IFormatProvider formatProvider)
    //    {
    //        return System.Web.HttpUtility.HtmlEncode(string.Format(formatProvider, "{0}", value));
    //    }
    //}

    //public interface IMarkupRenderer
    //{
    //    string Render(IMarkupElement markupElement);
    //}

    //public abstract class MarkupRenderer : IMarkupRenderer
    //{
    //    private readonly IMarkupFormatting _formatting;
    //    private readonly ISanitizer _sanitizer;
    //    private readonly IFormatProvider _formatProvider;

    //    protected MarkupRenderer(IMarkupFormatting formatting, ISanitizer sanitizer, IFormatProvider formatProvider)
    //    {
    //        _formatting = formatting ?? throw new ArgumentNullException(nameof(formatting));
    //        _sanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));
    //        _formatProvider = formatProvider ?? throw new ArgumentNullException(nameof(formatProvider));
    //    }

    //    protected MarkupRenderer(IMarkupFormatting formatting, ISanitizer sanitizer)
    //        : this(formatting, sanitizer, CultureInfo.InvariantCulture)
    //    { }

    //    #region IMarkupRenderer

    //    public string Render(IMarkupElement markupElement)
    //    {
    //        var content = (markupElement ?? throw new ArgumentNullException(nameof(markupElement))).Aggregate(
    //            new StringBuilder(),
    //            (result, next) => result.Append(
    //                next is IMarkupElement e
    //                    ? Render(e)
    //                    : _sanitizer.Sanitize(next, _formatProvider)
    //            )
    //        )
    //        .ToString();

    //        var indent = markupElement.Parent != null;
    //        var placeOpeningTagOnNewLine = _formatting[markupElement.Name].HasFlag(MarkupFormattingOptions.PlaceOpeningTagOnNewLine);
    //        var placeClosingTagOnNewLine = _formatting[markupElement.Name].HasFlag(MarkupFormattingOptions.PlaceClosingTagOnNewLine);
    //        var hasClosingTag = _formatting[markupElement.Name].HasFlag(MarkupFormattingOptions.IsVoid) == false;
    //        var indentString = IndentString(_formatting.IndentWidth, markupElement.Depth);

    //        var html =
    //            new StringBuilder()
    //                .Append(IndentTag(placeOpeningTagOnNewLine, indent, indentString))
    //                .Append(RenderOpeningTag(markupElement.Name, markupElement.Attributes))
    //                .AppendWhen(() => hasClosingTag, sb => sb
    //                    .Append(content)
    //                    .Append(IndentTag(placeClosingTagOnNewLine, indent, indentString))
    //                    .Append(RenderClosingTag(markupElement.Name)));

    //        return html.ToString();
    //    }

    //    #endregion

    //    private static string IndentTag(bool newLine, bool indent, string indentString)
    //    {
    //        return
    //            new StringBuilder()
    //                .AppendWhen(() => newLine, sb => sb.AppendLine())
    //                .AppendWhen(() => newLine && indent, sb => sb.Append(indentString))
    //                .ToString();
    //    }

    //    private static string RenderOpeningTag(string tag, IEnumerable<KeyValuePair<string, string>> attributes)
    //    {
    //        return
    //            new StringBuilder()
    //                .Append($"<{tag}")
    //                .AppendWhen(
    //                    () => RenderAttributes(attributes).ToList(),
    //                    attributeStrings => attributeStrings.Any(),
    //                    (sb, attributeStrings) => sb.Append($" {(string.Join(" ", attributeStrings))}"))
    //                .Append(">").ToString();
    //    }

    //    private static IEnumerable<string> RenderAttributes(IEnumerable<KeyValuePair<string, string>> attributes)
    //    {
    //        return attributes.Select(attr => $"{attr.Key}=\"{attr.Value}\"");
    //    }

    //    private static string RenderClosingTag(string tag)
    //    {
    //        return $"</{tag}>";
    //    }

    //    private static string IndentString(int indentWidth, int depth)
    //    {
    //        return new string(' ', indentWidth * depth);
    //    }
    //}

    //public class HtmlRenderer : MarkupRenderer
    //{
    //    public HtmlRenderer(IMarkupFormatting formatting, ISanitizer sanitizer, IFormatProvider formatProvider)
    //        : base(formatting, sanitizer, formatProvider)
    //    { }

    //    public HtmlRenderer(IMarkupFormatting formatting, ISanitizer sanitizer)
    //        : this(formatting, sanitizer, CultureInfo.InvariantCulture)
    //    { }

    //    public HtmlRenderer(IMarkupFormatting formatting)
    //        : this(formatting, new HtmlSanitizer(), CultureInfo.InvariantCulture)
    //    { }
    //}

    //public interface IMarkupFormatting
    //{
    //    MarkupFormattingOptions this[string name] { get; }

    //    int IndentWidth { get; }
    //}

    //public abstract class MarkupFormatting : Dictionary<string, MarkupFormattingOptions>, IMarkupFormatting
    //{
    //    public new MarkupFormattingOptions this[string tag]
    //    {
    //        get =>
    //            TryGetValue(tag, out MarkupFormattingOptions tagFormattingOptions)
    //                ? tagFormattingOptions
    //                : MarkupFormattingOptions.None;
    //        set => base[tag] = value;
    //    }

    //    public int IndentWidth { get; set; }
    //}

    //public class HtmlFormatting : MarkupFormatting
    //{
    //    public const int DefaultIndentWidth = 4;

    //    public HtmlFormatting() : this(DefaultIndentWidth)
    //    {
    //        this["body"] = MarkupFormattingOptions.PlaceClosingTagOnNewLine;
    //        this["br"] = MarkupFormattingOptions.IsVoid;
    //        //this["span"] = MarkupFormattingOptions.None;
    //        this["p"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["pre"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["h1"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["h2"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["h3"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["h4"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["h5"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["h6"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["ul"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
    //        this["ol"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
    //        this["li"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["table"] = MarkupFormattingOptions.PlaceClosingTagOnNewLine;
    //        this["caption"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["thead"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
    //        this["tbody"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
    //        this["tfoot"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
    //        this["tr"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
    //        this["th"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["td"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["colgroup"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["col"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
    //        this["hr"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine | MarkupFormattingOptions.IsVoid;
    //    }

    //    public HtmlFormatting(int indentWidth)
    //    {
    //        IndentWidth = indentWidth;
    //    }
    //}

    //public static class StringBuilderExtensions
    //{
    //    public static StringBuilder AppendWhen(this StringBuilder @this, Func<bool> predicate, Func<StringBuilder, StringBuilder> append)
    //    {
    //        return predicate() ? append(@this) : @this;
    //    }

    //    public static StringBuilder AppendWhen<T>(this StringBuilder @this, Func<T> getValue, Func<T, bool> predicate, Func<StringBuilder, T, StringBuilder> append)
    //    {
    //        var value = getValue();
    //        return predicate(value) ? append(@this, value) : @this;
    //    }
    //}

    //public static class FunctionalExtensions
    //{
    //    public static T Tee<T>(this T @this, Action<T> tee)
    //    {
    //        tee(@this);
    //        return @this;
    //    }
    //}
}
