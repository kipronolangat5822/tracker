namespace api.Models
{
    public class InventoryItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    public string Location { get; set; }

    public DateTime? ExpiryDate { get; set; }
    public ItemStatus Status { get; set; }

    public int? DaysUntilExpiry { get; set; }
}

}