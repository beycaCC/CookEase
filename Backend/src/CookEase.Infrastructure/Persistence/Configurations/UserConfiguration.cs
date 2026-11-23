using CookEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookEase.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("UserProfiles", table =>
            table.HasCheckConstraint("CK_UserProfiles_DisplayName_NotBlank", "length(btrim(\"DisplayName\")) > 0"));

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.DisplayName)
            .HasMaxLength(User.MaxDisplayNameLength)
            .IsRequired();
        builder.Property(user => user.CreatedAt).HasColumnType("timestamp with time zone");

        builder.HasMany(user => user.FridgeItems)
            .WithOne(item => item.User)
            .HasForeignKey(item => item.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
