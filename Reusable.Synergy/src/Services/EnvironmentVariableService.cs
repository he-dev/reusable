using System;
using System.Threading.Tasks;

namespace Reusable.Synergy.Services;

public class EnvironmentVariableService : Service
{
    public EnvironmentVariableService(IPropertyAccessor<string> property) => Property = property;

    private IPropertyAccessor<string> Property { get; }

    public override async Task<object> InvokeAsync(IRequest request)
    {
        Property.SetValue(request, Environment.ExpandEnvironmentVariables(Property.GetValue(request)));

        return await InvokeNext(request);
    }
}