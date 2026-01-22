namespace api.Models
{
 
    public class StkPushRequestDto
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AccountReference { get; set; } = string.Empty;
        public string TransactionDesc { get; set; } = string.Empty;
    }

    
    public class StkPushResponseDto
    {
        public string MerchantRequestID { get; set; } = string.Empty;
        public string CheckoutRequestID { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string ResponseDescription { get; set; } = string.Empty;
        public string CustomerMessage { get; set; } = string.Empty;
    }

   
    public class StkCallbackDto
    {
        public StkCallbackBody Body { get; set; } = new();
    }

    public class StkCallbackBody
    {
        public StkCallback StkCallback { get; set; } = new();
    }

    public class StkCallback
    {
        public string MerchantRequestID { get; set; } = string.Empty;
        public string CheckoutRequestID { get; set; } = string.Empty;
        public int ResultCode { get; set; }
        public string ResultDesc { get; set; } = string.Empty;
        public CallbackMetadata? CallbackMetadata { get; set; }
    }

    public class CallbackMetadata
    {
        public List<CallbackItem> Item { get; set; } = new();
    }

    public class CallbackItem
    {
        public string Name { get; set; } = string.Empty;
        public object? Value { get; set; }
    }

    public class QueryRequestDto
    {
        public string CheckoutRequestID { get; set; } = string.Empty;
    }

    
    public class QueryResponseDto
    {
        public string ResponseCode { get; set; } = string.Empty;
        public string ResponseDescription { get; set; } = string.Empty;
        public string MerchantRequestID { get; set; } = string.Empty;
        public string CheckoutRequestID { get; set; } = string.Empty;
        public string ResultCode { get; set; } = string.Empty;
        public string ResultDesc { get; set; } = string.Empty;
    }

  
    public class DarajaAuthResponseDto
    {
        public string Access_token { get; set; } = string.Empty;
        public string Expires_in { get; set; } = string.Empty;
    }
}
