using CookEase.Domain.Entities;

namespace CookEase.Application.Dtos.FridgeItems;

public class GetFridgeItemsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public string Unit { get; set; } = null!;
    public DateTime ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
}