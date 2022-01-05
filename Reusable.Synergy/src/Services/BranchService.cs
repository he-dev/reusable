using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Synergy.Services;

[PublicAPI]
public class BranchService : Service
{
    public ICondition When { get; set; } = Condition.False;

    public PipelineBuilder Services { get; } = new();

    private IService? First { get; set; }

    public override async Task<object> InvokeAsync(IRequest request)
    {
        if (First is null && Next is { })
        {
            Services.Add(Next);
            First = Services.Build();
        }

        return
            When.Evaluate(request) && First is { }
                ? await First.InvokeAsync(request)
                : await InvokeNext(request);
    }
}