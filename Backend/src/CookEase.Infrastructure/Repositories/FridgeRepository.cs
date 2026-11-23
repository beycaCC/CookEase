using Microsoft.EntityFrameworkCore;
using CookEase.Domain.Entities;
using CookEase.Domain.Interface;
using CookEase.Infrastructure.Persistence;

namespace CookEase.Infrastructure.Repositories
{
    public class FridgeRepository(CookEaseDbContext dbContext) : IFridgeRepository
    {
        public async Task<Guid> AddFridgeAsync(FridgeItem fridgeItem)
        {
            await dbContext.FridgeItems.AddAsync(fridgeItem);
            await dbContext.SaveChangesAsync();
            return fridgeItem.Id;
        }

        public async Task DeleteFridgeAsync(FridgeItem fridgeItem)
        {
            dbContext.FridgeItems.Remove(fridgeItem);
            await dbContext.SaveChangesAsync();
        }

        public async Task<ICollection<FridgeItem>> GetAllFridgesAsync()
        {
            return await dbContext.FridgeItems.ToListAsync();
        }

        public async Task<FridgeItem?> GetFridgeByIdAsync(Guid id)
        {
            return await dbContext.FridgeItems.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task UpdateFridgeAsync(FridgeItem fridgeItem)
        {
            dbContext.FridgeItems.Update(fridgeItem);
            await dbContext.SaveChangesAsync();
        }
    }
}