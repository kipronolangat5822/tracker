using System.ComponentModel.DataAnnotations;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Endpoints
{
public static class InventoryEndpoints
    {
        public static void MapInventoryEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/inventories");
            group.MapGet("/", async (InventoryService inventoryService) =>
            {
                var items = await inventoryService.GetInventoriesAsync();
                return Results.Ok(items);
            });
            group.MapPost("/", async ([FromBody] CreateInventoryItemDto dto, InventoryService inventoryService) =>
            {
                try
                {
                    var createdItem = await inventoryService.AddInventoryItemAsync(dto);
                    return Results.Created($"/api/inventories/{createdItem.Id}", createdItem);
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(new { Error = ex.Message });
                }
            });
            group.WithOpenApi().WithTags("Inventory");
        }
    }
}