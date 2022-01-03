using System.Threading.Tasks;

namespace Reusable.Synergy.Services;

public class BranchService : Service
{
    public BranchService(ICondition condition) => Condition = condition;

    public ICondition Condition { get; }

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
            Condition.Evaluate(request) && First is { }
                ? await First.InvokeAsync(request)
                : await InvokeNext(request);
    }
}