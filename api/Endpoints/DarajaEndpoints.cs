using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Endpoints
{
    public static class DarajaEndpoints
    {
        public static void MapDarajaEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/daraja");

            
            group.MapGet("/test-auth", async (DarajaService darajaService) =>
            {
                try
                {
                    
                    var testResult = await darajaService.TestAuthenticationAsync();
                    return Results.Ok(new { success = true, message = "Authentication successful", details = testResult });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { success = false, error = ex.Message, stackTrace = ex.ToString() });
                }
            })
            .WithName("TestAuthentication")
            .WithOpenApi();

            
            group.MapPost("/stk-push", async ([FromBody] StkPushRequestDto request, DarajaService darajaService) =>
            {
                try
                {
                    var result = await darajaService.InitiateStkPushAsync(request);
                    return result is not null ? Results.Ok(result) : Results.BadRequest("Failed to initiate payment");
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("InitiateStkPush")
            .WithOpenApi();

          
            group.MapPost("/callback", async ([FromBody] StkCallbackDto callback, DarajaService darajaService) =>
            {
                try
                {
                    var result = await darajaService.HandleCallbackAsync(callback);
                    return result ? Results.Ok("Callback processed") : Results.BadRequest("Failed to process callback");
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("DarajaCallback")
            .WithOpenApi();

           
            group.MapPost("/query", async ([FromBody] QueryRequestDto request, DarajaService darajaService) =>
            {
                try
                {
                    var result = await darajaService.QueryTransactionStatusAsync(request.CheckoutRequestID);
                    return result is not null ? Results.Ok(result) : Results.NotFound("Transaction not found");
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("QueryTransaction")
            .WithOpenApi();

           
            group.MapGet("/transactions", async (DarajaService darajaService) =>
            {
                var transactions = await darajaService.GetAllTransactionsAsync();
                return Results.Ok(transactions);
            })
            .WithName("GetAllTransactions")
            .WithOpenApi();

           
            group.MapGet("/transactions/{id:int}", async (int id, DarajaService darajaService) =>
            {
                var transaction = await darajaService.GetTransactionByIdAsync(id);
                return transaction is not null ? Results.Ok(transaction) : Results.NotFound("Transaction not found");
            })
            .WithName("GetTransactionById")
            .WithOpenApi();

            
            group.MapGet("/transactions/checkout/{checkoutRequestId}", async (string checkoutRequestId, DarajaService darajaService) =>
            {
                var transaction = await darajaService.GetTransactionByCheckoutRequestIdAsync(checkoutRequestId);
                return transaction is not null ? Results.Ok(transaction) : Results.NotFound("Transaction not found");
            })
            .WithName("GetTransactionByCheckoutRequestId")
            .WithOpenApi();

            
            group.MapGet("/transactions/phone/{phoneNumber}", async (string phoneNumber, DarajaService darajaService) =>
            {
                var transactions = await darajaService.GetTransactionsByPhoneNumberAsync(phoneNumber);
                return Results.Ok(transactions);
            })
            .WithName("GetTransactionsByPhoneNumber");
            group.WithTags("Payments");
        }
    }
}
