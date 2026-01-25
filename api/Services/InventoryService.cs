using System.ComponentModel.DataAnnotations;
using api.Data;
using api.Mappers;
using api.Models;
using Humanizer;
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

        public async Task<PaginatedResult<InventoryItemDto>> GetInventoriesAsync(QueryParameters queryParams)
        {
            var query = _context.Inventories.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var searchLower = queryParams.SearchTerm.ToLower();
                query = query.Where(i => 
                    i.Name.ToLower().Contains(searchLower) ||
                    i.Category.ToLower().Contains(searchLower) ||
                    i.Location.ToLower().Contains(searchLower)
                );
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderBy(i => i.Name)
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return new PaginatedResult<InventoryItemDto>
            {
                Items = items.Select(i => i.ToReadDTO()).ToList(),
                TotalCount = totalCount,
                PageNumber = queryParams.PageNumber,
                PageSize = queryParams.PageSize
            };
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

        public async Task<InventoryItemDto> GetInventoryByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ValidationException("Invalid inventory ID.");
            }
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == id);
            if (inventory == null)
            {
                throw new KeyNotFoundException("Inventory item not found.");
            }
            return inventory.ToReadDTO();
        }

        public async Task DeleteInventoryItemAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ValidationException("Invalid inventory ID.");
            }
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == id);
            if (inventory == null)
            {
                throw new KeyNotFoundException("Inventory item not found.");
            }
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
        }

       public async Task<InventoryItemDto> UpdateInventoryItemAsync(Guid id, CreateInventoryItemDto dto)
        {
            if (id == Guid.Empty)
            {
                throw new ValidationException("Invalid inventory ID.");
            }
            var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == id);
            if (inventory == null)
            {
                throw new KeyNotFoundException("Inventory item not found.");
            }

            
            inventory.Name = dto.Name;
            inventory.Category = dto.Category;
            inventory.Quantity = dto.Quantity;
            inventory.Unit = dto.Unit;
            inventory.Location = dto.Location;
            inventory.PurchaseDate = dto.PurchaseDate;
            inventory.ExpiryDate = dto.ExpiryDate;

            await _context.SaveChangesAsync();

            return inventory.ToReadDTO();
        }
    }
}