namespace CookEase.Domain.Entities;

public sealed class User
{
    public const int MaxDisplayNameLength = 200;

    private User() { }

    public User(Guid id, string displayName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("A user ID is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        displayName = displayName.Trim();

        if (displayName.Length > MaxDisplayNameLength)
        {
            throw new ArgumentException(
                $"Display name cannot exceed {MaxDisplayNameLength} characters.", nameof(displayName));
        }

        Id = id;
        DisplayName = displayName;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public ICollection<FridgeItem> FridgeItems { get; private set; } = [];
}
