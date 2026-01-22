using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace api.Services
{
    public class DarajaService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DarajaService> _logger;

        public DarajaService(
            IConfiguration configuration, 
            AppDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            ILogger<DarajaService> logger)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

       
        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var consumerKey = _configuration["Daraja:ConsumerKey"];
                var consumerSecret = _configuration["Daraja:ConsumerSecret"];
                var authUrl = _configuration["Daraja:AuthUrl"];

                
                if (string.IsNullOrEmpty(consumerKey) || string.IsNullOrEmpty(consumerSecret))
                {
                    _logger.LogError("Daraja Consumer Key or Consumer Secret is not configured");
                    throw new InvalidOperationException("Daraja credentials are not properly configured");
                }

                _logger.LogInformation("Requesting access token from: {AuthUrl}", authUrl);

                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");

                var response = await client.GetAsync(authUrl);
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Auth Response Status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Auth Response: {Content}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get access token. Status: {StatusCode}, Response: {Content}", 
                        response.StatusCode, content);
                    throw new HttpRequestException($"Failed to authenticate with Daraja API: {content}");
                }

                var authResponse = JsonSerializer.Deserialize<DarajaAuthResponseDto>(content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (string.IsNullOrEmpty(authResponse?.Access_token))
                {
                    _logger.LogError("Access token is empty in response: {Content}", content);
                    throw new InvalidOperationException("Failed to extract access token from response");
                }

                _logger.LogInformation("Successfully obtained access token");
                return authResponse.Access_token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting access token from Daraja API");
                throw;
            }
        }

       
        private string GeneratePassword(string shortCode, string passkey, string timestamp)
        {
            var data = $"{shortCode}{passkey}{timestamp}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        }

        
        private string GenerateTimestamp()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        
        public async Task<StkPushResponseDto?> InitiateStkPushAsync(StkPushRequestDto request)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogError("Access token is empty, cannot proceed with STK Push");
                    throw new InvalidOperationException("Failed to obtain access token");
                }

                var timestamp = GenerateTimestamp();
                var shortCode = _configuration["Daraja:ShortCode"];
                var passkey = _configuration["Daraja:Passkey"];
                var stkPushUrl = _configuration["Daraja:StkPushUrl"];
                var callbackUrl = _configuration["Daraja:CallbackUrl"];

                var password = GeneratePassword(shortCode!, passkey!, timestamp);

                
                var phoneNumber = request.PhoneNumber.TrimStart('0');
                if (!phoneNumber.StartsWith("254"))
                {
                    phoneNumber = "254" + phoneNumber;
                }

                _logger.LogInformation("Initiating STK Push for phone: {Phone}, amount: {Amount}", phoneNumber, request.Amount);

                var stkPushPayload = new
                {
                    BusinessShortCode = shortCode,
                    Password = password,
                    Timestamp = timestamp,
                    TransactionType = "CustomerPayBillOnline",
                    Amount = request.Amount,
                    PartyA = phoneNumber,
                    PartyB = shortCode,
                    PhoneNumber = phoneNumber,
                    CallBackURL = callbackUrl,
                    AccountReference = request.AccountReference,
                    TransactionDesc = request.TransactionDesc
                };

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var jsonPayload = JsonSerializer.Serialize(stkPushPayload);
                _logger.LogInformation("STK Push Payload: {Payload}", jsonPayload);
                
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(stkPushUrl, httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("STK Push Response Status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("STK Push Response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("STK Push failed. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    
                  
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        
                        var errorMessage = errorResponse?.ContainsKey("errorMessage") == true 
                            ? errorResponse["errorMessage"].ToString() 
                            : responseContent;
                        
                        throw new HttpRequestException($"STK Push failed: {errorMessage}");
                    }
                    catch (JsonException)
                    {
                        throw new HttpRequestException($"STK Push failed: {responseContent}");
                    }
                }

                var stkResponse = JsonSerializer.Deserialize<StkPushResponseDto>(responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (stkResponse != null && stkResponse.ResponseCode == "0")
                {
                   
                    var transaction = new MpesaTransaction
                    {
                        MerchantRequestID = stkResponse.MerchantRequestID,
                        CheckoutRequestID = stkResponse.CheckoutRequestID,
                        PhoneNumber = phoneNumber,
                        Amount = request.Amount,
                        AccountReference = request.AccountReference,
                        TransactionDesc = request.TransactionDesc,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    };

                    _dbContext.MpesaTransactions.Add(transaction);
                    await _dbContext.SaveChangesAsync();
                    
                    _logger.LogInformation("Transaction saved successfully. CheckoutRequestID: {CheckoutRequestID}", 
                        stkResponse.CheckoutRequestID);
                }

                return stkResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating STK Push");
                throw;
            }
        }

        
        public async Task<bool> HandleCallbackAsync(StkCallbackDto callback)
        {
            try
            {
                var stkCallback = callback.Body.StkCallback;
                
                var transaction = await _dbContext.MpesaTransactions
                    .FirstOrDefaultAsync(t => t.CheckoutRequestID == stkCallback.CheckoutRequestID);

                if (transaction == null)
                {
                    _logger.LogWarning("Transaction not found for CheckoutRequestID: {CheckoutRequestID}", 
                        stkCallback.CheckoutRequestID);
                    return false;
                }

                transaction.ResultCode = stkCallback.ResultCode;
                transaction.ResultDesc = stkCallback.ResultDesc;
                transaction.UpdatedAt = DateTime.UtcNow;

                if (stkCallback.ResultCode == 0 && stkCallback.CallbackMetadata != null)
                {
                    transaction.Status = "Success";

                    
                    var metadata = stkCallback.CallbackMetadata.Item;
                    transaction.MpesaReceiptNumber = metadata.FirstOrDefault(i => i.Name == "MpesaReceiptNumber")?.Value?.ToString();
                    
                    var transactionDateValue = metadata.FirstOrDefault(i => i.Name == "TransactionDate")?.Value?.ToString();
                    if (!string.IsNullOrEmpty(transactionDateValue) && long.TryParse(transactionDateValue, out long transactionDateLong))
                    {
                        transaction.TransactionDate = DateTime.ParseExact(transactionDateValue, "yyyyMMddHHmmss", null);
                    }
                }
                else
                {
                    transaction.Status = "Failed";
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Callback processed successfully for CheckoutRequestID: {CheckoutRequestID}", 
                    stkCallback.CheckoutRequestID);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling callback");
                return false;
            }
        }

       
        public async Task<QueryResponseDto?> QueryTransactionStatusAsync(string checkoutRequestId)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                var timestamp = GenerateTimestamp();
                var shortCode = _configuration["Daraja:ShortCode"];
                var passkey = _configuration["Daraja:Passkey"];
                var queryUrl = _configuration["Daraja:QueryUrl"];

                var password = GeneratePassword(shortCode!, passkey!, timestamp);

                var queryPayload = new
                {
                    BusinessShortCode = shortCode,
                    Password = password,
                    Timestamp = timestamp,
                    CheckoutRequestID = checkoutRequestId
                };

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var jsonPayload = JsonSerializer.Serialize(queryPayload);
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(queryUrl, httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Query Response: {Response}", responseContent);

                return JsonSerializer.Deserialize<QueryResponseDto>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying transaction status");
                throw;
            }
        }

                public async Task<List<MpesaTransaction>> GetAllTransactionsAsync()
        {
            return await _dbContext.MpesaTransactions
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

      
        public async Task<MpesaTransaction?> GetTransactionByIdAsync(int id)
        {
            return await _dbContext.MpesaTransactions.FindAsync(id);
        }

        
        public async Task<MpesaTransaction?> GetTransactionByCheckoutRequestIdAsync(string checkoutRequestId)
        {
            return await _dbContext.MpesaTransactions
                .FirstOrDefaultAsync(t => t.CheckoutRequestID == checkoutRequestId);
        }

        public async Task<List<MpesaTransaction>> GetTransactionsByPhoneNumberAsync(string phoneNumber)
        {
            return await _dbContext.MpesaTransactions
                .Where(t => t.PhoneNumber.Contains(phoneNumber))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

       
        public async Task<object> TestAuthenticationAsync()
        {
            var consumerKey = _configuration["Daraja:ConsumerKey"];
            var consumerSecret = _configuration["Daraja:ConsumerSecret"];
            var authUrl = _configuration["Daraja:AuthUrl"];

            _logger.LogInformation("Testing Daraja authentication...");
            _logger.LogInformation("Consumer Key: {Key}", string.IsNullOrEmpty(consumerKey) ? "NOT SET" : $"{consumerKey.Substring(0, Math.Min(10, consumerKey.Length))}...");
            _logger.LogInformation("Auth URL: {Url}", authUrl);

            var accessToken = await GetAccessTokenAsync();
            
            return new
            {
                configurationValid = !string.IsNullOrEmpty(consumerKey) && !string.IsNullOrEmpty(consumerSecret),
                authUrl = authUrl,
                tokenReceived = !string.IsNullOrEmpty(accessToken),
                tokenLength = accessToken?.Length ?? 0,
                tokenPreview = accessToken?.Length > 10 ? $"{accessToken.Substring(0, 10)}..." : accessToken
            };
        }
    }
}
