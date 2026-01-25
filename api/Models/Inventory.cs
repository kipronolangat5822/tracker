namespace api.Models
{
    public class Inventory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        
        public ItemStatus Status
        {
            get
            {
                if (!ExpiryDate.HasValue)
                    return ItemStatus.FRESH;

                var daysUntilExpiry = (ExpiryDate.Value.Date - DateTime.UtcNow.Date).Days;

                if (daysUntilExpiry < 0)
                    return ItemStatus.EXPIRED;
                else if (daysUntilExpiry <= 7)
                    return ItemStatus.EXPIRING_SOON;
                else
                    return ItemStatus.FRESH;
            }
        }
    }
}
