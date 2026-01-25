namespace api.Models
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public NotificationSeverity Severity { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public Guid? RelatedItemId { get; set; }
        public string RelatedItemType { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
    }
}
