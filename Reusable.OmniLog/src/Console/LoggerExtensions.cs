using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Console
{
    public static class LoggerExtensions
    {
        /// <summary>
        /// Writes to console without line breaks.
        /// </summary>
        /// <param name="console">The logger to log.</param>
        /// <param name="style">Overrides the default console style.</param>
        /// <param name="builders">Console template builders used to render the output.</param>
        public static void Write(this ILogger console, ConsoleStyle? style, params ConsoleTemplateBuilder[] builders)
        {
            console.Log(log => log.ConsoleTemplateBuilder(new CompositeConsoleTemplateBuilder
            {
                IsParagraph = false,
                Builders = builders
            }).ConsoleStyle(style));
        }

        /// <summary>
        /// Writes to console line breaks.
        /// </summary>
        /// <param name="console">The logger to log.</param>
        /// <param name="style">Overrides the default console style.</param>
        /// <param name="builders">Console template builders used to render the output.</param>
        public static void WriteLine(this ILogger console, ConsoleStyle? style, params ConsoleTemplateBuilder[] builders)
        {
            console.Log(log => log.ConsoleTemplateBuilder(new CompositeConsoleTemplateBuilder
            {
                IsParagraph = true,
                Builders = builders
            }).ConsoleStyle(style));
        }
    }
}