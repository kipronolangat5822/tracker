using System.ComponentModel.DataAnnotations;
using api.Data;
using api.Mappers;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class InventoryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InventoryService> _logger;
        public InventoryService(AppDbContext dbContext, ILogger<InventoryService> logger)
        {
            _context = dbContext;
            _logger = logger;
        }

        public async Task<List<InventoryItemDto>> GetInventoriesAsync()
        {
            var items=await _context.Inventories.ToListAsync();
            return items.Select(i=>i.ToReadDTO()).ToList();
        }
       public async Task<InventoryItemDto> AddInventoryItemAsync(CreateInventoryItemDto dto)
{
    if (dto == null)
        throw new ArgumentNullException(nameof(dto));
    if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ValidationException("Name is required.");
    if (dto.Name.Length > 100)
        throw new ValidationException("Name cannot exceed 100 characters.");

    if (string.IsNullOrWhiteSpace(dto.Category))
        throw new ValidationException("Category is required.");
    if (dto.Category.Length > 50)
        throw new ValidationException("Category cannot exceed 50 characters.");

    if (dto.Quantity <= 0)
        throw new ValidationException("Quantity must be greater than 0.");

    if (string.IsNullOrWhiteSpace(dto.Unit))
        throw new ValidationException("Unit is required.");
    if (dto.Unit.Length > 20)
        throw new ValidationException("Unit cannot exceed 20 characters.");

    if (string.IsNullOrWhiteSpace(dto.Location))
        throw new ValidationException("Location is required.");
    if (dto.Location.Length > 50)
        throw new ValidationException("Location cannot exceed 50 characters.");

    if (dto.ExpiryDate.HasValue && dto.ExpiryDate.Value.Date < dto.PurchaseDate.Date)
        throw new ValidationException("Expiry date cannot be before purchase date.");

    var inventory = new Inventory
    {
        Id = Guid.NewGuid(),
        Name = dto.Name,
        Category = dto.Category,
        Quantity = dto.Quantity,
        Unit = dto.Unit,
        Location = dto.Location,
        PurchaseDate = dto.PurchaseDate,
        ExpiryDate = dto.ExpiryDate
    };

    _context.Inventories.Add(inventory);
    await _context.SaveChangesAsync();

   
    return inventory.ToReadDTO();
}

    }
}
//    public string DaysToExpiry()
//         {
//             if (ExpiryDate == null)
//             {
//                 return "No expiry date set";
//             }

//             var daysLeft = (ExpiryDate.Value - DateTime.Now).Days;

//             if (daysLeft < 0)
//             {
//                 return "Expired";
//             }
//             else if (daysLeft == 0)
//             {
//                 return "Expires today";
//             }
//             else
//             {
//                 return $"{daysLeft} days left";
//             }
//         }