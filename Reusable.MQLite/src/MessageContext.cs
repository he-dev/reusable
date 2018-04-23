using Microsoft.EntityFrameworkCore;
using Reusable.MQLite.Entities;

namespace Reusable.MQLite
{
    internal class MessageContext : DbContext
    {
        private readonly string _connectionString;

        private readonly string _schema;

        public MessageContext(string connectionString, string schema)
        {
            _connectionString = connectionString;
            _schema = schema;
        }

        public DbSet<TimeRange> TimeRanges { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Queue> Queues { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<Message>(entity =>
            {
                entity
                    .ToTable(nameof(Message), _schema)
                    .HasKey(t => t.Id);

                entity
                    .Property(p => p.Id)
                    .UseSqlServerIdentityColumn(); //.ValueGeneratedOnAdd();

                entity
                    .Property(p => p.TimeRangeId)
                    .IsRequired();

                entity
                    .Property(p => p.Body)
                    .IsRequired();

                entity
                    .Property(p => p.Fingerprint)
                    .IsRequired();

                entity
                    .HasOne(p => p.TimeRange)
                    .WithMany(p => p.Messages);
                
                //e.Property(p => p.CreatedOn).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<TimeRange>(entity =>
            {
                entity
                    .ToTable(nameof(TimeRange), _schema)
                    .HasKey(t => t.Id);

                entity
                    .Property(p => p.Id).
                    UseSqlServerIdentityColumn();

                entity
                    .Property(p => p.StartsOn)
                    .IsRequired();

                entity
                    .Property(p => p.EndsOn)
                    .IsRequired();

                entity
                    .Property(p => p.CreatedOn)
                    .ValueGeneratedOnAdd();

                entity
                    .HasOne(p => p.Queue)
                    .WithMany(p => p.TimeRanges);

                entity
                    .HasMany(p => p.Messages)
                    .WithOne(p => p.TimeRange)
                    .OnDelete(DeleteBehavior.Cascade);


                //e.Property(p => p.Messages).IsRequired();
            });

            modelBuilder.Entity<Queue>(entity =>
            {
                entity
                    .ToTable(nameof(Queue), _schema)
                    .HasKey(t => t.Id);

                entity
                    .Property(p => p.Id)
                    .UseSqlServerIdentityColumn();

                entity
                    .Property(p => p.Name)
                    .IsRequired();                

                entity
                    .HasMany(p => p.TimeRanges)
                    .WithOne(p => p.Queue);
            });
        }
    }
}