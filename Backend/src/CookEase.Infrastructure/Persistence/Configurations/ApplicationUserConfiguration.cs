using CookEase.Domain.Entities;
using CookEase.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookEase.Infrastructure.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasOne(account => account.Profile)
            .WithOne()
            .HasForeignKey<User>(profile => profile.Id)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(account => account.NormalizedEmail).IsUnique();
    }
}
