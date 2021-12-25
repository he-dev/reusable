using System.Threading;
using System.Threading.Tasks;

namespace Reusable.DoubleDash.Commands;

public delegate Task ExecuteDelegate<in TParameter>(ArgumentName name, TParameter parameter, CancellationToken cancellationToken = default) where TParameter : class, new();

public class Lambda<TParameter> : Command<TParameter> where TParameter : class, new()
{
    private readonly ExecuteDelegate<TParameter> _execute;

    public Lambda(ArgumentName name, ExecuteDelegate<TParameter> execute) : base(name) => _execute = execute;

    protected override Task ExecuteAsync(TParameter parameter, CancellationToken cancellationToken) => _execute(Name, parameter, cancellationToken);
}