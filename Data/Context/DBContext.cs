using BilgisayarMuhendisligiTasarimi.Data.Configurations;
using BilgisayarMuhendisligiTasarimi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BilgisayarMuhendisligiTasarimi.Data.Context
{
    public class DBContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DBContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Settings> Settings => Set<Settings>();
        public DbSet<ActivityLog> ActivityLog { get; set; }
        public DbSet<Studio> Studios => Set<Studio>();
        public DbSet<TaskItem> TaskItems => Set<TaskItem>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new SettingsConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new StudioConfiguration());
            modelBuilder.ApplyConfiguration(new TaskItemConfiguration());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            var connectionString = _configuration.GetConnectionString("Local1");
            optionsBuilder
                .UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .EnableSensitiveDataLogging();
            
        }
    }
}
