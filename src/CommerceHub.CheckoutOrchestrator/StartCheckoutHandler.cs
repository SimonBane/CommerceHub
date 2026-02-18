using CommerceHub.Contracts.Basket;
using CommerceHub.Contracts.Inventory;

namespace CommerceHub.CheckoutOrchestrator;

/// <summary>
/// Starts the CheckoutSaga when CheckoutInitiatedV1 is received from BasketService.
/// Publishes ReserveInventoryCommand to inventory-commands.
/// </summary>
public static class StartCheckoutHandler
{
    public static (CheckoutSaga, ReserveInventoryCommand) Handle(CheckoutInitiatedV1 msg)
    {
        var orderId = Guid.CreateVersion7();

        var saga = new CheckoutSaga
        {
            Id = orderId.ToString(),
            BasketId = msg.BasketId,
            CustomerId = msg.CustomerId,
            Items = msg.Items.Select(i => new CheckoutSagaItem
            {
                ProductId = i.ProductId,
                Sku = i.Sku,
                Name = i.Name,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity
            }).ToList(),
            Total = msg.Total,
            CurrencyCode = "USD",
            Step = CheckoutSagaStep.AwaitingReservation
        };

        var reserveCommand = new ReserveInventoryCommand(
            orderId,
            msg.CustomerId,
            msg.Items.Select(i => new ReserveInventoryItemDto(i.ProductId, i.Sku, i.Quantity)).ToList());

        return (saga, reserveCommand);
    }
}
