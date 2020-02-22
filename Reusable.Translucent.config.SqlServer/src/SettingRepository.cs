using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Reusable.Translucent
{
    public interface ISettingRepository<TSetting> where TSetting : ISetting
    {
        Task<TSetting> ReadSetting(string name, CancellationToken cancellationToken);

        Task<int> CreateOrUpdateSetting(string name, string value, CancellationToken cancellationToken);
    }

    public delegate Expression<Func<T, bool>> WhereDelegate<T>(string name) where T : ISetting;

    public class SettingRepository<T> : ISettingRepository<T> where T : class, ISetting
    {
        private readonly string _connectionString;

        public SettingRepository(string connectionString) => _connectionString = connectionString;

        public WhereDelegate<T> Where { get; set; } = name => setting => setting.Name == name;

        public async Task<T> ReadSetting(string name, CancellationToken cancellationToken)
        {
            using var context = new SettingContext<T>(_connectionString);
            return await context.Settings.Where(Where(name)).SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<int> CreateOrUpdateSetting(string name, string value, CancellationToken cancellationToken)
        {
            using var context = new SettingContext<T>(_connectionString);
            var current = await context.Settings.Where(Where(name)).SingleOrDefaultAsync(cancellationToken);
            current.Value = value;
            return await context.SaveChangesAsync(cancellationToken);
        }
    }

    public interface ISetting
    {
        [Key]
        int Id { get; set; }

        string Name { get; set; }

        string Value { get; set; }
    }

    [Table("Setting", Schema = "dbo")]
    public class Setting : ISetting
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public string Value { get; set; } = default!;
    }

    public class SettingContext<T> : DbContext where T : class
    {
        private readonly string _connectionString;

        public SettingContext(string connectionString) => _connectionString = connectionString;

        public DbSet<T> Settings { get; set; } = default!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
    }
}