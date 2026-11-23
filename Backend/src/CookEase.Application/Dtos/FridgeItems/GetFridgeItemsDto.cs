using CookEase.Domain.Enums;

namespace CookEase.Application.Dtos.FridgeItems;

public class GetFridgeItemsDto
{
    public Guid Id { get; init; }
    public required string ItemName { get; init; }
    public decimal Amount { get; init; }
    public MeasurementUnit Unit { get; init; }
    public DateOnly? ExpirationDate { get; init; }
    public DateTimeOffset AddedAt { get; init; }
    public StorageLocation StorageLocation { get; init; }
}
