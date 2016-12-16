using Reusable.Shelly.Commands;

namespace Reusable.Shelly
{
    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder RegisterHelpCommand(this CommandLineBuilder builder, IHelpWriter helpWriter)
        {
            return builder.Register<HelpCommand>(helpWriter);
        }
    }
}