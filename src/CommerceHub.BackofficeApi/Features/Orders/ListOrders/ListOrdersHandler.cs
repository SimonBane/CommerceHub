using System.Net.Http.Json;
using CommerceHub.BuildingBlocks;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace CommerceHub.BackofficeApi.Features.Orders.ListOrders;

/// <summary>
/// Lists orders by proxying to OrderingService.
/// </summary>
public static class ListOrdersHandler
{
    [WolverineGet("/backoffice/orders")]
    public static async Task<IResult> Handle(
        [FromQuery] ListOrdersQuery query,
        IHttpClientFactory httpClientFactory,
        CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("OrderingService");
        var queryString = $"?Page={query.Page}&PageSize={query.PageSize}";
        if (query.CustomerId.HasValue)
            queryString += $"&CustomerId={query.CustomerId.Value}";
        if (!string.IsNullOrWhiteSpace(query.Status))
            queryString += $"&Status={query.Status}";

        var response = await client.GetAsync($"/orders{queryString}", ct);
        if (!response.IsSuccessStatusCode)
            return await response.ForwardErrorResponseAsync(ct);

        var result = await response.Content.ReadFromJsonAsync<ListOrdersResponse>(ct);
        return Results.Ok(result);
    }
}
