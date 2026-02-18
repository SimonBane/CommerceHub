using System.Net.Http.Json;
using CommerceHub.BuildingBlocks;
using Wolverine.Http;

namespace CommerceHub.BackofficeApi.Features.Catalog.CreateProduct;

/// <summary>
/// Creates a product by proxying to CatalogService.
/// </summary>
public static class CreateProductHandler
{
    [WolverinePost("/backoffice/products")]
    public static async Task<IResult> Handle(
        CreateProductRequest request,
        IHttpClientFactory httpClientFactory,
        CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("CatalogService");
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Sku,
            request.CategoryId,
            request.BasePrice,
            request.ImageUrl);

        var response = await client.PostAsJsonAsync("/catalog/products", command, ct);
        if (!response.IsSuccessStatusCode)
            return await response.ForwardErrorResponseAsync(ct);

        var result = await response.Content.ReadFromJsonAsync<CreateProductResponse>(ct);
        return Results.Created($"/backoffice/products/{result?.Id}", result);
    }
}
