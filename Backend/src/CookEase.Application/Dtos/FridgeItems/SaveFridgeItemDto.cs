using System.ComponentModel.DataAnnotations;
using CookEase.Domain.Entities;
using CookEase.Domain.Enums;

namespace CookEase.Application.Dtos.FridgeItems;

public class SaveFridgeItemDto
{
    [Required]
    [MaxLength(FridgeItem.MaxItemNameLength)]
    public required string ItemName { get; init; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335",
        MinimumIsExclusive = true, ParseLimitsInInvariantCulture = true)]
    public decimal Amount { get; init; }

    [EnumDataType(typeof(MeasurementUnit))]
    public MeasurementUnit Unit { get; init; }

    public DateOnly? ExpirationDate { get; init; }

    [EnumDataType(typeof(StorageLocation))]
    public StorageLocation StorageLocation { get; init; }
}
