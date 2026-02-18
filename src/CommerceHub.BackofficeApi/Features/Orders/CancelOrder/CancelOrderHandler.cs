using System.Net.Http.Json;
using CommerceHub.BuildingBlocks;
using Wolverine.Http;

namespace CommerceHub.BackofficeApi.Features.Orders.CancelOrder;

/// <summary>
/// Cancels an order by proxying to OrderingService.
/// </summary>
public static class CancelOrderHandler
{
    [WolverinePost("/backoffice/orders/{orderId:guid}/cancel")]
    public static async Task<IResult> Handle(
        Guid orderId,
        CancelOrderRequest request,
        IHttpClientFactory httpClientFactory,
        CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("OrderingService");
        var response = await client.PostAsJsonAsync($"/orders/{orderId}/cancel", request, ct);
        if (!response.IsSuccessStatusCode)
            return await response.ForwardErrorResponseAsync(ct);

        return Results.NoContent();
    }
}
