using CookEase.Application.Dtos.FridgeItems;
using CookEase.Application.Users;
using CookEase.Domain.Entities;
using CookEase.Domain.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookEase.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fridgeItems")]
public class FridgeController(IFridgeRepository repository, ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllFridgeItems()
    {
        var fridgeItems = await repository.GetAllFridgesAsync();

        var result = fridgeItems.Select(ToDto).ToList();

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFridgeItemById(Guid id)
    {
        var fridgeItem = await repository.GetFridgeByIdAsync(id);
        if (fridgeItem is null)
        {
            return NotFound();
        }

        return Ok(ToDto(fridgeItem));
    }

    [HttpPost]
    public async Task<IActionResult> AddFridgeItem([FromBody] SaveFridgeItemDto request)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Unauthorized();
        }

        var fridgeItem = new FridgeItem(
            userId, request.ItemName, request.Amount, request.Unit,
            request.ExpirationDate, request.StorageLocation);

        var id = await repository.AddFridgeAsync(fridgeItem);
        return CreatedAtAction(nameof(GetFridgeItemById), new { id }, ToDto(fridgeItem));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateFridgeItem(Guid id, [FromBody] SaveFridgeItemDto request)
    {
        var fridgeItem = await repository.GetFridgeByIdAsync(id);
        if (fridgeItem is null)
        {
            return NotFound();
        }

        fridgeItem.UpdateDetails(
            request.ItemName, request.Amount, request.Unit,
            request.ExpirationDate, request.StorageLocation);

        await repository.UpdateFridgeAsync(fridgeItem);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFridgeItem(Guid id)
    {
        var fridgeItem = await repository.GetFridgeByIdAsync(id);
        if (fridgeItem is null)
        {
            return NotFound();
        }

        await repository.DeleteFridgeAsync(fridgeItem);
        return NoContent();
    }

    private static GetFridgeItemsDto ToDto(FridgeItem item) => new()
    {
        Id = item.Id,
        ItemName = item.ItemName,
        Amount = item.Amount,
        Unit = item.Unit,
        ExpirationDate = item.ExpirationDate,
        AddedAt = item.AddedAt,
        StorageLocation = item.StorageLocation
    };
}
