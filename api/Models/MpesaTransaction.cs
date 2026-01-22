using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class MpesaTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MerchantRequestID { get; set; } = string.Empty;

        [Required]
        public string CheckoutRequestID { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        public string AccountReference { get; set; } = string.Empty;
        
        public string TransactionDesc { get; set; } = string.Empty;

        public string? MpesaReceiptNumber { get; set; }

        public DateTime? TransactionDate { get; set; }

        public int ResultCode { get; set; }

        public string ResultDesc { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending, Success, Failed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
