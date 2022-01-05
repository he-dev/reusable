using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Reusable.Synergy;

public interface ISettingRepository<TSetting> where TSetting : ISetting
{
    Task<TSetting> ReadSetting(string name, CancellationToken cancellationToken);

    Task<int> CreateOrUpdateSetting(string name, string value, CancellationToken cancellationToken);
}

public delegate Expression<Func<T, bool>> WhereDelegate<T>(string name) where T : ISetting;

[PublicAPI]
public class SettingRepository<T> : ISettingRepository<T> where T : class, ISetting
{
    private readonly Func<SettingContext<T>> _createContext;

    public SettingRepository(Func<SettingContext<T>> createContext) => _createContext = createContext;

    public WhereDelegate<T> Where { get; set; } = name => setting => setting.Name == name;

    public async Task<T> ReadSetting(string name, CancellationToken cancellationToken)
    {
        await using var context = _createContext();
        return await context.Settings.Where(Where(name)).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CreateOrUpdateSetting(string name, string value, CancellationToken cancellationToken)
    {
        await using var context = _createContext();
        var current = await context.Settings.Where(Where(name)).SingleOrDefaultAsync(cancellationToken);
        current.Value = value;
        return await context.SaveChangesAsync(cancellationToken);
    }
}