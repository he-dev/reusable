using Microsoft.EntityFrameworkCore;

namespace Reusable.Synergy;

public abstract class SettingContext<T> : DbContext where T : class
{
    public DbSet<T> Settings { get; set; } = default!;

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     //optionsBuilder.UseSqlServer(_connectionString);
    // }
}