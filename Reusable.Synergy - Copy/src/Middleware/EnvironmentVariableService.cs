using System;
using System.Threading.Tasks;

namespace Reusable.Synergy.Middleware;

public class EnvironmentVariableService<T> : Service<T>
{
    public EnvironmentVariableService(IPropertyService<string> property) => Property = property;

    private IPropertyService<string> Property { get; }

    public override async Task<T> InvokeAsync()
    {
        Property.SetValue(Last, Environment.ExpandEnvironmentVariables(Property.GetValue(Last)));

        return await InvokeNext()!;
    }
}