using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Endpoints
{
    public static class NotificationEndpoints
    {
        public static void MapNotificationEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/notifications");

            group.MapGet("/", async (
                NotificationService service,
                [FromQuery] bool unreadOnly = false,
                [FromQuery] string? searchTerm = null,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var queryParams = new QueryParameters
                {
                    SearchTerm = searchTerm,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                var result = await service.GetAllAsync(queryParams, unreadOnly);
                return Results.Ok(result);
            }).WithName("GetNotifications");

            group.MapGet("/count", async (NotificationService service) =>
            {
                var count = await service.GetUnreadCountAsync();
                return Results.Ok(new { count });
            }).WithName("GetNotificationCount");

            group.MapPost("/generate", async (NotificationService service) =>
            {
                await service.GenerateInventoryExpiryNotificationsAsync();
                return Results.Ok(new { message = "Notifications generated successfully" });
            }).WithName("GenerateNotifications");

            group.MapPatch("/{id:guid}/read", async (Guid id, NotificationService service) =>
            {
                var notification = await service.MarkAsReadAsync(id);
                return notification != null ? Results.Ok(notification) : Results.NotFound();
            }).WithName("MarkNotificationAsRead");

            group.MapPost("/mark-all-read", async (NotificationService service) =>
            {
                await service.MarkAllAsReadAsync();
                return Results.Ok(new { message = "All notifications marked as read" });
            }).WithName("MarkAllNotificationsAsRead");

            group.MapDelete("/{id:guid}", async (Guid id, NotificationService service) =>
            {
                var deleted = await service.DeleteAsync(id);
                return deleted ? Results.NoContent() : Results.NotFound();
            }).WithName("DeleteNotification");
            group.WithOpenApi().WithTags("Notifications");
        }
    }
}
