using System.Net.Http.Json;
using CommerceHub.BuildingBlocks;
using Wolverine.Http;

namespace CommerceHub.BackofficeApi.Features.Orders.ShipOrder;

/// <summary>
/// Ships an order by proxying to OrderingService.
/// </summary>
public static class ShipOrderHandler
{
    [WolverinePost("/backoffice/orders/{orderId:guid}/ship")]
    public static async Task<IResult> Handle(
        Guid orderId,
        ShipOrderRequest request,
        IHttpClientFactory httpClientFactory,
        CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("OrderingService");
        var response = await client.PostAsJsonAsync($"/orders/{orderId}/ship", request, ct);
        if (!response.IsSuccessStatusCode)
            return await response.ForwardErrorResponseAsync(ct);

        return Results.NoContent();
    }
}
