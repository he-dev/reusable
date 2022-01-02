using System.Threading.Tasks;

namespace Reusable.Synergy.Controllers;

public static class Constant
{
    // Reads files as string.
    public class Text : Service<string>
    {
        public Text(string value) => Value = value;

        private string Value { get; }

        public override Task<string> InvokeAsync()
        {
            return Task.FromResult(Value);
        }
    }
}