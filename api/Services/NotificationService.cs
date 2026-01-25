using api.Data;
using api.Mappers;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(AppDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedResult<NotificationDto>> GetAllAsync(QueryParameters queryParams, bool unreadOnly = false)
        {
            var query = _context.Notifications.AsQueryable();

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var searchLower = queryParams.SearchTerm.ToLower();
                query = query.Where(n => 
                    n.Title.ToLower().Contains(searchLower) ||
                    n.Message.ToLower().Contains(searchLower)
                );
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var notifications = await query
                .OrderByDescending(n => n.CreatedDate)
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return new PaginatedResult<NotificationDto>
            {
                Items = notifications.Select(n => n.ToDto()).ToList(),
                TotalCount = totalCount,
                PageNumber = queryParams.PageNumber,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _context.Notifications.CountAsync(n => !n.IsRead);
        }

        public async Task<NotificationDto?> MarkAsReadAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return null;

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return notification.ToDto();
        }

        public async Task<bool> MarkAllAsReadAsync()
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return false;

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task GenerateInventoryExpiryNotificationsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var inventoryItems = await _context.Inventories
                .Where(i => i.ExpiryDate.HasValue)
                .ToListAsync();

            foreach (var item in inventoryItems)
            {
                if (!item.ExpiryDate.HasValue) continue;

                var daysUntilExpiry = (item.ExpiryDate.Value.Date - today).Days;

                
                var existingNotification = await _context.Notifications
                    .Where(n => n.RelatedItemId == item.Id && 
                                n.CreatedDate >= today.AddHours(-12))
                    .FirstOrDefaultAsync();

                if (existingNotification != null) continue;

                Notification? notification = null;

                if (daysUntilExpiry < 0)
                {
                   
                    notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = "Item Expired",
                        Message = $"{item.Name} has expired {Math.Abs(daysUntilExpiry)} days ago",
                        Type = NotificationType.EXPIRED,
                        Severity = NotificationSeverity.CRITICAL,
                        CreatedDate = DateTime.UtcNow,
                        IsRead = false,
                        RelatedItemId = item.Id,
                        RelatedItemType = "Inventory"
                    };
                }
                else if (daysUntilExpiry == 0)
                {
                  
                    notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = "Item Expires Today",
                        Message = $"{item.Name} expires today!",
                        Type = NotificationType.EXPIRY_URGENT,
                        Severity = NotificationSeverity.ERROR,
                        CreatedDate = DateTime.UtcNow,
                        IsRead = false,
                        RelatedItemId = item.Id,
                        RelatedItemType = "Inventory"
                    };
                }
                else if (daysUntilExpiry <= 3)
                {
                   
                    notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = "Urgent: Item Expiring Soon",
                        Message = $"{item.Name} will expire in {daysUntilExpiry} day{(daysUntilExpiry > 1 ? "s" : "")}",
                        Type = NotificationType.EXPIRY_URGENT,
                        Severity = NotificationSeverity.ERROR,
                        CreatedDate = DateTime.UtcNow,
                        IsRead = false,
                        RelatedItemId = item.Id,
                        RelatedItemType = "Inventory"
                    };
                }
                else if (daysUntilExpiry <= 7)
                {
                    
                    notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = "Item Expiring Soon",
                        Message = $"{item.Name} will expire in {daysUntilExpiry} days",
                        Type = NotificationType.EXPIRY_WARNING,
                        Severity = NotificationSeverity.WARNING,
                        CreatedDate = DateTime.UtcNow,
                        IsRead = false,
                        RelatedItemId = item.Id,
                        RelatedItemType = "Inventory"
                    };
                }

                if (notification != null)
                {
                    _context.Notifications.Add(notification);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
