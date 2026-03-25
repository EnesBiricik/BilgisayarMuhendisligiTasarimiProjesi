using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BilgisayarMuhendisligiTasarimi.Data.Entities;

namespace BilgisayarMuhendisligiTasarimi.Data.Configurations
{
    public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
    {
        public void Configure(EntityTypeBuilder<TaskItem> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(x => x.CreateDate).HasDefaultValueSql("getdate()");

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .IsRequired();

            builder.Property(t => t.TaskType)
                .IsRequired();
            
            builder.Property(t => t.TaskStatus)
                .IsRequired();

            builder.Property(t => t.Priority)
                .IsRequired();

            builder.Property(t => t.Attachments)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");

            builder.HasOne(t => t.CreatorUser)
                .WithMany()
                .HasForeignKey(t => t.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AssignedUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict); // Maintain history even if user is deleted, or set null if nullable? 
                                                    // FK is nullable in Entity, so SetNull might be better, or Restrict to prevent deleting user with tasks. 
                                                    // Given "Game Studio", probably safer to Restrict delete of users with active tasks.
        }
    }
}
