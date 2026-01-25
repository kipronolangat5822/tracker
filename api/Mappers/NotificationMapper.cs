using api.Models;

namespace api.Mappers
{
    public static class NotificationMapper
    {
        public static NotificationDto ToDto(this Notification notification)
        {
            var timeAgo = GetTimeAgo(notification.CreatedDate);

            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Severity = notification.Severity,
                CreatedDate = notification.CreatedDate,
                IsRead = notification.IsRead,
                RelatedItemId = notification.RelatedItemId,
                RelatedItemType = notification.RelatedItemType,
                TimeAgo = timeAgo
            };
        }

        private static string GetTimeAgo(DateTime date)
        {
            var timeSpan = DateTime.UtcNow - date;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            
            return date.ToString("MMM dd, yyyy");
        }
    }
}
