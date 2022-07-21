using System.Threading;
using System.Threading.Tasks;

namespace Reusable.DoubleDash.Commands;

public delegate Task ExecuteDelegate<in TParameter>(NameCollection nameCollection, TParameter parameter, CancellationToken cancellationToken = default) where TParameter : class, new();

public class Lambda<TParameter> : Command<TParameter> where TParameter : class, new()
{
    private readonly ExecuteDelegate<TParameter> _execute;

    public Lambda(NameCollection nameCollection, ExecuteDelegate<TParameter> execute) : base(nameCollection) => _execute = execute;

    protected override Task ExecuteAsync(TParameter parameter, CancellationToken cancellationToken) => _execute(NameCollection, parameter, cancellationToken);
}