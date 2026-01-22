namespace api.Models
{
    public class UpdateInventoryItemDto
{
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    public string Location { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

}