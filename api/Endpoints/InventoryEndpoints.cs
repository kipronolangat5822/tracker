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
            group.MapGet("/", async (
                InventoryService inventoryService,
                [FromQuery] string? searchTerm = null,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var queryParams = new QueryParameters
                {
                    SearchTerm = searchTerm,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                var result = await inventoryService.GetInventoriesAsync(queryParams);
                return Results.Ok(result);
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
            group.MapGet("/{id:guid}", async (Guid id, InventoryService inventoryService) =>
            {
                var item = await inventoryService.GetInventoryByIdAsync(id);
                if (item == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(item);
            });
            group.MapPut("/{id:guid}", async (Guid id, [FromBody] CreateInventoryItemDto dto, InventoryService inventoryService) =>
            {
                try
                {
                    var updatedItem = await inventoryService.UpdateInventoryItemAsync(id, dto);
                    if (updatedItem == null)
                    {
                        return Results.NotFound();
                    }
                    return Results.Ok(updatedItem);
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(new { Error = ex.Message });
                }
            });
            group.MapDelete("/{id:guid}", async (Guid id, InventoryService inventoryService) =>
            {
                try
                {
                    await inventoryService.DeleteInventoryItemAsync(id);
                    return Results.NoContent();
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(new { Error = ex.Message });
                }
                catch (KeyNotFoundException)
                {
                    return Results.NotFound();
                }
            });
            group.WithOpenApi().WithTags("Inventory");
        }
    }
}