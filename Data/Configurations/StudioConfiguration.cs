using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BilgisayarMuhendisligiTasarimi.Data.Entities;

namespace BilgisayarMuhendisligiTasarimi.Data.Configurations
{
    public class StudioConfiguration : IEntityTypeConfiguration<Studio>
    {
        public void Configure(EntityTypeBuilder<Studio> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(x => x.CreateDate).HasDefaultValueSql("getdate()");

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasMany(s => s.Users)
                .WithOne(u => u.Studio)
                .HasForeignKey(u => u.StudioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.TaskItems)
                .WithOne(t => t.Studio)
                .HasForeignKey(t => t.StudioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
