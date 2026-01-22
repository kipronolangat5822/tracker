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
        public ItemStatus Status { get; private set; }
     
    }
}
