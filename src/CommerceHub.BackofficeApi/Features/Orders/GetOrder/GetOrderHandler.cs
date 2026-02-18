using System.Net.Http.Json;
using CommerceHub.BuildingBlocks;
using Wolverine.Http;

namespace CommerceHub.BackofficeApi.Features.Orders.GetOrder;

/// <summary>
/// Gets order details by proxying to OrderingService.
/// </summary>
public static class GetOrderHandler
{
    [WolverineGet("/backoffice/orders/{orderId:guid}")]
    public static async Task<IResult> Handle(
        Guid orderId,
        IHttpClientFactory httpClientFactory,
        CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("OrderingService");
        var response = await client.GetAsync($"/orders/{orderId}", ct);
        if (!response.IsSuccessStatusCode)
            return await response.ForwardErrorResponseAsync(ct);

        var result = await response.Content.ReadFromJsonAsync<GetOrderResponse>(ct);
        return Results.Ok(result);
    }
}
