using System.Threading.Tasks;
using Reusable.Essentials.Extensions;

namespace Reusable.Synergy.Controllers;

public static class ConstantService
{
    public class Text : Service
    {
        public Text(string value) => Value = value;

        private string Value { get; }

        public override Task<object> InvokeAsync(IRequest request)
        {
            return
                request is IRequest<string>
                    ? Value.ToTask<object>()
                    : Unit.Default.ToTask<object>();
        }
    }
}