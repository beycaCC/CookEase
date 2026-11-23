using CookEase.Domain.Entities;

namespace CookEase.Domain.Interface
{
    public interface IFridgeRepository
    {
        Task<ICollection<FridgeItem>> GetAllFridgesAsync();
        Task<FridgeItem?> GetFridgeByIdAsync(Guid id);
        Task<Guid> AddFridgeAsync(FridgeItem fridgeItem);
        Task UpdateFridgeAsync(FridgeItem fridgeItem);
        Task DeleteFridgeAsync(FridgeItem fridgeItem);
    }
}