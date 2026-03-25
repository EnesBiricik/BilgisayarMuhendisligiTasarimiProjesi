using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BilgisayarMuhendisligiTasarimi.Data.Entities;

namespace BilgisayarMuhendisligiTasarimi.Data.Configurations
{
    public class SettingsConfiguration : IEntityTypeConfiguration<Settings>
    {
        public void Configure(EntityTypeBuilder<Settings> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CreateDate).HasDefaultValueSql("getdate()");
            builder.Property(x => x.Logo).IsRequired();
            builder.Property(x => x.Icon).IsRequired();
            builder.Property(x => x.Slogan).IsRequired();
            builder.Property(x => x.PhoneNumber).IsRequired();
            builder.Property(x => x.Email).IsRequired();


            builder.HasData(new Settings
            {
                Id = 1,
                Icon = "no-image.png",
                Logo = "no-image.png",
                Slogan = "Lorem Ipsum Lorem Ipsum.",
                PhoneNumber = "111112222211",
                Email = "Lorem@gmail.com",
            });

        }
    }
}
