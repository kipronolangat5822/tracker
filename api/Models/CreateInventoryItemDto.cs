namespace api.Models
{
    public class CreateInventoryItemDto
{
    public string Name { get; set; }
    public string Category { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; }
    public string Location { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public DateTime PurchaseDate { get; set; }
    
}

}