using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BilgisayarMuhendisligiTasarimi.Data.Entities;

namespace BilgisayarMuhendisligiTasarimi.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial admin role
            builder.HasData(new Role
            {
                Id = 1,
                Name = "SuperAdmin",
                CreateDate = DateTime.UtcNow
            }, new Role
            {
                Id = 2,
                Name = "Admin",
                CreateDate = DateTime.UtcNow
            }, new Role
            {
                Id = 3,
                Name = "User",
                CreateDate = DateTime.UtcNow
            });

        }
    }
}