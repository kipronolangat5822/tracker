using api.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace api.Mappers
{
    public static class InventoryMapper
    {
        public static InventoryItemDto ToReadDTO(this Inventory item)
        {
            return new InventoryItemDto
            {
        Id = item.Id,
        Name = item.Name,
        Category = item.Category,
        Quantity = item.Quantity,
        Unit = item.Unit,
        Location = item.Location,
        ExpiryDate = item.ExpiryDate,
        Status = item.Status,
        DaysUntilExpiry = item.ExpiryDate.HasValue
            ? (item.ExpiryDate.Value.Date - DateTime.UtcNow.Date).Days
            : null
            };
        }
        
       
    }
}