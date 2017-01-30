namespace Reusable.Markup.Renderers
{
    public class HtmlRenderer : IMarkupRenderer
    {
        public HtmlRenderer(MarkupFormatting markupFormatting = null)
        {
            MarkupRenderer = new MarkupRenderer(markupFormatting ?? new HtmlFormatting());
        }

        private MarkupRenderer MarkupRenderer { get; set; }

        public string Render(IElement element)
        {
            return MarkupRenderer.Render(element);
        }
    }
}
