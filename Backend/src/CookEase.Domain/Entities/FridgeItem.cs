using CookEase.Domain.Enums;

namespace CookEase.Domain.Entities;

public sealed class FridgeItem
{
    public const int MaxItemNameLength = 200;

    private FridgeItem() { }

    public FridgeItem(
        Guid userId,
        string itemName,
        decimal amount,
        MeasurementUnit unit,
        DateOnly? expirationDate,
        StorageLocation storageLocation)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("An owner is required.", nameof(userId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(itemName);
        itemName = itemName.Trim();

        if (itemName.Length > MaxItemNameLength)
        {
            throw new ArgumentException(
                $"Item name cannot exceed {MaxItemNameLength} characters.", nameof(itemName));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        if (!Enum.IsDefined(unit))
        {
            throw new ArgumentOutOfRangeException(nameof(unit));
        }

        if (!Enum.IsDefined(storageLocation))
        {
            throw new ArgumentOutOfRangeException(nameof(storageLocation));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        ItemName = itemName;
        Amount = amount;
        Unit = unit;
        ExpirationDate = expirationDate;
        AddedAt = DateTimeOffset.UtcNow;
        StorageLocation = storageLocation;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string ItemName { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public MeasurementUnit Unit { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }
    public StorageLocation StorageLocation { get; private set; }
    public User User { get; private set; } = null!;
}
