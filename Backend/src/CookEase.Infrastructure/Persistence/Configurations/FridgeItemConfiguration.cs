using CookEase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CookEase.Infrastructure.Persistence.Configurations;

public sealed class FridgeItemConfiguration : IEntityTypeConfiguration<FridgeItem>
{
    public void Configure(EntityTypeBuilder<FridgeItem> builder)
    {
        builder.ToTable("FridgeItems", table =>
        {
            table.HasCheckConstraint("CK_FridgeItems_Amount_Positive", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_FridgeItems_ItemName_NotBlank", "length(btrim(\"ItemName\")) > 0");
            table.HasCheckConstraint("CK_FridgeItems_Unit_Valid",
                "\"Unit\" IN ('Piece', 'Gram', 'Kilogram', 'Milliliter', 'Liter')");
            table.HasCheckConstraint("CK_FridgeItems_StorageLocation_Valid",
                "\"StorageLocation\" IN ('Refrigerator', 'Freezer', 'Pantry', 'Countertop')");
        });

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.ItemName)
            .HasMaxLength(FridgeItem.MaxItemNameLength)
            .IsRequired();
        builder.Property(item => item.Amount).HasColumnType("numeric");
        builder.Property(item => item.Unit).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(item => item.StorageLocation).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(item => item.ExpirationDate).HasColumnType("date");
        builder.Property(item => item.AddedAt).HasColumnType("timestamp with time zone");

        builder.HasIndex(item => new { item.UserId, item.ExpirationDate });
    }
}
